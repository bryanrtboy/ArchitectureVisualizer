//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using UnityEngine;

namespace Phonon
{
    public class EnvironmentalRendererComponent : MonoBehaviour
    {
        public void SetEnvironment(Environment environment, PropagationSettings simulationSettings)
        {
            ambisonicsFormat.channelLayoutType = ChannelLayoutType.Ambisonics;
            ambisonicsFormat.ambisonicsOrder = simulationSettings.ambisonicsOrder;
            ambisonicsFormat.numSpeakers = (ambisonicsFormat.ambisonicsOrder + 1) * (ambisonicsFormat.ambisonicsOrder + 1);
            ambisonicsFormat.ambisonicsOrdering = AmbisonicsOrdering.ACN;
            ambisonicsFormat.ambisonicsNormalization = AmbisonicsNormalization.N3D;
            ambisonicsFormat.channelOrder = ChannelOrder.Deinterleaved;

            var error = PhononCore.iplCreateEnvironmentalRenderer(Context.GetContext(), environment.GetEnvironment(),
                RenderingSettings(), ambisonicsFormat, ref environmentalRenderer);
            if (error != Error.None)
            {
                throw new Exception("Unable to create environment renderer [" + error.ToString() + "]");
            }
        }

        public IntPtr GetEnvironmentalRenderer()
        {
            return environmentalRenderer;
        }

        public void SetUsedByMixer()
        {
            usedByMixer = true;
        }

        void OnDestroy()
        {
            if (environmentalRenderer != IntPtr.Zero && !usedByMixer)
                PhononCore.iplDestroyEnvironmentalRenderer(ref environmentalRenderer);
        }

        RenderingSettings RenderingSettings()
        {
            return PhononSettings.GetRenderingSettings();
        }

        AudioFormat ambisonicsFormat;
        IntPtr environmentalRenderer = IntPtr.Zero;
        bool usedByMixer = false;
    }
}