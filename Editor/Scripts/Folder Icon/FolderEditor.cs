using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.ProductiveKit
{
    [CustomEditor(typeof(DefaultAsset))]
    public class FolderEditor : Editor
    {
        private Texture2D m_folderIcon;
        private Color     m_tintColor;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            string assetPath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(assetPath) || !AssetDatabase.IsValidFolder(assetPath))
            {
                return root;
            }

            VisualElement hasNoIconConfigContent = new VisualElement();
            hasNoIconConfigContent.style.flexGrow = 1.0f;
            root.Add(hasNoIconConfigContent);

            VisualElement hasIconConfigContent = new VisualElement();
            hasIconConfigContent.style.flexGrow = 1.0f;
            root.Add(hasIconConfigContent);

            // No icon config yet

            Button addIconConfigButton = new Button(() =>
            {
                hasNoIconConfigContent.style.display = DisplayStyle.None;
                hasIconConfigContent.style.display = DisplayStyle.Flex;
                FolderIcon.SetDefault(assetPath);
            });
            addIconConfigButton.text = "Add Folder Icon";
            addIconConfigButton.SetEnabled(assetPath.StartsWith("Assets"));
            hasNoIconConfigContent.Add(addIconConfigButton);

            // Has icon config

            string folderIconGuid;
            (folderIconGuid, m_tintColor) = FolderIcon.GetOrDefault(assetPath);
            m_folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(folderIconGuid));

            ObjectField folderIconField = new ObjectField("Folder Icon");
            folderIconField.AddToClassList("unity-base-field__aligned");
            folderIconField.objectType = typeof(Texture2D);
            folderIconField.value = m_folderIcon;
            hasIconConfigContent.Add(folderIconField);

            ColorField tintColorField = new ColorField("Tint Color");
            tintColorField.AddToClassList("unity-base-field__aligned");
            tintColorField.showAlpha = false;
            tintColorField.value = m_tintColor;
            hasIconConfigContent.Add(tintColorField);

            Button removeIconConfigButton = new Button(() =>
            {
                hasNoIconConfigContent.style.display = DisplayStyle.Flex;
                hasIconConfigContent.style.display = DisplayStyle.None;
                FolderIcon.Remove(assetPath);
            });
            removeIconConfigButton.text = "Remove Folder Icon";
            hasIconConfigContent.Add(removeIconConfigButton);

            folderIconField.RegisterValueChangedCallback(e =>
            {
                if (e.newValue is Texture2D folderIcon)
                {
                    m_folderIcon = folderIcon;
                    FolderIcon.Set(assetPath, AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_folderIcon)).ToString(), m_tintColor);
                }
            });

            tintColorField.RegisterValueChangedCallback(e =>
            {
                m_tintColor = e.newValue;
                FolderIcon.Set(assetPath, AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_folderIcon)).ToString(), m_tintColor);
            });

            if (FolderIcon.TryGet(assetPath, out (string, Color) folderIconAndTint))
            {
                hasNoIconConfigContent.style.display = DisplayStyle.None;
                hasIconConfigContent.style.display = DisplayStyle.Flex;
            }
            else
            {
                hasNoIconConfigContent.style.display = DisplayStyle.Flex;
                hasIconConfigContent.style.display = DisplayStyle.None;
            }

            return root;
        }

        protected override void OnHeaderGUI()
        {
            Color guiColor = GUI.contentColor;
            GUI.contentColor = new Color(guiColor.r * m_tintColor.r, guiColor.g * m_tintColor.g, guiColor.b * m_tintColor.b, guiColor.a);

            base.OnHeaderGUI();

            GUI.contentColor = guiColor;
        }

        
    }
}
