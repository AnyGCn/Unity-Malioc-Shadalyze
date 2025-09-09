using System.Collections;
using System.Collections.Generic;
using Shadalyze.Editor;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ShadalyzeSettingsProvider : SettingsProvider
{
    Editor m_Editor;

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider() => new ShadalyzeSettingsProvider();
    
    public ShadalyzeSettingsProvider() : base("Project/Shadalyze", SettingsScope.Project)
    {
    }
    
    void DestroyEditor()
    {
        if (m_Editor == null)
            return;

        UnityEngine.Object.DestroyImmediate(m_Editor);
        m_Editor = null;
    }

    /// <summary>
    /// This method is being called when the provider is activated
    /// </summary>
    /// <param name="searchContext">The context with the search</param>
    /// <param name="rootElement">The <see cref="VisualElement"/> with the root</param>
    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        DestroyEditor();
        base.OnActivate(searchContext, rootElement);
    }

    /// <summary>
    /// This method is being called when the provider is deactivated
    /// </summary>
    public override void OnDeactivate()
    {
        DestroyEditor();
        base.OnDeactivate();
    }
    
    /// <summary>
    /// Method called to render the IMGUI of the settings provider
    /// </summary>
    /// <param name="searchContext">The search content</param>
    public override void OnGUI(string searchContext)
    {
        if (m_Editor != null && (m_Editor.target == null || m_Editor.target != ShadalyzeGlobalSettings.Instance))
            DestroyEditor();

        if (m_Editor == null)
            m_Editor = Editor.CreateEditor(ShadalyzeGlobalSettings.Instance);

        m_Editor?.OnInspectorGUI();

        base.OnGUI(searchContext);
    }
}
