using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Shadalyze.Editor.Wrapper
{
    /// <summary>
    /// Wrapper for calling ShaderUtil internal function.
    /// </summary>
    public static class ShaderUtilWrapper
    {
        /// <summary>
        /// Shader Compile Mode.
        /// </summary>
        public enum CompileMode
        {
            CurrentGraphicsDevice,
            CurrentBuildPlatform,
            AllPlatforms,
            Custom,
        }
    
        /// <summary>
        /// Keep in sync with ShaderCompilerPlatform.
        /// </summary>
        [Flags]
        public enum CompilePlatform
        {
            None            = 1 << 0,
            D3D             = 1 << 4,
            GLES3x          = 1 << 9,
            Metal           = 1 << 14,
            Vulkan          = 1 << 18,
        }
    
        private static readonly MethodInfo OpenCompiledShaderFunc = typeof(ShaderUtil).GetMethod("OpenCompiledShader", BindingFlags.NonPublic | BindingFlags.Static);
    
        /// <summary>
        /// Call ShaderUtil.OpenCompiledShader like Compile and show code on shader inspector.
        /// </summary>
        /// <param name="shader"> Shader to compile. </param>
        /// <param name="mode"> Compile Mode. </param>
        /// <param name="externPlatformsMask"> Compile Platform. </param>
        /// <param name="includeAllVariants"> If true, include all shader features. </param>
        /// <param name="preprocessOnly"> If true, export preprocess shader only. </param>
        /// <param name="stripLineDirectives"> If true, export preprocess shader stripping line directives. </param>
        /// <returns> the file path of compiled shader source code. </returns>
        public static string OpenCompiledShader(Shader shader, CompileMode mode, CompilePlatform externPlatformsMask, bool includeAllVariants, bool preprocessOnly, bool stripLineDirectives)
        {
            OpenCompiledShaderFunc.Invoke(null, new object[] { shader, (int)mode, (int)externPlatformsMask, includeAllVariants, preprocessOnly, stripLineDirectives});
            return $"{Application.dataPath}/Compiled-{shader.name.Replace('/', '-')}.shader";
        }
    
        private static readonly MethodInfo OpenCompiledComputeShaderFunc = typeof(ShaderUtil).GetMethod("OpenCompiledComputeShader", BindingFlags.NonPublic | BindingFlags.Static);
    
        /// <summary>
        /// Call ShaderUtil.OpenCompiledComputeShader like Compile and show code on compute shader inspector.
        /// </summary>
        /// <param name="shader"> Compute Shader to compile </param>
        /// <param name="allVariantsAndPlatforms"> If true, include all shader features. </param>
        /// <param name="preprocessOnly"> If true, export preprocess shader only. </param>
        /// <param name="stripLineDirectives"> If true, export preprocess shader stripping line directives. </param>
        /// <returns> the file path of compiled shader source code. </returns>
        public static string OpenCompiledComputeShader(ComputeShader shader, bool allVariantsAndPlatforms, bool preprocessOnly, bool stripLineDirectives)
        {
            OpenCompiledComputeShaderFunc.Invoke(null, new object[] { shader, allVariantsAndPlatforms, preprocessOnly, stripLineDirectives});
            return $"{Application.dataPath}/Compiled-{shader.name.Replace('/', '-')}.shader";
        }

        private static readonly MethodInfo GetShaderVariantEntriesFilteredFunc = typeof(ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Call ShaderUtil.GetShaderVariantEntriesFiltered.
        /// </summary>
        public static void GetShaderVariantEntriesFiltered(Shader shader,
            int maxEntries,
            string[] filterKeywords,
            ShaderVariantCollection excludeCollection,
            out int[] passTypes,
            out string[] keywordLists,
            out string[] remainingKeywords)
        {
            object[] parameters = new object[] { shader, maxEntries, filterKeywords, excludeCollection, null, null, null };
            GetShaderVariantEntriesFilteredFunc.Invoke(null, parameters);
            passTypes = parameters[4] as int[];
            keywordLists = parameters[5] as string[];
            remainingKeywords = parameters[6] as string[];
        }
        
        /// <summary>
        /// Get Shader variants list from ShaderVariantCollection.
        /// </summary>
        public static List<ShaderVariantCollection.ShaderVariant> GetShaderVariantsFromCollections(ShaderVariantCollection shaderVariantCollection)
        {
            List<ShaderVariantCollection.ShaderVariant> reuslt = new List<ShaderVariantCollection.ShaderVariant>();
            SerializedObject svcObject = new SerializedObject(shaderVariantCollection);
            SerializedProperty shaders = svcObject.FindProperty("m_Shaders");
            for (var shaderIndex = 0; shaderIndex < shaders.arraySize; ++shaderIndex)
            {
                var entryProp = shaders.GetArrayElementAtIndex(shaderIndex);
                Shader shader = (Shader)entryProp.FindPropertyRelative("first").objectReferenceValue;
                var variantsProp = entryProp.FindPropertyRelative("second.variants");
                for (var i = 0; i < variantsProp.arraySize; ++i)
                {
                    var prop = variantsProp.GetArrayElementAtIndex(i);
                    var passType = (UnityEngine.Rendering.PassType)prop.FindPropertyRelative("passType").intValue;
                    var keywords = prop.FindPropertyRelative("keywords").stringValue;
                    reuslt.Add(new ShaderVariantCollection.ShaderVariant(shader, passType, keywords.Split(' ')));
                }
            }

            return reuslt;
        }
    }
}
