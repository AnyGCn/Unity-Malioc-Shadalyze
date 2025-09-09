using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Shadalyze.Editor
{
    [CustomEditor(typeof(ShadalyzeGlobalSettings))]
    internal class ShadalyzeGlobalSettingsEditor : UnityEditor.Editor
    {
        protected override void OnHeaderGUI() {}
        
        public override void OnInspectorGUI()
        {
            EmbeddedToolLocation("Compiled Code", ShadalyzeGlobalSettings.CompileCodePath);
            EmbeddedToolLocation("Analyze Result", ShadalyzeGlobalSettings.AnalyzeResultPath);
            EmbeddedToolLocation("Default Malioc", ShadalyzeGlobalSettings.MaliocExePath);
            base.OnInspectorGUI();
        }
        
        static readonly GUILayoutOption[] labelLayoutOption = new GUILayoutOption[] { GUILayout.Width(146) };
        static readonly GUILayoutOption buttonLayoutOption = GUILayout.MaxWidth(120);
        
        static void EmbeddedToolLocation(string pathLabel, string path)
        {
            GUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(true))
            {
                GUILayout.Label(pathLabel, labelLayoutOption);
                EditorGUILayout.TextField(path);
            }

            if (GUILayout.Button("Copy Path", EditorStyles.miniButton, buttonLayoutOption))
            {
                GUIUtility.systemCopyBuffer = path;
            }

            GUILayout.EndHorizontal();
        }
        
        static void ToolLocation(string pathLabel, ref string pathRef, string fileName)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(pathLabel, labelLayoutOption);

            EditorGUI.BeginChangeCheck();
            var candidate = EditorGUILayout.TextField(pathRef);
            if (EditorGUI.EndChangeCheck() && Directory.Exists(candidate))
                pathRef = candidate;

            if (GUILayout.Button("Browse", EditorStyles.miniButton, buttonLayoutOption))
            {
                candidate = Browse(pathLabel, pathRef, fileName);
                if (!string.IsNullOrEmpty(candidate))
                {
                    pathRef = candidate;
                    GUI.FocusControl("");
                }
                //After returning from a native dialog on OSX GUILayout gets into a corrupt state, stop rendering UI for this frame.
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();
        }

        void ToolLocation(string toolName, SerializedProperty pathRef, SerializedProperty toolUseEmbedded, string toolEmbeddedPath, string fileName)
        {
            EditorGUI.BeginChangeCheck();
            var useEmbedded = EditorGUILayout.ToggleLeft($"{toolName} Installed with Unity (recommended)", toolUseEmbedded.boolValue);
            if (EditorGUI.EndChangeCheck())
                toolUseEmbedded.boolValue = useEmbedded;

            if (useEmbedded)
            {
                if (pathRef.stringValue != toolEmbeddedPath)
                    pathRef.stringValue = toolEmbeddedPath;
                EmbeddedToolLocation(toolName, toolEmbeddedPath);
            }
            else
            {
                string path = pathRef.stringValue;
                ToolLocation(toolName, ref path, fileName);
                if (path != pathRef.stringValue)
                    pathRef.stringValue = path;
            }
        }
        
        public static bool Validate(ref string directory, string fileName)
        {
            if (string.IsNullOrEmpty(directory))
                return false;
            if (!Directory.Exists(directory))
                return false;
            
            return string.IsNullOrEmpty(fileName) || File.Exists(Path.Combine(directory, fileName));
        }
        
        public static string Browse(string toolName, string directory, string fileName)
        {
            for (;;)
            {
                directory = EditorUtility.OpenFolderPanel($"Select {toolName} directory", directory, string.Empty);
                if (string.IsNullOrEmpty(directory))
                    return string.Empty;
                
                if (Validate(ref directory, fileName))
                    return directory;
                
                if (!EditorUtility.DisplayDialog($"Invalid directory", String.IsNullOrEmpty(fileName) ? "Invalid directory" : $"Can't find {fileName}.", "Browse", "Cancel"))
                    return String.Empty;
            }
        }

    }
}