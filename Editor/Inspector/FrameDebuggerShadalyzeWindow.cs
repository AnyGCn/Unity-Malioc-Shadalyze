using System;
using Shadalyze.Editor.Data;
using Shadalyze.Editor.Wrapper;
using UnityEditor;
using UnityEngine;

namespace Shadalyze.Editor
{
    public class FrameDebuggerShadalyzeWindow : EditorWindow
    {
        private FrameDebuggerEventData eventData;
        private GUIStyle m_TextStyle;
        private GUIContent m_CachedPreview;
        private Vector2 m_ScrollViewVector = Vector2.zero;

        [MenuItem("Window/Analysis/Frame Debugger Shadalyze", false, 10)]
        public static void OpenWindow()
        {
            OpenFrameDebuggerWindow();
            
            var wnd = GetWindow(typeof(FrameDebuggerShadalyzeWindow), true);
            wnd.titleContent = EditorGUIUtility.TrTextContent("Frame Debugger Shadalyze");
            wnd.minSize = new Vector2(1000f, 500f);
        }

        private static void OpenFrameDebuggerWindow()
        {
            var wnd = GetWindow(FrameDebuggerWindowClass);
            wnd.titleContent = EditorGUIUtility.TrTextContent("Frame Debugger");
            wnd.minSize = new Vector2(1000f, 800f);
        }

        private void OnGUI()
        {
            if (FrameDebugger.enabled)
            {
                if (GUILayout.Button("Analyze performance"))
                {
                    eventData = FrameDebuggerUtility.GetFrameEventData(FrameDebuggerUtility.curEventIndex);
                    if (!String.IsNullOrEmpty(eventData.m_RealShaderName))
                    {
                        // draw pass
                        var compileRequest = new ShaderCompileRequest(Shader.Find(eventData.m_RealShaderName),
                            eventData.m_SubShaderIndex, eventData.m_ShaderPassIndex, eventData.m_PassName, eventData.shaderKeywords);
                        compileRequest.Compile();
                        m_CachedPreview = new GUIContent(compileRequest.Analyze());
                    }
                    else if (!String.IsNullOrEmpty(eventData.m_ComputeShaderName))
                    {
                        // TODO: support compute shader.
                        m_CachedPreview = new GUIContent("Compute shader analysis would be supported in the future.");
                    }
                    else
                    {
                        // Not draw pass
                        m_CachedPreview = null;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Start FrameDebugger"))
                    OpenFrameDebuggerWindow();
            }
            
            if (m_CachedPreview == null)
                m_CachedPreview = new GUIContent("Start Frame Debugger and capture a draw call or dispatch call.");

            if (m_TextStyle == null)
            {
                m_TextStyle = "ScriptText";
                m_TextStyle.normal.background = Texture2D.whiteTexture;
                m_TextStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            }
            bool enabledTemp = GUI.enabled;
            Color backgroundColorTemp = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            GUI.enabled = true;

            Rect rect = new Rect(0, 20, position.width, position.height);
            GUILayout.BeginArea(rect);
            m_ScrollViewVector = EditorGUILayout.BeginScrollView(m_ScrollViewVector);
            rect = GUILayoutUtility.GetRect(m_CachedPreview, m_TextStyle);
            rect.x = 0;
            rect.y -= 3;
            rect.width = rect.width - 1;
            GUI.Box(rect, "");
            EditorGUI.SelectableLabel(rect, m_CachedPreview.text, m_TextStyle);
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.backgroundColor = backgroundColorTemp;
            GUI.enabled = enabledTemp;
        }
        
        private static readonly Type FrameDebuggerWindowClass = typeof(EditorWindow).Assembly.GetType("UnityEditor.FrameDebuggerWindow");
    }
}
