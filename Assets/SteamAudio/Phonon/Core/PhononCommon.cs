//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using System.Runtime.InteropServices;

namespace Phonon
{
    //
    // Basic types.
    //

    // Boolean values.
    public enum Bool
    {
        False,
        True
    }

    // Error codes.
    public enum Error
    {
        None,
        Fail,
        OutOfMemory,
        Initialization
    }

    // Global context.
    [StructLayout(LayoutKind.Sequential)]
    public struct GlobalContext
    {
        public IntPtr logCallback;
        public IntPtr allocateCallback;
        public IntPtr freeCallback;
    };


    //
    // Geometric types.
    //

    // Point in 3D space.
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    // Quaternion for Ambisonics rotation.
    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    // Box for Acoustic Probes.
    [StructLayout(LayoutKind.Sequential)]
    public struct Box
    {
        public Vector3 minCoordinates;
        public Vector3 maxCoordinates;
    }

    // Sphere for Acoustic Probes.
    [StructLayout(LayoutKind.Sequential)]
    public struct Sphere
    {
		public float centerx;
		public float centery;
		public float centerz;
        public float radius;
    }

    //
    // Audio pipeline.
    //

    // Supported channel layout types.
    public enum ChannelLayoutType
    {
        Speakers,
        Ambisonics
    }

    // Supported channel layouts.
    public enum ChannelLayout
    {
        Mono,
        Stereo,
        Quadraphonic,
        FivePointOne,
        SevenPointOne,
        Custom
    }

    // Supported channel order.
    public enum ChannelOrder
    {
        Interleaved,
        Deinterleaved
    }

    // Supported Ambisonics ordering.
    public enum AmbisonicsOrdering
    {
        FurseMalham,
        ACN
    }

    // Supported Ambisonics normalization.
    public enum AmbisonicsNormalization
    {
        FurseMalham,
        SN3D,
        N3D
    }

    // Audio format.
    [StructLayout(LayoutKind.Sequential)]
    public struct AudioFormat
    {
        public ChannelLayoutType channelLayoutType;
        public ChannelLayout channelLayout;
        public int numSpeakers;
        public Vector3[] speakerDirections;
        public int ambisonicsOrder;
        public AmbisonicsOrdering ambisonicsOrdering;
        public AmbisonicsNormalization ambisonicsNormalization;
        public ChannelOrder channelOrder;
    }

    // Audio format.
    [StructLayout(LayoutKind.Sequential)]
    public struct AudioBuffer
    {
        public AudioFormat audioFormat;
        public int numSamples;
        public float[] interleavedBuffer;
        public IntPtr[] deInterleavedBuffer;
    }

    // Rendering Setings
    [StructLayout(LayoutKind.Sequential)]
    public struct RenderingSettings
    {
        public int samplingRate;
        public int frameSize;
        public ConvolutionOption convolutionOption;
    }

    // HRTF interpolation options.
    public enum HRTFInterpolation
    {
        Nearest,
        Bilinear
    }

    // Indexed triangle.
    [StructLayout(LayoutKind.Sequential)]
    public struct Triangle
    {
        public int index0;
        public int index1;
        public int index2;
    }

    // Material.
    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public float absorptionLow;
        public float absorptionMid;
        public float absorptionHigh;
        public float scattering;
    }

    // Choose a scene type.
    public enum SceneType
    {
        Phonon,
        Embree,
        RadeonRays,
        Custom
    }

    // Choose a convolution option.
    public enum ConvolutionOption
    {
        Phonon,
        TrueAudioNext
    }

    // Choose an occlusion option for direct sound.
    public enum OcclusionOption
    {
        None,
        Raycast,
        Partial
    }

    public enum SimulationType
    {
        Realtime,
        Baked
    }

    // Settings for propagation simulation.
    [StructLayout(LayoutKind.Sequential)]
    public struct PropagationSettings
    {
        public SceneType sceneType;
        public int rays;
        public int secondaryRays;
        public int bounces;
        public float irDuration;
        public int ambisonicsOrder;
        public int maxConvolutionSources;
    }

    // Choose a compute device.
    public enum ComputeDeviceType
    {
        CPU,
        GPU,
        Any
    }

    // Choose probe batch type.
    public enum ProbeBatchType
    {
        Static,
        Dynamic
    }

    // Choose probe placement strategy.
    public enum ProbePlacementStrategy
    {
        Centroid,
        Octree,
        UniformFloor
    }

    // Parameters for probe placement.
    [StructLayout(LayoutKind.Sequential)]
    public struct ProbePlacementParameters
    {
        public ProbePlacementStrategy placement;
        public float horizontalSpacing;
        public float heightAboveFloor;
        public int maxNumTriangles;
        public int maxOctreeDepth;
    }

    // Baking Settings.
    [StructLayout(LayoutKind.Sequential)]
    public struct BakingSettings
    {
        public Bool bakeParametric;
        public Bool bakeConvolution;
    }

    // Settings for propagation rendering.
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectSoundPath
    {
        public Vector3 direction;
        public float distanceAttenuation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] airAbsorption;
        public float propagationDelay;
        public float occlusionFactor;
    }

    //
    // Common API functions.
    //
    public static class Common
    {
        public static Vector3 ConvertVector(UnityEngine.Vector3 point)
        {
            Vector3 convertedPoint;
            convertedPoint.x = point.x;
            convertedPoint.y = point.y;
            convertedPoint.z = -point.z;

            return convertedPoint;
        }

        public static UnityEngine.Vector3 ConvertVector(Vector3 point)
        {
            UnityEngine.Vector3 convertedPoint;
            convertedPoint.x = point.x;
            convertedPoint.y = point.y;
            convertedPoint.z = -point.z;

            return convertedPoint;
        }
    }
}
