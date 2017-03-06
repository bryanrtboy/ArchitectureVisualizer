//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

using UnityEngine;

namespace Phonon
{
    //
    // PhononEffect
    // Enables physics-based modeling for any object.
    //

    [AddComponentMenu("Phonon/Phonon Effect")]
    public class PhononEffect : MonoBehaviour
    {
        //
        // Initializes the effect.
        //
        void Start()
        {
            audioEngine = AudioEngineComponent.GetAudioEngine();
            if (audioEngine == AudioEngine.Unity)
            {
                // Setup 3D audio and panning effect for direct sound.
                GlobalContext globalContext = PhononSettings.GetGlobalContext();
                RenderingSettings renderingSettings = PhononSettings.GetRenderingSettings();

                directAttnInterlop.Init(directAttnInteropFrames);
                inputFormat = PhononSettings.GetAudioConfiguration();
                outputFormat = PhononSettings.GetAudioConfiguration();

                if (PhononCore.iplCreateBinauralRenderer(globalContext, renderingSettings, null, ref binauralRenderer) != Error.None)
                {
                    Debug.Log("Unable to create binaural renderer for object: " + gameObject.name + ". Please check the log file for details.");
                    return;
                }

                if (outputFormat.channelLayout == ChannelLayout.Stereo)
                {
                    // Create object based binaural effect for direct sound if the output format is stereo.
                    if (PhononCore.iplCreateBinauralEffect(binauralRenderer, inputFormat, outputFormat, ref directBinauralEffect) != Error.None)
                    {
                        Debug.Log("Unable to create 3D effect for object: " + gameObject.name + ". Please check the log file for details.");
                        return;
                    }
                }

                if (outputFormat.channelLayout == ChannelLayout.Custom)
                {
                    // Panning effect for direct sound (used for rendering only for custom speaker layout, otherwise use default Unity panning)
                    if (PhononCore.iplCreatePanningEffect(binauralRenderer, inputFormat, outputFormat, ref directCustomPanningEffect) != Error.None)
                    {
                        Debug.Log("Unable to create custom panning effect for object: " + gameObject.name + ". Please check the log file for details.");
                        return;
                    }
                }

                PropagationSettings simulationSettings = EnvironmentComponent.SimulationSettings();
                ambisonicsFormat.channelLayoutType = ChannelLayoutType.Ambisonics;
                ambisonicsFormat.ambisonicsOrder = simulationSettings.ambisonicsOrder;
                ambisonicsFormat.numSpeakers = (ambisonicsFormat.ambisonicsOrder + 1) * (ambisonicsFormat.ambisonicsOrder + 1);
                ambisonicsFormat.ambisonicsOrdering = AmbisonicsOrdering.ACN;
                ambisonicsFormat.ambisonicsNormalization = AmbisonicsNormalization.N3D;
                ambisonicsFormat.channelOrder = ChannelOrder.Deinterleaved;

                AudioFormat ambisonicsBinauralFormat = outputFormat;
                ambisonicsBinauralFormat.channelOrder = ChannelOrder.Deinterleaved;

#if !UNITY_ANDROID
                if (PhononCore.iplCreateAmbisonicsPanningEffect(binauralRenderer, ambisonicsFormat, ambisonicsBinauralFormat, ref propagationPanningEffect) != Error.None)
                {
                    Debug.Log("Unable to create Ambisonics panning effect for object: " + gameObject.name + ". Please check the log file for details.");
                    return;
                }

                if (outputFormat.channelLayout == ChannelLayout.Stereo)
                {
                    // Create ambisonics based binaural effect for indirect sound if the output format is stereo.
                    if (PhononCore.iplCreateAmbisonicsBinauralEffect(binauralRenderer, ambisonicsFormat, ambisonicsBinauralFormat, ref propagationBinauralEffect) != Error.None)
                    {
                        Debug.Log("Unable to create propagation binaural effect for object: " + gameObject.name + ". Please check the log file for details.");
                        return;
                    }
                }
#endif

                wetData = new float[renderingSettings.frameSize * outputFormat.numSpeakers];

                wetAmbisonicsDataMarshal = new IntPtr[ambisonicsFormat.numSpeakers];
                for (int i = 0; i < ambisonicsFormat.numSpeakers; ++i)
                    wetAmbisonicsDataMarshal[i] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * renderingSettings.frameSize);

                wetDataMarshal = new IntPtr[outputFormat.numSpeakers];
                for (int i = 0; i < outputFormat.numSpeakers; ++i)
                    wetDataMarshal[i] = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(float)) * renderingSettings.frameSize);

                listener = FindObjectOfType<AudioListener>();
                phononMixer = FindObjectOfType<PhononMixer>();
                StartCoroutine(EndOfFrameUpdate());

                environmentalRenderer = FindObjectOfType<EnvironmentalRendererComponent>();
                environment = FindObjectOfType<EnvironmentComponent>();
                if ((environmentalRenderer == null || environment == null) && (enableReflections || directOcclusionOption != OcclusionOption.None))
                    Debug.LogError("Environment and Environmental Renderer component must be attached when reflections are enabled or direct occlusion is set to Raycast or Partial.");
            }
        }

        //
        // Destroys the effect.
        //
        void OnDestroy()
        {
            mutex.WaitOne();
            destroying = true;
            errorLogged = false;

            if (audioEngine == AudioEngine.Unity)
            {
                directAttnInterlop.Reset();

#if !UNITY_ANDROID
                PhononCore.iplDestroyConvolutionEffect(ref propagationAmbisonicsEffect);
                propagationAmbisonicsEffect = IntPtr.Zero;

                PhononCore.iplDestroyAmbisonicsBinauralEffect(ref propagationBinauralEffect);
                propagationBinauralEffect = IntPtr.Zero;

                PhononCore.iplDestroyAmbisonicsPanningEffect(ref propagationPanningEffect);
                propagationPanningEffect = IntPtr.Zero;
#endif

                PhononCore.iplDestroyBinauralEffect(ref directBinauralEffect);
                directBinauralEffect = IntPtr.Zero;

                PhononCore.iplDestroyPanningEffect(ref directCustomPanningEffect);
                directCustomPanningEffect = IntPtr.Zero;

                wetData = null;

                if (wetDataMarshal != null)
                    for (int i = 0; i < outputFormat.numSpeakers; ++i)
                        Marshal.FreeCoTaskMem(wetDataMarshal[i]);
                wetDataMarshal = null;

                if (wetAmbisonicsDataMarshal != null)
                    for (int i = 0; i < ambisonicsFormat.numSpeakers; ++i)
                        Marshal.FreeCoTaskMem(wetAmbisonicsDataMarshal[i]);
                wetAmbisonicsDataMarshal = null;
            }

            mutex.ReleaseMutex();
        }

        //
        // Courutine to update source and listener position and orientation at frame end.
        // Done this way to ensure correct update in VR setup.
        //
        private IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                if (!initialized && propagationAmbisonicsEffect == IntPtr.Zero &&
                    environmentalRenderer != null && environmentalRenderer.GetEnvironmentalRenderer() != IntPtr.Zero && environment != null && environment.Environment().GetEnvironment() != IntPtr.Zero)
                {
                    // Check for Baked Source or Baked Reverb component.
                    string effectName = "";
                    if (simulationMode == SimulationType.Baked)
                    {
                        BakedSource bakedSource = GetComponent<BakedSource>();
                        BakedReverb bakedReverb = GetComponent<BakedReverb>();
                        if (bakedSource != null)
                            if (bakedSource.useBakedStaticListener)
                                effectName = "__staticlistener__" + bakedSource.bakedListenerIdentifier;
                            else
                                effectName = bakedSource.uniqueIdentifier;
                        if (bakedReverb != null)
                            effectName = "__reverb__";
                    }

#if !UNITY_ANDROID
                    if (PhononCore.iplCreateConvolutionEffect(environmentalRenderer.GetEnvironmentalRenderer(),
                        effectName, simulationMode, inputFormat, ambisonicsFormat, ref propagationAmbisonicsEffect) != Error.None)
                    {
                        Debug.LogError("Unable to create propagation effect for object: " + gameObject.name);
                    }
#endif

                    initialized = true;
                }

                if (!errorLogged && environment != null && environment.Scene().GetScene() == IntPtr.Zero && ((directOcclusionOption != OcclusionOption.None) || enableReflections))
                {
                    Debug.LogError("Scene not found. Make sure to pre-export the scene.");
                    errorLogged = true;
                }

                if (!initialized && (directOcclusionOption == OcclusionOption.None) && !enableReflections)
                    initialized = true;

                UpdateRelativeDirection();
                if (phononMixer && phononMixer.enabled)
                    fourierMixingEnabled = true;
                else
                    fourierMixingEnabled = false;
                yield return new WaitForEndOfFrame();   // Must yield after updating the relative direction.
            }
        }

        //
        // Updates the direction of the source relative to the listener.
        // Wait until the end of the frame to update the position to get latest information.
        //
        private void UpdateRelativeDirection()
        {
            sourcePosition = Common.ConvertVector(transform.position);
            listenerPosition = Common.ConvertVector(listener.transform.position);
            listenerAhead = Common.ConvertVector(listener.transform.forward);
            listenerUp = Common.ConvertVector(listener.transform.up);

            IntPtr envRenderer = (environmentalRenderer != null) ? environmentalRenderer.GetEnvironmentalRenderer() : IntPtr.Zero;
            directSoundPath = PhononCore.iplGetDirectSoundPath(envRenderer, listenerPosition, listenerAhead, listenerUp, sourcePosition, 
                partialOcclusionRadius, directOcclusionOption);
        }

        //
        // Helper function to change the name of the BakedSource or BakedStaticListener
        // used by the effect.
        //
        public void UpdateEffectName(string effectName)
        {
            if (propagationAmbisonicsEffect != IntPtr.Zero)
                PhononCore.iplSetConvolutionEffectName(propagationAmbisonicsEffect, effectName);
        }

        //
        // Applies the Phonon 3D effect to dry audio.
        //
        void OnAudioFilterRead(float[] data, int channels)
        {
            mutex.WaitOne();

            if (!initialized || destroying)
            {
                mutex.ReleaseMutex();
                Array.Clear(data, 0, data.Length);
                return;
            }

            if (data == null)
            {
                mutex.ReleaseMutex();
                Array.Clear(data, 0, data.Length);
                return;
            }

            float distanceAttenuation = (physicsBasedAttenuation) ? directSoundPath.distanceAttenuation : 1f;

            directAttnInterlop.Set(directSoundPath.occlusionFactor * directMixFraction);
            float occlusionFactor = directAttnInterlop.Update();
            Vector3 directDirection = directSoundPath.direction;

            AudioBuffer inputBuffer;
            inputBuffer.audioFormat = inputFormat;
            inputBuffer.numSamples = data.Length / channels;
            inputBuffer.deInterleavedBuffer = null;
            inputBuffer.interleavedBuffer = data;

            AudioBuffer outputBuffer;
            outputBuffer.audioFormat = outputFormat;
            outputBuffer.numSamples = data.Length / channels;
            outputBuffer.deInterleavedBuffer = null;
            outputBuffer.interleavedBuffer = data;

            // Input data is sent (where it is copied) for indirect propagation effect processing.
            // This data must be sent before applying any other effect to the input audio.
#if !UNITY_ANDROID
            if (enableReflections && (wetData != null) && (wetDataMarshal != null) && (wetAmbisonicsDataMarshal != null) && (propagationAmbisonicsEffect != IntPtr.Zero))
            {
                for (int i = 0; i < data.Length; ++i)
                    wetData[i] = data[i] * indirectMixFraction;

                AudioBuffer propagationInputBuffer;
                propagationInputBuffer.audioFormat = inputFormat;
                propagationInputBuffer.numSamples = wetData.Length / channels;
                propagationInputBuffer.deInterleavedBuffer = null;
                propagationInputBuffer.interleavedBuffer = wetData;

                PhononCore.iplSetDryAudioForConvolutionEffect(propagationAmbisonicsEffect, sourcePosition, propagationInputBuffer);
            }
#endif

            if ((outputFormat.channelLayout == ChannelLayout.Stereo) && directBinauralEnabled)
            {
                // Apply binaural audio to direct sound.
                PhononCore.iplApplyBinauralEffect(directBinauralEffect, inputBuffer, directDirection, hrtfInterpolation, outputBuffer);
            }
            else if (outputFormat.channelLayout == ChannelLayout.Custom)
            {
                // Apply panning fo custom speaker layout.
                PhononCore.iplApplyPanningEffect(directCustomPanningEffect, inputBuffer, directDirection, outputBuffer);
            }

            // Process direct sound occlusion
            for (int i = 0; i < data.Length; ++i)
                data[i] *= occlusionFactor * distanceAttenuation;

#if !UNITY_ANDROID
            if (enableReflections && (wetData != null) && (wetDataMarshal != null) && (wetAmbisonicsDataMarshal != null) && (propagationAmbisonicsEffect != IntPtr.Zero))
            {
                if (fourierMixingEnabled)
                {
                    phononMixer.processMixedAudio = true;
                    mutex.ReleaseMutex();
                    return;
                }

                AudioBuffer wetAmbisonicsBuffer;
                wetAmbisonicsBuffer.audioFormat = ambisonicsFormat;
                wetAmbisonicsBuffer.numSamples = data.Length / channels;
                wetAmbisonicsBuffer.deInterleavedBuffer = wetAmbisonicsDataMarshal;
                wetAmbisonicsBuffer.interleavedBuffer = null;
                PhononCore.iplGetWetAudioForConvolutionEffect(propagationAmbisonicsEffect, listenerPosition, listenerAhead, listenerUp, wetAmbisonicsBuffer);

                AudioBuffer wetBufferMarshal;
                wetBufferMarshal.audioFormat = outputFormat;
                wetBufferMarshal.audioFormat.channelOrder = ChannelOrder.Deinterleaved;     // Set format to deinterleave.
                wetBufferMarshal.numSamples = data.Length / channels;
                wetBufferMarshal.deInterleavedBuffer = wetDataMarshal;
                wetBufferMarshal.interleavedBuffer = null;

                if ((outputFormat.channelLayout == ChannelLayout.Stereo) && indirectBinauralEnabled)
                    PhononCore.iplApplyAmbisonicsBinauralEffect(propagationBinauralEffect, wetAmbisonicsBuffer, wetBufferMarshal);
                else
                    PhononCore.iplApplyAmbisonicsPanningEffect(propagationPanningEffect, wetAmbisonicsBuffer, wetBufferMarshal);

                AudioBuffer wetBuffer;
                wetBuffer.audioFormat = outputFormat;
                wetBuffer.numSamples = data.Length / channels;
                wetBuffer.deInterleavedBuffer = null;
                wetBuffer.interleavedBuffer = wetData;
                PhononCore.iplInterleaveAudioBuffer(wetBufferMarshal, wetBuffer);

                for (int i = 0; i < data.Length; ++i)
                    data[i] += wetData[i];
            }
#endif

            mutex.ReleaseMutex();
        }

        // Data members.
        public bool directBinauralEnabled = true;
        public HRTFInterpolation hrtfInterpolation;
        public OcclusionOption directOcclusionOption;
        [Range(.1f, 32f)]
        public float partialOcclusionRadius = 1.0f;
        public bool physicsBasedAttenuation = true;
        [Range(.0f, 1.0f)]
        public float directMixFraction = 1.0f;

        public bool enableReflections = false;
        public SimulationType simulationMode;
        [Range(.0f, 10.0f)]
        public float indirectMixFraction = 1.0f;
        public bool indirectBinauralEnabled = false;
        //public bool diffractionEnabled = false;

        // Private fields.
        AudioEngine audioEngine;
        AudioListener listener;
        PhononMixer phononMixer;

        IntPtr binauralRenderer = IntPtr.Zero;

        IntPtr directBinauralEffect = IntPtr.Zero;
        IntPtr directCustomPanningEffect = IntPtr.Zero;
        IntPtr propagationAmbisonicsEffect = IntPtr.Zero;
        IntPtr propagationPanningEffect = IntPtr.Zero;
        IntPtr propagationBinauralEffect = IntPtr.Zero;

        AttenuationInterpolator directAttnInterlop = new AttenuationInterpolator();
        int directAttnInteropFrames = 4;

        AudioFormat inputFormat;
        AudioFormat outputFormat;
        AudioFormat ambisonicsFormat;

        DirectSoundPath directSoundPath;
        Vector3 sourcePosition;
        Vector3 listenerPosition;
        Vector3 listenerAhead;
        Vector3 listenerUp;

        bool fourierMixingEnabled;

        float[] wetData = null;
        IntPtr[] wetDataMarshal = null;
        IntPtr[] wetAmbisonicsDataMarshal = null;

        Mutex mutex = new Mutex();
        bool initialized = false;
        bool destroying = false;

        EnvironmentalRendererComponent environmentalRenderer;
        EnvironmentComponent environment;
        bool errorLogged = false;
    }
}
