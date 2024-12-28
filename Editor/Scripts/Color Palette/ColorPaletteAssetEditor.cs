using System;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.ProductiveKit
{
    [CustomEditor(typeof(ColorPaletteAsset))]
    public class ColorPaletteAssetEditor : Editor
    {
        private SerializedProperty m_entriesProperty;

        private void OnEnable()
        {
            m_entriesProperty = serializedObject.FindProperty("m_entries");
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            ListView listView = new ListView();
            listView.showAddRemoveFooter = true;
            listView.showBoundCollectionSize = true;
            listView.showFoldoutHeader = true;
            listView.headerTitle = "Colors";
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;

            listView.makeItem = () => new ColorPaletteEntry();

            listView.bindItem = (e, i) =>
            {
                if (e is ColorPaletteEntry colorPaletteEntry)
                {
                    colorPaletteEntry.Bind(m_entriesProperty, i, () =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            listView.RefreshItems();
                        };
                    });
                }
            };

            listView.BindProperty(m_entriesProperty);

            root.Add(listView);

            return root;
        }

        private class ColorPaletteEntry : VisualElement
        {
            private static Texture2D s_warningIcon = EditorGUIUtility.IconContent("Warning@2x").image as Texture2D;

            private SerializedProperty m_entriesProperty;
            private int m_index;

            private SerializedProperty m_property;
            private SerializedProperty m_nameProperty;
            private SerializedProperty m_colorProperty;
            private SerializedProperty m_showAlphaProperty;
            private SerializedProperty m_hdrProperty;

            private Action m_onNameChanged;

            private TextField m_nameField;

            private ColorField m_colorField;

            private Button m_showAlpha;
            private Button m_hdr;

            private VisualElement m_conflictIcon;

            public void Bind(SerializedProperty entriesProperty, int index, Action onNameChanged)
            {
                m_entriesProperty = entriesProperty;
                m_index = index;

                m_property = m_entriesProperty.GetArrayElementAtIndex(m_index);

                m_onNameChanged = null;

                m_nameProperty = m_property.FindPropertyRelative("m_name");
                m_nameField.BindProperty(m_nameProperty);

                m_colorProperty = m_property.FindPropertyRelative("m_color");
                m_colorField.BindProperty(m_colorProperty);

                m_showAlphaProperty = m_property.FindPropertyRelative("m_showAlpha");
                if (m_showAlphaProperty != null)
                {
                    m_colorField.showAlpha = m_showAlphaProperty.boolValue;
                    m_showAlpha.text    = m_showAlphaProperty.boolValue ? "RGBA" : "RGB";
                    m_showAlpha.tooltip = m_showAlphaProperty.boolValue ? "Currently showing alpha." : "Currently hiding alpha.";

                    m_showAlpha.SetEnabled(true);
                }
                else
                {
                    m_colorField.showAlpha = true;
                    m_showAlpha.text    = "RGBA";
                    m_showAlpha.tooltip = "Currently showing alpha.";

                    m_showAlpha.SetEnabled(false);
                }

                m_hdrProperty = m_property.FindPropertyRelative("m_hdr");
                if (m_hdrProperty != null)
                {
                    m_colorField.hdr = m_hdrProperty.boolValue;
                    m_hdr.text = m_hdrProperty.boolValue ? "HDR" : "SDR";
                    m_hdr.tooltip = m_hdrProperty.boolValue ? "Currently using HDR." : "Currently not using HDR.";

                    m_hdr.SetEnabled(true);
                }
                else
                {
                    m_colorField.hdr = false;
                    m_hdr.text = "SDR";
                    m_hdr.tooltip = "Currently not using HDR.";

                    m_hdr.SetEnabled(false);
                }

                m_conflictIcon.style.backgroundImage = null;
                m_conflictIcon.tooltip = null;
                for (int i = 0; i < m_entriesProperty.arraySize; i++)
                {
                    if (i == m_index)
                    {
                        continue;
                    }

                    SerializedProperty siblingProperty = m_entriesProperty.GetArrayElementAtIndex(i);
                    SerializedProperty siblingNameProperty = siblingProperty.FindPropertyRelative("m_name");

                    if (siblingNameProperty.stringValue == m_nameProperty.stringValue)
                    {
                        m_conflictIcon.style.backgroundImage = s_warningIcon;
                        m_conflictIcon.tooltip = "Duplicated names will be ignored";

                        break;
                    }
                }

                m_onNameChanged = onNameChanged;
            }

            public ColorPaletteEntry()
            {
                AlignedField aligned = new AlignedField();
                Add(aligned);

                m_nameField = new TextField();
                m_nameField.label = "";
                m_nameField.style.flexGrow = 1.0f;
                m_nameField.RegisterValueChangedCallback(OnNameChanged);
                aligned.labelContainer.Add(m_nameField);

                m_colorField = new ColorField();
                m_colorField.label = "";
                m_colorField.style.flexGrow = 1.0f;
                aligned.fieldContainer.Add(m_colorField);

                m_showAlpha = new Button(() =>
                {
                    if (m_showAlphaProperty != null && m_colorField != null)
                    {
                        m_showAlphaProperty.boolValue = !m_showAlphaProperty.boolValue;
                        m_showAlphaProperty.serializedObject.ApplyModifiedProperties();
                        m_colorField.showAlpha = m_showAlphaProperty.boolValue;

                        m_showAlpha.text    = m_showAlphaProperty.boolValue ? "RGBA" : "RGB";
                        m_showAlpha.tooltip = m_showAlphaProperty.boolValue ? "Currently showing alpha." : "Currently hiding alpha.";
                    }
                });
                m_showAlpha.style.marginRight = -2.0f;
                m_showAlpha.style.width = 40.0f;
                aligned.fieldContainer.Add(m_showAlpha);

                m_hdr = new Button(() =>
                {
                    if (m_hdrProperty != null && m_colorField != null)
                    {
                        m_hdrProperty.boolValue = !m_hdrProperty.boolValue;
                        m_hdrProperty.serializedObject.ApplyModifiedProperties();
                        m_colorField.hdr = m_hdrProperty.boolValue;

                        m_hdr.text = m_hdrProperty.boolValue ? "HDR" : "SDR";
                        m_hdr.tooltip = m_hdrProperty.boolValue ? "Currently using HDR." : "Currently not using HDR.";
                    }
                });
                m_hdr.style.marginRight = -2.0f;
                m_hdr.style.width = 40.0f;
                aligned.fieldContainer.Add(m_hdr);

                m_conflictIcon = new VisualElement();
                m_conflictIcon.style.minWidth   = 22.0f;
                m_conflictIcon.style.marginLeft = 4.0f;
                aligned.fieldContainer.Add(m_conflictIcon);
            }

            private void OnNameChanged(ChangeEvent<string> e)
            {
                m_onNameChanged?.Invoke();
            }
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
                fieldContainer.style.justifyContent = Justify.FlexEnd;
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

                bool notInInspector = false;
                if (m_cachedInspectorElement == null || m_cachedContextWidthElement == null)
                {
                    notInInspector = true;
                    m_cachedInspectorElement    = parent;
                    m_cachedContextWidthElement = parent;
                }

                if (m_cachedInspectorElement == null)
                {
                    RemoveFromClassList("unity-base-field__inspector-field");
                    return;
                }

                m_labelWidthRatio = notInInspector ? 0.3f : 0.45f;

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
