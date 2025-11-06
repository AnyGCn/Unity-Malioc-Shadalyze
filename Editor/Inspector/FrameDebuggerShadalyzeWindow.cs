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
        private int m_SelectIndex = 0;
        private ShaderCompileRequest m_CompiledRequest;
        private GUIContent m_PassName;
        private GUIContent m_Keywords;
        private GUIContent m_VertShaderCode;
        private GUIContent m_FragShaderCode;
        private GUIContent m_VertReport;
        private GUIContent m_FragReport;
        private static readonly GUIContent[] contentType = new GUIContent[]
        {
            new GUIContent("Vertex Shader"),
            new GUIContent("Fragment Shader"),
            new GUIContent("Vertex Report"),
            new GUIContent("Fragment Report"),
        };

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
                    var curEventIndex = FrameDebuggerUtility.curEventIndex;
                    eventData = FrameDebuggerUtility.GetFrameEventData(curEventIndex);
                    FrameDebuggerEvent frameEvent = FrameDebuggerUtility.GetFrameEvent(curEventIndex);
                    FrameEventType eventType = frameEvent.m_Type;
                    bool isClearEvent = FrameDebuggerHelper.IsAClearEvent(eventType);
                    bool isComputeEvent = FrameDebuggerHelper.IsAComputeEvent(eventType);
                    bool isRayTracingEvent = FrameDebuggerHelper.IsARayTracingEvent(eventType);
                    
                    // TODO: support compute shader and detect real draw event
                    if (!isClearEvent && !isComputeEvent && !isRayTracingEvent)
                    {
                        // draw pass
                        m_CompiledRequest = new ShaderCompileRequest(Shader.Find(eventData.m_RealShaderName),
                            eventData.m_SubShaderIndex, eventData.m_ShaderPassIndex, eventData.m_PassName, eventData.shaderKeywords);
                        m_CompiledRequest.Compile();
                        m_CompiledRequest.Analyze();

                        string prefix = $"Pass: {m_CompiledRequest.PassName} ({m_CompiledRequest.PassIndex})\n" +
                                        $"Keywords: {String.Join(' ', m_CompiledRequest.ShaderKeywords)}\n\n";
                        m_VertShaderCode = new GUIContent(prefix + m_CompiledRequest.VertCode);
                        m_FragShaderCode = new GUIContent(prefix + m_CompiledRequest.FragCode);
                        m_VertReport = new GUIContent(prefix + m_CompiledRequest.VertReport);
                        m_FragReport = new GUIContent(prefix + m_CompiledRequest.FragReport);
                    }
                    else
                    {
                        // Not draw pass
                        m_CompiledRequest = default;
                        m_SelectIndex = 0;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Start FrameDebugger"))
                    OpenFrameDebuggerWindow();
            }

            if (m_TextStyle == null)
            {
                m_TextStyle = "ScriptText";
                m_TextStyle.normal.background = Texture2D.whiteTexture;
                m_TextStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
            }
            
            if (!m_CompiledRequest.ShaderObject)
            {
                EditorGUILayout.LabelField("Start Frame Debugger and capture a draw call (compute shader support is on the way.)", m_TextStyle);
                return;
            }
            
            GUIContent content = null;
            m_SelectIndex = EditorGUILayout.Popup(m_SelectIndex, contentType);
            switch (m_SelectIndex)
            {
                case 0:
                    content = m_VertShaderCode;
                    break;
                case 1:
                    content = m_FragShaderCode;
                    break;
                case 2:
                    content = m_VertReport;
                    break;
                case 3:
                    content = m_FragReport;
                    break;
                default:
                    content = m_VertShaderCode;
                    break;
            }

            bool enabledTemp = GUI.enabled;
            Color backgroundColorTemp = GUI.backgroundColor;
            GUI.enabled = false;
            EditorGUILayout.ObjectField(m_CompiledRequest.ShaderObject, typeof(Shader), false);
            Rect rectNow = EditorGUILayout.GetControlRect();
            GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            GUI.enabled = true;
            float initialHeight = rectNow.y + 10 + 3 * EditorGUIUtility.singleLineHeight;
            Rect rect = new Rect(rectNow.x, initialHeight, position.width, position.height - initialHeight);
            GUILayout.BeginArea(rect, GUIContent.none, GUIStyle.none);
            m_ScrollViewVector = EditorGUILayout.BeginScrollView(m_ScrollViewVector);
            rect = GUILayoutUtility.GetRect(content, m_TextStyle);
            rect.x = 0;
            rect.y -= 3;
            rect.width = rect.width - 1;
            GUI.Box(rect, "");
            EditorGUI.SelectableLabel(rect, content.text, m_TextStyle);
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.backgroundColor = backgroundColorTemp;
            GUI.enabled = enabledTemp;
        }
        
        private static readonly Type FrameDebuggerWindowClass = typeof(EditorWindow).Assembly.GetType("UnityEditor.FrameDebuggerWindow");
    }
}
