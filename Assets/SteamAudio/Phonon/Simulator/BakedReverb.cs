//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Phonon
{
    //
    //	ReverbBakeStatus
    //	Possible states the bake process can be in.
    //
    public enum BakeStatus
    {
        Ready,
        InProgress,
        Complete
    }

    //
    // BakedReverb
    // Represents a baked reverb component.
    //

    [AddComponentMenu("Phonon/Baked Reverb")]
    public class BakedReverb : MonoBehaviour
    {
        void OnDrawGizmosSelected()
        {
            Color oldColor = Gizmos.color;

            Gizmos.color = Color.magenta;
            ProbeBox[] drawProbeBoxes = probeBoxes;
            if (useAllProbeBoxes)
                drawProbeBoxes = FindObjectsOfType<ProbeBox>() as ProbeBox[];

            if(drawProbeBoxes != null)
                foreach (ProbeBox probeBox in drawProbeBoxes)
                    if (probeBox != null)
                        Gizmos.DrawWireCube(probeBox.transform.position, probeBox.transform.localScale);

            Gizmos.color = oldColor;
        }

        public void BakeEffectThread()
        {
            BakingSettings bakeSettings;
			bakeSettings.bakeConvolution = bakeConvolution ? Bool.True : Bool.False;
			bakeSettings.bakeParametric = bakeParameteric ? Bool.True : Bool.False;

            foreach (ProbeBox probeBox in duringBakeProbeBoxes)
            {
                if (cancelBake)
                    return;

                IntPtr probeBoxPtr = IntPtr.Zero;
                try
                {
                    PhononCore.iplLoadProbeBox(probeBox.probeBoxData, probeBox.probeBoxData.Length, ref probeBoxPtr);
                    probeBoxBakingCurrently++;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                PhononCore.iplBakeReverb(duringBakeEnvComponent.Environment().GetEnvironment(), probeBoxPtr, bakeSettings, bakeCallback);

                if (cancelBake)
                    return;

                int probeBoxSize = PhononCore.iplSaveProbeBox(probeBoxPtr, null);
                probeBox.probeBoxData = new byte[probeBoxSize];
                PhononCore.iplSaveProbeBox(probeBoxPtr, probeBox.probeBoxData);

                int probeBoxEffectSize = PhononCore.iplGetBakedDataSizeByName(probeBoxPtr, "__reverb__");
                probeBox.UpdateProbeDataMapping("__reverb__", probeBoxEffectSize);

                PhononCore.iplDestroyProbeBox(ref probeBoxPtr);
            }

            bakeStatus = BakeStatus.Complete;
        }

        public void BeginBake()
        {
            bakeStatus = BakeStatus.InProgress;

            duringBakeProbeBoxes = probeBoxes;
            if (useAllProbeBoxes)
                duringBakeProbeBoxes = FindObjectsOfType<ProbeBox>() as ProbeBox[];

            totalProbeBoxes = duringBakeProbeBoxes.Length;

            // Initialize environment
            try
            {
                duringBakeEnvComponent = FindObjectOfType<EnvironmentComponent>();
                if (duringBakeEnvComponent == null)
                    throw new Exception("Environment Component not found. Add one to the scene");

                bool initializeRenderer = false;
                duringBakeEnvComponent.Initialize(initializeRenderer);
            }
            catch (Exception e)
            {
                bakeStatus = BakeStatus.Complete;
                Debug.LogError(e.Message);
                return;
            }

            bakeCallback = new PhononCore.BakeProgressCallback(AdvanceProgress);

			#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            bakeCallbackPointer = Marshal.GetFunctionPointerForDelegate(bakeCallback);
            bakeCallbackHandle = GCHandle.Alloc(bakeCallbackPointer);
            GC.Collect();
			#endif

            bakeThread = new Thread(new ThreadStart(BakeEffectThread));
            bakeThread.Start();
        }

        public void EndBake()
        {
            if (bakeThread != null)
                bakeThread.Join();

			#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            if (bakeCallbackHandle.IsAllocated)
                bakeCallbackHandle.Free();
			#endif

            if (duringBakeEnvComponent)
                duringBakeEnvComponent.Destroy();

            duringBakeEnvComponent = null;
            duringBakeProbeBoxes = null;
            totalProbeBoxes = 0;
            probeBoxBakingCurrently = 0;
            probeBoxBakingProgress = .0f;
            bakeStatus = BakeStatus.Ready;
        }

        void AdvanceProgress(float bakeProgressFraction)
        {
            probeBoxBakingProgress = bakeProgressFraction;
        }

        public int totalProbeBoxes = 0;
        public int probeBoxBakingCurrently = 0;
        public float probeBoxBakingProgress = .0f;

        public bool bakeConvolution = true;
        public bool bakeParameteric = false;
        public bool useAllProbeBoxes = false;
        public bool cancelBake = false;
        public ProbeBox[] probeBoxes = null;

        public BakeStatus bakeStatus = BakeStatus.Ready;
        public EnvironmentComponent duringBakeEnvComponent = null;
        public ProbeBox[] duringBakeProbeBoxes = null;

        Thread bakeThread;
        PhononCore.BakeProgressCallback bakeCallback;
        IntPtr bakeCallbackPointer;
        GCHandle bakeCallbackHandle;
    }
}
