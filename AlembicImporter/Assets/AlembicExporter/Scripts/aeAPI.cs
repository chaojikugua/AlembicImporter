using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class aeAPI
{
    public enum aeArchiveType
    {
        HDF5,
        Ogawa,
    };

    public enum aeTypeSamplingType
    {
        Uniform,
        Acyclic,
    };

    public enum aePropertyType
    {
        FloatArray,
        IntArray,
        BoolArray,
        Vec2Array,
        Vec3Array,
        Vec4Array,
        Mat44Array,
    };

    public struct aeContext { public IntPtr ptr; }
    public struct aeObject { public IntPtr ptr; }
    public struct aeProperty { public IntPtr ptr; }

    [Serializable]
    public struct aeConfig
    {
        [MarshalAs(UnmanagedType.U4)] public aeArchiveType archiveType;
        [MarshalAs(UnmanagedType.U4)] public aeTypeSamplingType timeSamplingType;
        public float startTime;     // relevant only if timeSamplingType is uniform
        public float timePerSample; // relevant only if timeSamplingType is uniform
        [MarshalAs(UnmanagedType.U1)] public bool swapHandedness; // swap rhs <-> lhs


        public static aeConfig default_value
        {
            get
            {
                return new aeConfig
                {
                    archiveType = aeArchiveType.Ogawa,
                    timeSamplingType = aeTypeSamplingType.Uniform,
                    startTime = 0.0f,
                    timePerSample = 1.0f / 30.0f,
                    swapHandedness = true,
                };
            }
        }
    }

    public struct aeXFormSampleData
    {
        public Vector3 translation;
        public Vector3 rotationAxis;
        public float rotationAngle;
        public Vector3 scale;
        [MarshalAs(UnmanagedType.U1)] public bool inherits;
    }

    public struct aePointsSampleData
    {
        public IntPtr positions; // Vector3*
        public IntPtr ids; // ulong*. can be null
        public int count;
    }

    public struct aePolyMeshSampleData
    {
        public IntPtr positions; // Vector3*
        public IntPtr normals; // Vector3*. can be null
        public IntPtr uvs; // Vector2*. can be null
        public IntPtr indices; // int*
        public IntPtr faces; // int*. can be null. assume all faces are triangles if null

        public int vertexCount;
        public int indexCount;
        public int faceCount;
    }

    public struct aeCameraSampleData
    {
        public float nearClippingPlane;
        public float farClippingPlane;
        public float fieldOfView;
        public float focusDistance;
        public float focalLength;
    }

    
    [DllImport("AlembicExporter")] public static extern aeContext   aeCreateContext(ref aeConfig conf);
    [DllImport("AlembicExporter")] public static extern void        aeDestroyContext(aeContext ctx);

    [DllImport("AlembicExporter")] public static extern bool        aeOpenArchive(aeContext ctx, string path);
    [DllImport("AlembicExporter")] public static extern aeObject    aeGetTopObject(aeContext ctx);
    [DllImport("AlembicExporter")] public static extern void        aeAddTime(aeContext ctx, float time); // relevant only if timeSamplingType is acyclic

    [DllImport("AlembicExporter")] public static extern aeObject    aeNewXForm(aeObject parent, string name);
    [DllImport("AlembicExporter")] public static extern aeObject    aeNewCamera(aeObject parent, string name);
    [DllImport("AlembicExporter")] public static extern aeObject    aeNewPoints(aeObject parent, string name);
    [DllImport("AlembicExporter")] public static extern aeObject    aeNewPolyMesh(aeObject parent, string name);
    [DllImport("AlembicExporter")] public static extern void        aeXFormWriteSample(aeObject obj, ref aeXFormSampleData data);
    [DllImport("AlembicExporter")] public static extern void        aePointsWriteSample(aeObject obj, ref aePointsSampleData data);
    [DllImport("AlembicExporter")] public static extern void        aePolyMeshWriteSample(aeObject obj, ref aePolyMeshSampleData data);
    [DllImport("AlembicExporter")] public static extern void        aeCameraWriteSample(aeObject obj, ref aeCameraSampleData data);

    [DllImport("AlembicExporter")] public static extern aeProperty  aeNewProperty(aeObject parent, string name, aePropertyType type);
    [DllImport("AlembicExporter")] public static extern void        aePropertyWriteSample(aeProperty prop, IntPtr data, int num_data);


    public static void aeWait(float wt)
    {
        while (Time.realtimeSinceStartup - Time.unscaledTime < wt) {
            System.Threading.Thread.Sleep(1);
        }
    }

    public static void aeWaitMaxDeltaTime()
    {
        aeWait(Time.maximumDeltaTime);
    }
}