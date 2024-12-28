using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zlitz.General.ProductiveKit
{
    [CustomPropertyDrawer(typeof(Color))]
    [CustomPropertyDrawer(typeof(ColorUsageAttribute))]
    public class ColorPalettePickerDrawer : PropertyDrawer
    {
        private static Texture2D s_paletteIcon;

        private static Texture2D paletteIcon
        {
            get
            {
                if (s_paletteIcon == null)
                {
                    s_paletteIcon = Resources.Load<Texture2D>("Color Palette Icons/Icon_Palette");
                }

                return s_paletteIcon;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            AlignedField root = new AlignedField();

            Label label = new Label(property.displayName);
            label.style.flexGrow = 1.0f;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            root.labelContainer.Add(label);

            if (property.propertyType != SerializedPropertyType.Color)
            {
                Label message = new Label("This must be a Color field.");
                message.style.flexGrow = 1.0f;
                message.style.color = Color.yellow;
                root.fieldContainer.Add(message);
            }
            else
            {
                ColorField colorField = new ColorField();
                colorField.hdr       = false;
                colorField.showAlpha = true;

                if (attribute is ColorUsageAttribute colorUsage)
                {
                    colorField.hdr       = colorUsage.hdr;
                    colorField.showAlpha = colorUsage.showAlpha;
                }

                colorField.style.flexGrow = 1.0f;
                colorField.BindProperty(property);
                root.fieldContainer.Add(colorField);

                Button paletteButton = new Button();
                paletteButton.style.width = 28.0f;
                paletteButton.style.marginRight = -1.0f;
                paletteButton.style.marginLeft = 4.0f;
                paletteButton.style.height = 18.0f;
                root.fieldContainer.Add(paletteButton);

                Image paletteButtonIcon = new Image();
                paletteButtonIcon.image = paletteIcon;
                paletteButtonIcon.scaleMode = ScaleMode.ScaleToFit;
                paletteButtonIcon.style.flexGrow = 1.0f;
                paletteButton.Add(paletteButtonIcon);

                paletteButton.clicked += () =>
                {
                    if (!Colors.HasColorPalette())
                    {
                        paletteButton.SetEnabled(false);
                        return;
                    }

                    ColorPalettePickerWindow.Open(color =>
                    {
                        property.colorValue = color;
                        property.serializedObject.ApplyModifiedProperties();
                    }, colorField.value, colorField.showAlpha, colorField.hdr);
                };

                root.schedule.Execute(() =>
                {
                    paletteButton.SetEnabled(Colors.HasColorPalette());
                }).Every(100);
            }
            
            return root;
        }

        private class AlignedField : VisualElement
        {
            public VisualElement labelContainer { get; private set; }

            public VisualElement fieldContainer { get; private set; }

            private float m_labelWidthRatio;
            private float m_labelExtraPadding;
            private float m_labelBaseMinWidth;
            private float m_labelExtraContextWidth;

            private VisualElement m_cachedContextWidthElement;
            private VisualElement m_cachedInspectorElement;

            public AlignedField()
            {
                style.minHeight = EditorGUIUtility.singleLineHeight;
                style.marginLeft = 3.0f;

                VisualElement top = new VisualElement();
                top.style.flexDirection = FlexDirection.Row;
                top.style.minHeight = 20.0f;
                Add(top);

                labelContainer = new VisualElement();
                labelContainer.style.flexDirection = FlexDirection.Column;
                labelContainer.style.justifyContent = Justify.Center;
                top.Add(labelContainer);

                fieldContainer = new VisualElement();
                fieldContainer.style.flexDirection = FlexDirection.Row;
                fieldContainer.style.flexGrow = 1.0f;
                top.Add(fieldContainer);

                RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            }

            private void OnAttachToPanel(AttachToPanelEvent e)
            {
                if (e.destinationPanel == null)
                {
                    return;
                }

                if (e.destinationPanel.contextType == ContextType.Player)
                {
                    return;
                }

                m_cachedInspectorElement = null;
                m_cachedContextWidthElement = null;

                var currentElement = parent;
                while (currentElement != null)
                {
                    if (currentElement.ClassListContains("unity-inspector-element"))
                    {
                        m_cachedInspectorElement = currentElement;
                    }

                    if (currentElement.ClassListContains("unity-inspector-main-container"))
                    {
                        m_cachedContextWidthElement = currentElement;
                        break;
                    }

                    currentElement = currentElement.parent;
                }

                if (m_cachedInspectorElement == null)
                {
                    RemoveFromClassList("unity-base-field__inspector-field");
                    return;
                }

                m_labelWidthRatio = 0.45f;

                m_labelExtraPadding = 37.0f;
                m_labelBaseMinWidth = 123.0f;

                m_labelExtraContextWidth = 1.0f;

                RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
                AddToClassList("unity-base-field__inspector-field");
                RegisterCallback<GeometryChangedEvent>(OnInspectorFieldGeometryChanged);
            }

            private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
            {
                AlignLabel();
            }

            private void OnInspectorFieldGeometryChanged(GeometryChangedEvent e)
            {
                AlignLabel();
            }

            private void AlignLabel()
            {
                if (labelContainer == null)
                {
                    return;
                }

                float totalPadding = m_labelExtraPadding;
                float spacing = worldBound.x - m_cachedInspectorElement.worldBound.x - m_cachedInspectorElement.resolvedStyle.paddingLeft;

                totalPadding += spacing;
                totalPadding += resolvedStyle.paddingLeft;

                var minWidth = m_labelBaseMinWidth - spacing - resolvedStyle.paddingLeft;
                var contextWidthElement = m_cachedContextWidthElement ?? m_cachedInspectorElement;

                labelContainer.style.minWidth = Mathf.Max(minWidth, 0);

                var newWidth = (contextWidthElement.resolvedStyle.width + m_labelExtraContextWidth) * m_labelWidthRatio - totalPadding;
                if (Mathf.Abs(labelContainer.resolvedStyle.width - newWidth) > 1E-30f)
                {
                    labelContainer.style.width = Mathf.Max(0f, newWidth);
                }
            }
        }
    }
}
