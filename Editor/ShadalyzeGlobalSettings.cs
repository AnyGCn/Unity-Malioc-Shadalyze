using System;
using System.Collections.Generic;
using System.IO;
using Shadalyze.Editor.Wrapper;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Shadalyze.Editor
{
    internal enum ShaderAnalysisLevel
    {
        Disabled,
        OnlySRPShaders,
        All
    }
    
    internal class ShadalyzeGlobalSettings : ScriptableObject
    {
        private const string packagePath = "Packages/com.anyg.shadalyze";
        public static readonly string MaliocExePath = Path.GetFullPath($"{packagePath}/Editor/mali_offline_compiler~/malioc.exe");
        public static readonly string CompileCodePath = Path.GetFullPath("Temp/Shadalyze/CompiledCode/");
        public static readonly string AnalyzeResultPath = Path.GetFullPath("Temp/Shadalyze/AnalyzeResult/");
        private static ShadalyzeGlobalSettings _instance = null;

        public static ShadalyzeGlobalSettings Instance
        {
            get
            {
                if (!_instance)
                {
                    string[] guids = AssetDatabase.FindAssets($"t:{nameof(ShadalyzeGlobalSettings)}");
                    for (var index = 0; index < guids.Length; index++)
                    {
                        var guid = guids[index];
                        if (index == 0)
                        {
                            _instance = AssetDatabase.LoadAssetAtPath<ShadalyzeGlobalSettings>(AssetDatabase.GUIDToAssetPath(guid));
                        }
                        else
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                        }
                    }

                    Directory.CreateDirectory(CompileCodePath);
                    Directory.CreateDirectory(AnalyzeResultPath);
                    if (_instance == null)
                    {
                        _instance = CreateInstance<ShadalyzeGlobalSettings>();
                        AssetDatabase.CreateAsset(_instance, $"Assets/{nameof(ShadalyzeGlobalSettings)}.asset");
                    }
                }
                
                return _instance;
            }
        }

        private HashSet<ShaderTagId> m_lightModes;
        public MaliDeviceType BaselineDevice => baselineDevice;
        
        public ShaderAnalysisLevel ShaderAnalysisLevel => shaderAnalysisLevel;

        public HashSet<ShaderTagId> lightModes
        {
            get
            {
                if (m_lightModes == null)
                {
                    m_lightModes = new HashSet<ShaderTagId>();
                    foreach (var mode in analysisLightMode)
                        if (!string.IsNullOrEmpty(mode))
                            m_lightModes.Add(new ShaderTagId(mode));
                }

                return m_lightModes;
            }
        }
            
        [FormerlySerializedAs("analysisDevice")]
        [SerializeField]
        [Tooltip("Controls which shaders will be analyzed in shader build process.")]
        private MaliDeviceType baselineDevice = MaliDeviceType.Immortalis_G715;
        
        [SerializeField]
        [Tooltip("Controls which shaders will be analyzed in shader build process.")]
        private ShaderAnalysisLevel shaderAnalysisLevel = ShaderAnalysisLevel.Disabled;

        [SerializeField]
        [Tooltip("Choose the light mode to analyze. (The pass without light mode tag (assumed that light mode is always) would be always included)")]
        private List<string> analysisLightMode = new List<string>() { "UniversalForward", "UniversalGBuffer" };
    }
}