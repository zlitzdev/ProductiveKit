using System;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.ProductiveKit
{
    public class ColorPalettePickerWindow : EditorWindow
    {
        private Action<Color> m_callback;
        private Color m_newColor;
        private bool m_newColorShowAlpha;
        private bool m_newColorHdr;

        private ColorField m_newColorField;

        private VisualElement m_colorsContainer;

        private ColorPaletteAsset m_colors;

        private SerializedObject m_serialzedColors;

        private SerializedProperty m_entriesProperty;

        public static void Open(Action<Color> callback, Color newColor, bool newColorShowAlpha, bool newColorHdr)
        {
            ColorPalettePickerWindow window = GetWindow<ColorPalettePickerWindow>("Color Palette");
            window.m_callback = callback;
            window.m_newColor = newColor;
            window.m_newColorShowAlpha = newColorShowAlpha;
            window.m_newColorHdr = newColorHdr;

            Vector2 mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            Rect newPosition = new Rect(mousePosition.x, mousePosition.y, 320.0f, 480.0f);
            window.position = newPosition;
        }

        private void OnEnable()
        {
            m_colors = ProductiveKitSettings.instance.colors;
            m_serialzedColors = m_colors == null ? null : new SerializedObject(m_colors);
            m_entriesProperty = m_serialzedColors == null ? null : m_serialzedColors.FindProperty("m_entries");

            minSize = new Vector2(480.0f, 480.0f);
            maxSize = new Vector2(480.0f, 960.0f);
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void CreateGUI()
        {
            if (m_serialzedColors == null)
            {
                HelpBox helpBox = new HelpBox("No available color palette to select from.", HelpBoxMessageType.Warning);
                rootVisualElement.Add(helpBox);
                return;
            }

            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1.0f;
            rootVisualElement.Add(scrollView);

            VisualElement contentContainer = new VisualElement();
            contentContainer.style.marginLeft = 8.0f;
            contentContainer.style.marginRight = 8.0f;
            scrollView.Add(contentContainer);

            Label colorsLabel = new Label("Colors");
            colorsLabel.style.marginTop = 8.0f;
            colorsLabel.style.marginBottom = 8.0f;
            colorsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            contentContainer.Add(colorsLabel);

            m_colorsContainer = new VisualElement();
            m_colorsContainer.style.marginLeft = 4.0f;
            m_colorsContainer.style.marginRight = 4.0f;
            contentContainer.Add(m_colorsContainer);

            Label newColorLabel = new Label("Add new color");
            newColorLabel.style.marginTop = 8.0f;
            newColorLabel.style.marginBottom = 8.0f;
            newColorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            contentContainer.Add(newColorLabel);

            VisualElement newColorContainer = new VisualElement();
            newColorContainer.style.marginLeft = 4.0f;
            newColorContainer.style.marginRight = 4.0f;
            contentContainer.Add(newColorContainer);

            newColorContainer.Add(MakeNewColorAdder());

            Rebuild();
            m_colorsContainer.schedule.Execute(Rebuild).Every(1000);
        }

        private void Rebuild()
        {
            if (m_newColorField != null)
            {
                m_newColorField.value = m_newColor;
            }

            if (m_colorsContainer == null)
            {
                return;
            }
            m_colorsContainer.Clear();

            for (int i = 0; i < m_entriesProperty.arraySize; i++)
            {
                m_colorsContainer.Add(MakeColorEntry(i));
            }
        }

        private VisualElement MakeColorEntry(int index)
        {
            SerializedProperty property = m_entriesProperty.GetArrayElementAtIndex(index);

            SerializedProperty nameProperty  = property.FindPropertyRelative("m_name");
            SerializedProperty colorProperty = property.FindPropertyRelative("m_color");

            bool showAlpha = true;
            SerializedProperty showAlphaProperty = property.FindPropertyRelative("m_showAlpha");
            if (showAlphaProperty != null)
            {
                showAlpha = showAlphaProperty.boolValue;
            }

            bool hdr = true;
            SerializedProperty hdrProperty = property.FindPropertyRelative("m_hdr");
            if (hdrProperty != null)
            {
                hdr = hdrProperty.boolValue;
            }

            bool conflicted = false;
            for (int i = 0; i < index; i++)
            {
                SerializedProperty siblingProperty = m_entriesProperty.GetArrayElementAtIndex(i);
                SerializedProperty siblingNameProperty = siblingProperty.FindPropertyRelative("m_name");

                if (siblingNameProperty.stringValue == nameProperty.stringValue)
                {
                    conflicted = true;
                    break;
                }
            }

            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.height = 20.0f;

            VisualElement labelContainer = new VisualElement();
            labelContainer.style.flexDirection = FlexDirection.Column;
            labelContainer.style.justifyContent = Justify.Center;
            labelContainer.style.width = Length.Percent(30.0f);
            labelContainer.style.height = Length.Percent(100.0f);
            root.Add(labelContainer);

            TextField label = new TextField();
            label.label = "";
            label.value = nameProperty.stringValue;
            label.SetEnabled(false);
            labelContainer.Add(label);

            VisualElement colorContainer = new VisualElement();
            colorContainer.style.flexDirection = FlexDirection.Column;
            colorContainer.style.justifyContent = Justify.Center;
            colorContainer.style.width = Length.Percent(50.0f);
            colorContainer.style.height = Length.Percent(100.0f);
            root.Add(colorContainer);

            ColorField color = new ColorField();
            color.value = colorProperty.colorValue;
            color.showAlpha = showAlpha;
            color.hdr = hdr;
            color.showEyeDropper = false;
            color.style.flexGrow = 1.0f;
            color.SetEnabled(false);
            colorContainer.Add(color);

            Button select = new Button(() =>
            {
                m_callback?.Invoke(colorProperty.colorValue);
                Close();
            });
            select.text = "Select";
            select.style.width = Length.Percent(20.0f);
            select.style.height = Length.Percent(100.0f);
            select.SetEnabled(!conflicted);
            root.Add(select);

            return root;
        }
    
        private VisualElement MakeNewColorAdder()
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.height = 20.0f;

            VisualElement labelContainer = new VisualElement();
            labelContainer.style.flexDirection = FlexDirection.Column;
            labelContainer.style.justifyContent = Justify.Center;
            labelContainer.style.width = Length.Percent(30.0f);
            labelContainer.style.height = Length.Percent(100.0f);
            root.Add(labelContainer);

            TextField label = new TextField();
            label.label = "";
            labelContainer.Add(label);

            VisualElement colorContainer = new VisualElement();
            colorContainer.style.flexDirection = FlexDirection.Column;
            colorContainer.style.justifyContent = Justify.Center;
            colorContainer.style.width = Length.Percent(50.0f);
            colorContainer.style.height = Length.Percent(100.0f);
            root.Add(colorContainer);

            ColorField color = new ColorField();
            color.value = m_newColor;
            color.showAlpha = m_newColorShowAlpha;
            color.hdr = m_newColorHdr;
            color.showEyeDropper = false;
            color.style.flexGrow = 1.0f;
            color.SetEnabled(false);
            colorContainer.Add(color);

            Button add = new Button(() =>
            {
                int index = m_entriesProperty.arraySize;

                m_entriesProperty.InsertArrayElementAtIndex(index);
                m_serialzedColors.ApplyModifiedProperties();

                SerializedProperty newEntryProperty = m_entriesProperty.GetArrayElementAtIndex(index);

                SerializedProperty nameProperty = newEntryProperty.FindPropertyRelative("m_name");
                SerializedProperty colorProperty = newEntryProperty.FindPropertyRelative("m_color");
                SerializedProperty showAlphaProperty = newEntryProperty.FindPropertyRelative("m_showAlpha");
                SerializedProperty hdrProperty = newEntryProperty.FindPropertyRelative("m_hdr");

                nameProperty.stringValue = label.value;
                colorProperty.colorValue = m_newColor;

                if (showAlphaProperty != null)
                {
                    showAlphaProperty.boolValue = m_newColorShowAlpha;
                }

                if (hdrProperty != null)
                {
                    hdrProperty.boolValue = m_newColorHdr;
                }

                m_serialzedColors.ApplyModifiedProperties();
                Close();
            });
            add.text = "Add";
            add.style.width = Length.Percent(20.0f);
            add.style.height = Length.Percent(100.0f);
            root.Add(add);

            m_newColorField = color;

            return root;
        }
    }
}
