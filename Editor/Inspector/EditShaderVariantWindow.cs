using System.Collections.Generic;
using System.Linq;
using Shadalyze.Editor.Data;
using Shadalyze.Editor.Manager;
using Shadalyze.Editor.Wrapper;
using UnityEditor;
using UnityEngine;

namespace Shadalyze.Editor
{
    /// <summary>
    /// Editor window that Choose variants for compilation, source code copy from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/ShaderVariantCollectionInspector.cs
    /// </summary>
    internal class EditShaderVariantWindow : EditorWindow
    {
        internal class PopupData
        {
            public Shader shader;
            public ShaderVariantCollection collection;
        }

        class Styles
        {
            public static readonly GUIStyle sMenuItem = "MenuItem";
            public static readonly GUIStyle sSeparator = "sv_iconselector_sep";
        }

        const float kMargin = 2;
        const float kSpaceHeight = 6;
        const float kSeparatorHeight = 3;

        private const float kMinWindowWidth = 400;
        static readonly float kMiscUIHeight =
            5 * EditorGUIUtility.singleLineHeight +
            kSeparatorHeight * 2 +
            kSpaceHeight * 5 +
            kMargin * 2;

        private static readonly float kMinWindowHeight = 9 * EditorGUIUtility.singleLineHeight + kMiscUIHeight;

        PopupData m_Data;
        List<string> m_SelectedKeywords;
        List<string> m_AvailableKeywords;
        List<int> m_SelectedVariants; // Indices of variants currently selected for adding

        int[] m_FilteredVariantTypes;
        string[][] m_FilteredVariantKeywords;

        int m_MaxVisibleVariants;
        int m_NumFilteredVariants;

        public EditShaderVariantWindow()
        {
            position = new Rect(100, 100, kMinWindowWidth * 1.5f, kMinWindowHeight * 1.5f);
            minSize = new Vector2(kMinWindowWidth, kMinWindowHeight);
            wantsMouseMove = true;
        }

        private void Initialize(Shader shader, string[] initialKeywords)
        {
            m_SelectedKeywords = new List<string>();
            m_AvailableKeywords = new List<string>();
            m_SelectedVariants = new List<int>();
            m_Data = new PopupData()
            {
                shader = shader,
                collection = new ShaderVariantCollection(),
            };

            if (initialKeywords != null)
            {
                m_SelectedKeywords.AddRange(initialKeywords);
                m_SelectedVariants.Add(0);
            }
            
            ApplyKeywordFilter();
        }

        private void OnDestroy()
        {
            if (m_Data?.collection)
                DestroyImmediate(m_Data.collection);
        }

        public static void Show(Shader shader, string[] initialKeywords)
        {
            var w = EditorWindow.GetWindow<EditShaderVariantWindow>(true, "Analyze shader " + shader.name + " variants");
            w.Initialize(shader, initialKeywords);
        }

        void ApplyKeywordFilter()
        {
            m_MaxVisibleVariants = (int)(CalcVerticalSpaceForVariants() / EditorGUIUtility.singleLineHeight);
            string[] keywordLists, remainingKeywords;
            m_FilteredVariantTypes = new int[m_MaxVisibleVariants];

            ShaderUtilWrapper.GetShaderVariantEntriesFiltered(m_Data.shader,
                m_MaxVisibleVariants + 1,                                         // query one more to know if we're truncating
                m_SelectedKeywords.ToArray(),
                m_Data.collection,
                out m_FilteredVariantTypes,
                out keywordLists,
                out remainingKeywords);

            m_NumFilteredVariants = m_FilteredVariantTypes.Length;
            m_FilteredVariantKeywords = new string[m_NumFilteredVariants][];
            for (var i = 0; i < m_NumFilteredVariants; ++i)
            {
                m_FilteredVariantKeywords[i] = keywordLists[i].Split(' ');
            }

            m_AvailableKeywords.Clear();
            m_AvailableKeywords.InsertRange(0, remainingKeywords);
            m_AvailableKeywords.Sort();
        }

        public void OnGUI()
        {
            // Objects became deleted while our window was showing? Close.
            if (m_Data == null || m_Data.shader == null || m_Data.collection == null)
            {
                Close();
                return;
            }

            // We do not use the layout event
            if (Event.current.type == EventType.Layout)
                return;

            Rect rect = new Rect(0, 0, position.width, position.height);
            Draw(rect);

            // Repaint on mouse move so we get hover highlights in menu item rows
            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private bool KeywordButton(Rect buttonRect, string k, Vector2 areaSize)
        {
            // If we can't fit all buttons (shader has *a lot* of keywords) and would start clipping,
            // do display the partially clipped ones with some transparency.
            var oldColor = GUI.color;
            if (buttonRect.yMax > areaSize.y)
                GUI.color = new Color(1, 1, 1, 0.4f);

            var result = GUI.Button(buttonRect, EditorGUIUtility.TrTempContent(k), EditorStyles.miniButton);
            GUI.color = oldColor;
            return result;
        }

        float CalcVerticalSpaceForKeywords()
        {
            return Mathf.Floor((position.height - kMiscUIHeight) / 4);
        }

        float CalcVerticalSpaceForVariants()
        {
            return (position.height - kMiscUIHeight) / 2;
        }

        void DrawKeywordsList(ref Rect rect, List<string> keywords, bool clickingAddsToSelected)
        {
            rect.height = CalcVerticalSpaceForKeywords();
            var displayKeywords = keywords.Select(k => k.ToLowerInvariant()).ToList();

            GUI.BeginGroup(rect);
            Rect indentRect = new Rect(4, 0, rect.width, rect.height);
            var layoutRects = EditorGUIUtility.GetFlowLayoutedRects(indentRect, EditorStyles.miniButton, 2, 2, displayKeywords);
            for (var i = 0; i < displayKeywords.Count; ++i)
            {
                if (KeywordButton(layoutRects[i], displayKeywords[i], rect.size))
                {
                    if (clickingAddsToSelected)
                    {
                        if (!m_SelectedKeywords.Contains(keywords[i]))
                        {
                            m_SelectedKeywords.Add(keywords[i]);
                            m_SelectedKeywords.Sort();
                            m_AvailableKeywords.Remove(keywords[i]);
                        }
                    }
                    else
                    {
                        m_AvailableKeywords.Add(keywords[i]);
                        m_SelectedKeywords.Remove(keywords[i]);
                    }
                    ApplyKeywordFilter();
                    GUIUtility.ExitGUI();
                }
            }
            GUI.EndGroup();
            rect.y += rect.height;
        }

        void DrawSectionHeader(ref Rect rect, string titleString, bool separator)
        {
            // space
            rect.y += kSpaceHeight;
            // separator
            if (separator)
            {
                rect.height = kSeparatorHeight;
                GUI.Label(rect, GUIContent.none, Styles.sSeparator);
                rect.y += rect.height;
            }
            // label
            rect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(rect, titleString);
            rect.y += rect.height;
        }

        private void Draw(Rect windowRect)
        {
            var rect = new Rect(kMargin, kMargin, windowRect.width - kMargin * 2, EditorGUIUtility.singleLineHeight);

            DrawSectionHeader(ref rect, "Pick shader keywords to narrow down variant list:", false);
            DrawKeywordsList(ref rect, m_AvailableKeywords, true);

            DrawSectionHeader(ref rect, "Selected keywords:", true);
            DrawKeywordsList(ref rect, m_SelectedKeywords, false);

            DrawSectionHeader(ref rect, "Shader variants with these keywords (click to select):", true);

            if (m_NumFilteredVariants > 0)
            {
                int maxFilteredLength = (int)(CalcVerticalSpaceForVariants() / EditorGUIUtility.singleLineHeight);

                if (maxFilteredLength > m_MaxVisibleVariants) // Query data again if we have bigger window than at last query
                    ApplyKeywordFilter();

                // Display first N variants (don't want to display thousands of them if filter is not narrow)
                for (var i = 0; i < Mathf.Min(m_NumFilteredVariants, maxFilteredLength); ++i)
                {
                    var passType = (UnityEngine.Rendering.PassType)m_FilteredVariantTypes[i];
                    var wasSelected = m_SelectedVariants.Contains(i);
                    var keywordString = string.IsNullOrEmpty(m_FilteredVariantKeywords[i][0]) ? "<no keywords>" : string.Join(" ", m_FilteredVariantKeywords[i]);
                    var displayString = passType + " " + keywordString.ToLowerInvariant();
                    var isSelected = GUI.Toggle(rect, wasSelected, displayString, Styles.sMenuItem);
                    rect.y += rect.height;

                    if (isSelected && !wasSelected)
                        m_SelectedVariants.Add(i);
                    else if (!isSelected && wasSelected)
                        m_SelectedVariants.Remove(i);
                }

                // show how many variants we skipped due to filter not being narrow enough
                if (m_NumFilteredVariants > maxFilteredLength)
                {
                    GUI.Label(rect, "List of variants was cropped. Pick further keywords to narrow the selection.", EditorStyles.miniLabel);
                    rect.y += rect.height;
                }
            }
            else
            {
                GUI.Label(rect, "No variants with these keywords");
                rect.y += rect.height;
            }

            // Button to add them at the bottom of popup
            rect.y = windowRect.height - kMargin - kSpaceHeight - EditorGUIUtility.singleLineHeight;
            rect.height = EditorGUIUtility.singleLineHeight;
            // Disable button if no variants selected
            using (new EditorGUI.DisabledScope(m_SelectedVariants.Count == 0))
            {
                if (GUI.Button(rect, string.Format("Add {0} selected variants", m_SelectedVariants.Count)))
                {
                    // Add the selected variants
                    for (var i = 0; i < m_SelectedVariants.Count; ++i)
                    {
                        var index = m_SelectedVariants[i];
                        var variant = new ShaderVariantCollection.ShaderVariant(m_Data.shader, (UnityEngine.Rendering.PassType)m_FilteredVariantTypes[index], m_FilteredVariantKeywords[index]);
                        m_Data.collection.Add(variant);
                    }
                    // Close our popup
                    var compileRequests = new List<ShaderCompileRequest>();
                    ShaderCompileRequest.GetShaderCompileData(m_Data.collection, compileRequests);
                    foreach (var compileRequest in compileRequests)
                    {
                        compileRequest.Compile();
                        string result = compileRequest.Analyze();
                        string reportPath = $"{ShadalyzeGlobalSettings.CompileCodePath}/{compileRequest.ShaderObject.name.Replace('/', '-')}-{compileRequest.PassName}-{compileRequest.sha256}.txt";
                        ShaderCompileDataManager.DumpToFile(reportPath, result, result.Length);
                        if (!string.IsNullOrEmpty(reportPath))
                            Application.OpenURL("file://" + reportPath);
                    }
                    Close();
                    GUIUtility.ExitGUI();
                }
            }
        }
    }
}
