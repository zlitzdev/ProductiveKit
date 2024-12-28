using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Drawing.Imaging;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.ProductiveKit
{
    internal class ProductiveKitSettingsProvider : SettingsProvider
    {
        private SerializedObject m_serializedSettings;

        private SerializedProperty m_colorsProperty;

        private Editor m_inlineColorsEditor;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            if (m_serializedSettings == null && ProductiveKitSettings.instance != null)
            {
                m_serializedSettings = new SerializedObject(ProductiveKitSettings.instance);

                m_colorsProperty = m_serializedSettings.FindProperty("m_colors");
            }

            VisualElement root = new VisualElement();
            root.style.marginLeft = 10.0f;
            rootElement.Add(root);
            rootElement = root;

            if (m_serializedSettings == null)
            {
                HelpBox helpBox = new HelpBox("No ProductiveKitSettings found.", HelpBoxMessageType.Error);
                rootElement.Add(helpBox);
                return;
            }

            Label settingTitle = new Label("Productive Kit");
            settingTitle.style.fontSize = 20.0f;
            settingTitle.style.marginBottom = 6.0f;
            settingTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            rootElement.Add(settingTitle);

            VisualElement settingContent = new VisualElement();
            settingContent.style.marginLeft = 10.0f;
            rootElement.Add(settingContent);

            Label colorTitle = new Label("Color Palette");
            colorTitle.style.fontSize = 16.0f;
            colorTitle.style.marginBottom = 4.0f;
            colorTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            settingContent.Add(colorTitle);

            VisualElement colorContent = new VisualElement();
            colorContent.style.marginLeft = 10.0f;
            settingContent.Add(colorContent); ;

            PropertyField colorsField = new PropertyField();
            colorsField.label = "Colors";
            colorsField.BindProperty(m_colorsProperty);
            colorContent.Add(colorsField);

            VisualElement inlineColorEditorContainer = new VisualElement();

            m_inlineColorsEditor = m_colorsProperty.objectReferenceValue == null
                ? null
                : Editor.CreateEditor(m_colorsProperty.objectReferenceValue, typeof(ColorPaletteAssetEditor));

            inlineColorEditorContainer.Clear();
            if (m_inlineColorsEditor != null)
            {
                VisualElement inlineEditor = m_inlineColorsEditor.CreateInspectorGUI();
                if (inlineEditor != null)
                {
                    inlineColorEditorContainer.Add(inlineEditor);
                }
            }

            colorContent.Add(inlineColorEditorContainer);

            colorsField.RegisterValueChangeCallback(e =>
            {
                if (m_inlineColorsEditor == null || m_inlineColorsEditor.target != m_colorsProperty.objectReferenceValue)
                {
                    m_inlineColorsEditor = m_colorsProperty.objectReferenceValue == null
                    ? null
                    : Editor.CreateEditor(m_colorsProperty.objectReferenceValue, typeof(ColorPaletteAssetEditor));

                    inlineColorEditorContainer.Clear();
                    if (m_inlineColorsEditor != null)
                    {
                        VisualElement inlineEditor = m_inlineColorsEditor.CreateInspectorGUI();
                        if (inlineEditor != null)
                        {
                            inlineColorEditorContainer.Add(inlineEditor);
                        }
                    }
                }
            });
        }

        public ProductiveKitSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateProductiveKitSettingsProvider()
        {
            return new ProductiveKitSettingsProvider("Project/Zlitz/Productive Kit", SettingsScope.Project);
        }
    }

    [InitializeOnLoad]
    internal static class ProductiveKitSettingsInitializer
    {
        static ProductiveKitSettingsInitializer()
        {
            ProductiveKitSettings instance = ProductiveKitSettings.instance;
        }
    }
}
