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
    // PhononMixer
    // Represents a Phonon Mixer. Performs optimized mixing in fourier
    // domain.
    //

    [AddComponentMenu("Phonon/Phonon Mixer")]
    public class PhononMixer : MonoBehaviour
    {
        //
        // Initializes the listener.
        //
        void Start()
        {
            audioEngine = AudioEngineComponent.GetAudioEngine();
            if (audioEngine == AudioEngine.Unity)
            {
                GlobalContext globalContext = PhononSettings.GetGlobalContext();
                PropagationSettings simulationSettings = EnvironmentComponent.SimulationSettings();
                RenderingSettings renderingSettings = PhononSettings.GetRenderingSettings();

                ambisonicsFormat.channelLayoutType = ChannelLayoutType.Ambisonics;
                ambisonicsFormat.ambisonicsOrder = simulationSettings.ambisonicsOrder;
                ambisonicsFormat.numSpeakers = (ambisonicsFormat.ambisonicsOrder + 1) * (ambisonicsFormat.ambisonicsOrder + 1);
                ambisonicsFormat.ambisonicsOrdering = AmbisonicsOrdering.ACN;
                ambisonicsFormat.ambisonicsNormalization = AmbisonicsNormalization.N3D;
                ambisonicsFormat.channelOrder = ChannelOrder.Deinterleaved;

                if (PhononCore.iplCreateBinauralRenderer(globalContext, renderingSettings, null, ref binauralRenderer) != Error.None)
                {
                    Debug.Log("Unable to create binaural renderer for object: " + gameObject.name + ". Please check the log file for details.");
                    return;
                }

                outputFormat = PhononSettings.GetAudioConfiguration();

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

                StartCoroutine(EndOfFrameUpdate());

                environmentalRendererComponent = FindObjectOfType<EnvironmentalRendererComponent>();
            }
        }

        //
        // Destroys the listener
        //
        void OnDestroy()
        {
            mutex.WaitOne();
            destroying = true;

            if (audioEngine == AudioEngine.Unity)
            {
                if (environmentRenderer != IntPtr.Zero)
                    PhononCore.iplDestroyEnvironmentalRenderer(ref environmentRenderer);

#if !UNITY_ANDROID
                PhononCore.iplDestroyAmbisonicsBinauralEffect(ref propagationBinauralEffect);
                PhononCore.iplDestroyAmbisonicsPanningEffect(ref propagationPanningEffect);
#endif

                PhononCore.iplDestroyBinauralEffect(ref binauralRenderer);

                environmentRenderer = IntPtr.Zero;

                propagationBinauralEffect = IntPtr.Zero;
                binauralRenderer = IntPtr.Zero;

                wetData = null;

                for (int i = 0; i < outputFormat.numSpeakers; ++i)
                    Marshal.FreeCoTaskMem(wetDataMarshal[i]);
                wetDataMarshal = null;

                for (int i = 0; i < ambisonicsFormat.numSpeakers; ++i)
                    Marshal.FreeCoTaskMem(wetAmbisonicsDataMarshal[i]);
                wetAmbisonicsDataMarshal = null;
            }

            mutex.ReleaseMutex();
        }

        //
        // Courutine to update listener position and orientation at frame end.
        // Done this way to ensure correct update in VR setup.
        //
        private IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                if (!initialized && environmentalRendererComponent != null)
                {
                    environmentRenderer = environmentalRendererComponent.GetEnvironmentalRenderer();
                    if (environmentRenderer != IntPtr.Zero)
                    {
                        environmentalRendererComponent.SetUsedByMixer();
                        initialized = true;
                    }
                }

                listenerPosition = Common.ConvertVector(transform.position);
                listenerAhead = Common.ConvertVector(transform.forward);
                listenerUp = Common.ConvertVector(transform.up);
                yield return new WaitForEndOfFrame();
            }
        }

        //
        // Applies the Phonon effect to audio.
        //
        void OnAudioFilterRead(float[] data, int channels)
        {
            mutex.WaitOne();

            if (!initialized || destroying)
            {
                mutex.ReleaseMutex();
                return;
            }

            if ((data == null) || (environmentRenderer == IntPtr.Zero) || (!processMixedAudio) || (wetData == null)
                || (wetAmbisonicsDataMarshal == null))
            {
                mutex.ReleaseMutex();
                return;
            }

#if !UNITY_ANDROID
            AudioBuffer wetAmbisonicsBuffer;
            wetAmbisonicsBuffer.audioFormat = ambisonicsFormat;
            wetAmbisonicsBuffer.numSamples = data.Length / channels;
            wetAmbisonicsBuffer.deInterleavedBuffer = wetAmbisonicsDataMarshal;
            wetAmbisonicsBuffer.interleavedBuffer = null;
            PhononCore.iplGetMixedEnvironmentalAudio(environmentRenderer, listenerPosition, listenerAhead, listenerUp, wetAmbisonicsBuffer);

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
#endif

            for (int i = 0; i < data.Length; ++i)
                data[i] += wetData[i];

            mutex.ReleaseMutex();
        }

        // Public members.
        public bool processMixedAudio;
        public bool indirectBinauralEnabled = false;

        // Private members.
        AudioEngine audioEngine;

        //IntPtr environment = IntPtr.Zero;
        IntPtr environmentRenderer = IntPtr.Zero;
        IntPtr propagationPanningEffect = IntPtr.Zero;
        IntPtr propagationBinauralEffect = IntPtr.Zero;
        IntPtr binauralRenderer = IntPtr.Zero;

        EnvironmentalRendererComponent environmentalRendererComponent;

        AudioFormat outputFormat;
        AudioFormat ambisonicsFormat;

        Vector3 listenerPosition;
        Vector3 listenerAhead;
        Vector3 listenerUp;

        float[] wetData = null;
        IntPtr[] wetDataMarshal = null;
        IntPtr[] wetAmbisonicsDataMarshal = null;

        Mutex mutex = new Mutex();
        bool initialized = false;
        bool destroying = false;
    }
}