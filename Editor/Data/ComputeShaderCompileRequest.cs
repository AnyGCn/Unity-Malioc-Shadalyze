using System.Collections;
using System.Collections.Generic;
using Shadalyze.Editor.Manager;
using Shadalyze.Editor.Parser;
using Shadalyze.Editor.Wrapper;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Shadalyze.Editor.Data
{
    public struct ComputeShaderCompileRequest
    {
        public readonly ComputeShader ShaderObject;
        public readonly int KernalIndex;
        public readonly string[] ShaderKeywords;
        public string sha256 { get; private set; }
        public string Code { get; private set; }
        public string Report { get; private set; }
        
        public ComputeShaderCompileRequest(ComputeShader shaderObject, int kernalIndex, string[] shaderKeywords)
        {
            ShaderObject = shaderObject;
            KernalIndex = kernalIndex;
            ShaderKeywords = shaderKeywords;
            sha256 = null;
            Code = null;
            Report = null;
        }
        
        /// <summary>
        /// Compile the shader of GLES3x and dump file to disk for analysis.
        /// </summary>
        /// <returns> success flag. </returns>
        public bool Compile()
        {
            // TODO: There are no method to compile single variant of compute shader, so we need to compile all variants of compute shader.
            return false;
        }

        /// <summary>
        /// Get the analysis report text.
        /// </summary>
        /// <returns> The text of analysis report, return null if analysis is failed. </returns>
        public bool Analyze()
        {
            if (MaliOfflineCompilerWrapper.Analyze($"{ShadalyzeGlobalSettings.CompileCodePath}/{sha256}.comp", out var analysisReport))
            {
                this.Report = analysisReport;
                return true;
            }
            
            return false;
        }
    }
}
