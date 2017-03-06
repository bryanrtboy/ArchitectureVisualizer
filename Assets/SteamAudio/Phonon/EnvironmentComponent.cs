//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;
using System;

namespace Phonon
{
    public class EnvironmentComponent : MonoBehaviour
    {
        public static void ExportScene()
        {
            Scene exportScene = new Scene();
            ComputeDevice exportComputeDevice = new ComputeDevice();
            try
            {
                exportScene.Export(exportComputeDevice, SimulationSettings());
            }
            catch (Exception e)
            {
                Debug.LogError("Phonon Geometry not attached. " + e.Message);
            }
        }

        public static void DumpScene()
        {
            Scene exportScene = new Scene();
            ComputeDevice exportComputeDevice = new ComputeDevice();
            try
            {
                exportScene.DumpToObj(exportComputeDevice, SimulationSettings());
            }
            catch (Exception e)
            {
                Debug.LogError("Phonon Geometry not attached. " + e.Message);
            }
        }

        public void Initialize(bool initializeRenderer)
        {
            bool useOpenCL;
            int numComputeUnits;
            ComputeDeviceType deviceType;
            deviceType = PhononSettings.GetComputeDeviceSettings(out numComputeUnits, out useOpenCL);

            try
            {
                computeDevice.Create(useOpenCL, deviceType, numComputeUnits);
            }
            catch (Exception e)
            {
                throw e;
            }

            if (initializeRenderer)
                probeManager.Create();

            if (scene.Create(computeDevice, SimulationSettings()) != Error.None)
                return;

            try
            {
                environment.Create(computeDevice, SimulationSettings(), scene, probeManager);
            }
            catch (Exception e)
            {
                throw e;
            }

            // initialize renderer only at runtime when rendering audio.
            if (initializeRenderer)
            {
                var environmentalRendererComponent = GameObject.FindObjectOfType<EnvironmentalRendererComponent>();
                if (environmentalRendererComponent != null)
                    environmentalRendererComponent.SetEnvironment(environment, SimulationSettings());
            }
        }

        public void Destroy()
        {
            environment.Destroy();
            scene.Destroy();
            computeDevice.Destroy();
            probeManager.Destroy();
        }

        void Start()
        {
            bool initializeRenderer = true;
            Initialize(initializeRenderer);
        }

        void OnDestroy()
        {
            Destroy();
        }

        public static PropagationSettings SimulationSettings()
        {
            SimulationSettings simSetings = PhononSimulationSettings.GetSimulationSettings();
            SceneType rayTracer = PhononSettings.GetRayTracerOption();
            float simDuration = simSetings.Value.Duration;
            int simAmbisonicsOrder = simSetings.Value.AmbisonicsOrder;
            int simMaxSources = simSetings.Value.MaxSources;

            int simRays = 0;
            int simSecondaryRays = 0;
            int simBounces = 0;

            bool editorInEditMode = false;
#if UNITY_EDITOR 
            editorInEditMode = Application.isEditor && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#endif
            //When in edit mode use baked settings, when in play mode or standalone application mode use realtime setttings.
            if (editorInEditMode)
            {
                simRays = simSetings.Value.BakeRays;
                simSecondaryRays = simSetings.Value.BakeSecondaryRays;
                simBounces = simSetings.Value.BakeBounces;
            }
            else
            {
                simRays = simSetings.Value.RealtimeRays;
                simSecondaryRays = simSetings.Value.RealtimeSecondaryRays;
                simBounces = simSetings.Value.RealtimeBounces;
            }

            PropagationSettings simulationSettings = new PropagationSettings
            {
                sceneType               = rayTracer,
                rays                    = simRays,
                secondaryRays           = simSecondaryRays,
                bounces                 = simBounces,
                irDuration              = simDuration,
                ambisonicsOrder         = simAmbisonicsOrder,
                maxConvolutionSources   = simMaxSources,
            };

            return simulationSettings;
        }

        public Scene Scene()
        {
            return scene;
        }

        public ProbeManager ProbeManager()
        {
            return probeManager;
        }

        public Environment Environment()
        {
            return environment;
        }

        ComputeDevice computeDevice = new ComputeDevice();
        Scene scene                 = new Scene();
        Environment environment     = new Environment();
        ProbeManager probeManager   = new ProbeManager();
    }
}