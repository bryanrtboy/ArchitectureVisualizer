//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using UnityEngine;

namespace Phonon
{

    //
    //	PhononSettings
    //  Class to query various Phonon settings.
    //

    public class PhononSettings : MonoBehaviour
    {
        //
        // Get Global context.
        //
        static public GlobalContext GetGlobalContext()
        {
            GlobalContext globalContext;
            globalContext.logCallback = IntPtr.Zero;
            globalContext.allocateCallback = IntPtr.Zero;
            globalContext.freeCallback = IntPtr.Zero;

            return globalContext;
        }

        //
        // Get DSP parameters.
        //
        static public RenderingSettings GetRenderingSettings()
        {
            int numBuffers, frameSize;
            RenderingSettings renderingSettings;

            AudioSettings.GetDSPBufferSize(out frameSize, out numBuffers);
            renderingSettings.samplingRate = AudioSettings.outputSampleRate;
            renderingSettings.frameSize = frameSize;
            renderingSettings.convolutionOption = GetConvolutionOption();

            return renderingSettings;
        }

        //
        // Get Audio Configuration.
        //
        static public AudioFormat GetAudioConfiguration()
        {
            AudioSpeakerMode projectSpeakerMode = AudioSettings.GetConfiguration().speakerMode;
            AudioSpeakerMode driverSpeakerMode = AudioSettings.driverCapabilities;
            AudioSpeakerMode minSpeakerMode;

            // NOTE: Prologic mode is not supported. Revert to stereo.
            if ((projectSpeakerMode == AudioSpeakerMode.Prologic) && (driverSpeakerMode == AudioSpeakerMode.Prologic))
                minSpeakerMode = AudioSpeakerMode.Stereo;
            else
                minSpeakerMode = (projectSpeakerMode < driverSpeakerMode) ? projectSpeakerMode : driverSpeakerMode;

            AudioFormat audioFormat;
            audioFormat.channelLayoutType = ChannelLayoutType.Speakers;
            audioFormat.speakerDirections = null;
            audioFormat.ambisonicsOrder = -1;
            audioFormat.ambisonicsOrdering = AmbisonicsOrdering.ACN;
            audioFormat.ambisonicsNormalization = AmbisonicsNormalization.N3D;
            audioFormat.channelOrder = ChannelOrder.Interleaved;

            switch (minSpeakerMode)
            {
                case AudioSpeakerMode.Mono:
                    audioFormat.channelLayout = ChannelLayout.Mono;
                    audioFormat.numSpeakers = 1;
                    break;
                case AudioSpeakerMode.Stereo:
                    audioFormat.channelLayout = ChannelLayout.Stereo;
                    audioFormat.numSpeakers = 2;
                    break;
                case AudioSpeakerMode.Quad:
                    audioFormat.channelLayout = ChannelLayout.Quadraphonic;
                    audioFormat.numSpeakers = 4;
                    break;
                case AudioSpeakerMode.Mode5point1:
                    audioFormat.channelLayout = ChannelLayout.FivePointOne;
                    audioFormat.numSpeakers = 6;
                    break;
                case AudioSpeakerMode.Mode7point1:
                    audioFormat.channelLayout = ChannelLayout.SevenPointOne;
                    audioFormat.numSpeakers = 8;
                    break;
                default:
                    Debug.LogWarning("Surround and Prologic mode is not supported. Revert to stereo");
                    audioFormat.channelLayout = ChannelLayout.Stereo;
                    audioFormat.numSpeakers = 2;
                    break;
            }

            CustomSpeakerLayout speakerLayout = FindObjectOfType<CustomSpeakerLayout>();
            if ((speakerLayout != null) && (speakerLayout.speakerPositions.Length == audioFormat.numSpeakers))
            {
                audioFormat.channelLayout = ChannelLayout.Custom;
                audioFormat.speakerDirections = new Vector3[audioFormat.numSpeakers];
                for (int i = 0; i < speakerLayout.speakerPositions.Length; ++i)
                {
                    audioFormat.speakerDirections[i].x = speakerLayout.speakerPositions[i].x;
                    audioFormat.speakerDirections[i].y = speakerLayout.speakerPositions[i].y;
                    audioFormat.speakerDirections[i].z = speakerLayout.speakerPositions[i].z;
                }
            }

            return audioFormat;
        }

        //
        // Get Ray tracing option.
        //
        static public SceneType GetRayTracerOption()
        {
            CustomPhononSettings customSettings = FindObjectOfType<CustomPhononSettings>();
            if (customSettings)
                return customSettings.rayTracerOption;

            return SceneType.Phonon;
        }

        //
        // Get convolution rendering settings.
        //
        static public ConvolutionOption GetConvolutionOption()
        {
            CustomPhononSettings customSettings = FindObjectOfType<CustomPhononSettings>();
            if (customSettings)
                return customSettings.convolutionOption;

            return ConvolutionOption.Phonon;
        }

        //
        // Get compute device settings.
        //
        static public ComputeDeviceType GetComputeDeviceSettings(out int numComputeUnits, out bool useOpenCL)
        {
            CustomPhononSettings customSettings = FindObjectOfType<CustomPhononSettings>();
            if (customSettings)
            {
                numComputeUnits = customSettings.numComputeUnits;
                useOpenCL = customSettings.useOpenCL;
                return customSettings.computeDeviceOption;
            }

            numComputeUnits = 0;
            useOpenCL = false;
            return ComputeDeviceType.CPU;
        }
    }
}