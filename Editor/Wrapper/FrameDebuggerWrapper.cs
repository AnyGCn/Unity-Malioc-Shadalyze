using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace Shadalyze.Editor.Wrapper
{
    internal static class FrameDebuggerUtility
    {
        public static int curEventIndex => limit - 1;
        
        public static FrameDebuggerEventData GetFrameEventData(int index)
        {
            object value = FrameDebuggerEventDataConstructor.Invoke(Array.Empty<object>());
            if (!(bool)GetFrameEventDataInternal.Invoke(null, new object[] { index, value }))
                return null;
            
            object shaderInfoSource = ShaderInfoField.GetValue(value);
            FrameDebuggerEventData frameDebuggerEventData = new FrameDebuggerEventData
            {
                m_FrameEventIndex = (int)FrameEventIndexField.GetValue(value),
                m_OriginalShaderName = (string)OriginalShaderNameField.GetValue(value),
                m_RealShaderName = (string)RealShaderNameField.GetValue(value),
                m_PassName = (string)PassNameField.GetValue(value),
                m_PassLightMode = (string)PassLightModeField.GetValue(value),
                m_ShaderInstanceID = (int)ShaderInstanceIDField.GetValue(value),
                m_SubShaderIndex = (int)SubShaderIndexField.GetValue(value),
                m_ShaderPassIndex = (int)ShaderPassIndexField.GetValue(value),
                m_ComputeShaderName = (string)ComputeShaderNameField.GetValue(value),
                m_ComputeShaderKernelName = (string)ComputeShaderKernelNameField.GetValue(value),
                m_ShaderInfo = UnsafeUtility.As<object, FrameDebuggerEventData.ShaderInfo>(ref shaderInfoSource),
            };

            frameDebuggerEventData.shaderKeywords =
                frameDebuggerEventData.m_ShaderInfo.m_Keywords.Select(k => k.m_Name).ToArray();
            return frameDebuggerEventData;
        }
        
        // the reflection of FrameDebuggerUtility
        private static int limit => (int)limitProperty.GetValue(null);
        private static Type FrameDebuggerUtilityClass = typeof(EditorUtility).Assembly.GetType("UnityEditorInternal.FrameDebuggerInternal.FrameDebuggerUtility");
        private static MethodInfo GetFrameEventDataInternal = FrameDebuggerUtilityClass.GetMethod(nameof(GetFrameEventData));
        private static PropertyInfo limitProperty = FrameDebuggerUtilityClass.GetProperty("limit");
        
        // the reflection of FrameDebuggerEventData
        private static Type FrameDebuggerEventDataClass = typeof(EditorUtility).Assembly.GetType("UnityEditorInternal.FrameDebuggerInternal.FrameDebuggerEventData");
        private static ConstructorInfo FrameDebuggerEventDataConstructor = FrameDebuggerEventDataClass.GetConstructor(Array.Empty<Type>());
        private static FieldInfo FrameEventIndexField = FrameDebuggerEventDataClass.GetField("m_FrameEventIndex");
        private static FieldInfo OriginalShaderNameField = FrameDebuggerEventDataClass.GetField("m_OriginalShaderName");
        private static FieldInfo RealShaderNameField = FrameDebuggerEventDataClass.GetField("m_RealShaderName");
        private static FieldInfo PassNameField = FrameDebuggerEventDataClass.GetField("m_PassName");
        private static FieldInfo PassLightModeField = FrameDebuggerEventDataClass.GetField("m_PassLightMode");
        private static FieldInfo ShaderInstanceIDField = FrameDebuggerEventDataClass.GetField("m_ShaderInstanceID");
        private static FieldInfo SubShaderIndexField = FrameDebuggerEventDataClass.GetField("m_SubShaderIndex");
        private static FieldInfo ShaderPassIndexField = FrameDebuggerEventDataClass.GetField("m_ShaderPassIndex");
        // private static FieldInfo ShaderKeywordsField = FrameDebuggerEventDataClass.GetField("shaderKeywords");
        private static FieldInfo ComputeShaderNameField = FrameDebuggerEventDataClass.GetField("m_ComputeShaderName");
        private static FieldInfo ComputeShaderKernelNameField = FrameDebuggerEventDataClass.GetField("m_ComputeShaderKernelName");
        private static FieldInfo ShaderInfoField = FrameDebuggerEventDataClass.GetField("m_ShaderInfo");
    }

    internal class FrameDebuggerEventData
    {
        // required properties 
        public int m_FrameEventIndex;
        public string m_OriginalShaderName;
        public string m_RealShaderName;
        public string m_PassName;
        public string m_PassLightMode;
        public int m_ShaderInstanceID;
        public int m_SubShaderIndex;
        public int m_ShaderPassIndex;
        public string[] shaderKeywords;
        public string m_ComputeShaderName;
        public string m_ComputeShaderKernelName;
        public ShaderInfo m_ShaderInfo;

        // Match C++ ScriptingShaderKeywordInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderKeywordInfo
        {
            public string m_Name;
            public int m_Flags;
            public bool m_IsGlobal;
            public bool m_IsDynamic;
        };

        // Match C++ ScriptingShaderFloatInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderFloatInfo
        {
            public string m_Name;
            public int m_Flags;
            public float m_Value;
        }

        // Match C++ ScriptingShaderIntInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderIntInfo
        {
            public string m_Name;
            public int m_Flags;
            public int m_Value;
        }

        // Match C++ ScriptingShaderVectorInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderVectorInfo
        {
            public string m_Name;
            public int m_Flags;
            public Vector4 m_Value;
        }

        // Match C++ ScriptingShaderMatrixInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderMatrixInfo
        {
            public string m_Name;
            public int m_Flags;
            public Matrix4x4 m_Value;
        }

        // Match C++ ScriptingShaderTextureInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderTextureInfo
        {
            public string m_Name;
            public int m_Flags;
            public string m_TextureName;
            public Texture m_Value;
        }

        // Match C++ ScriptingShaderBufferInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderBufferInfo
        {
            public string m_Name;
            public int m_Flags;
        }

        // Match C++ ScriptingShaderBufferInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderConstantBufferInfo
        {
            public string m_Name;
            public int m_Flags;
        }

        // Match C++ ScriptingShaderInfo memory layout!
        [StructLayout(LayoutKind.Sequential)]
        public class ShaderInfo
        {
            public ShaderKeywordInfo[] m_Keywords;
            public ShaderFloatInfo[] m_Floats;
            public ShaderIntInfo[] m_Ints;
            public ShaderVectorInfo[] m_Vectors;
            public ShaderMatrixInfo[] m_Matrices;
            public ShaderTextureInfo[] m_Textures;
            public ShaderBufferInfo[] m_Buffers;
            public ShaderConstantBufferInfo[] m_CBuffers;
        }
    }
}