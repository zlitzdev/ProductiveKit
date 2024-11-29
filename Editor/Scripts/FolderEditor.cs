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

            string folderIconGuid;
            (folderIconGuid, m_tintColor) = FolderIcon.GetOrDefault(assetPath);
            m_folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(folderIconGuid));

            ObjectField folderIconField = new ObjectField("Folder Icon");
            folderIconField.AddToClassList("unity-base-field__aligned");
            folderIconField.objectType = typeof(Texture2D);
            folderIconField.value = m_folderIcon;
            root.Add(folderIconField);

            ColorField tintColorField = new ColorField("Tint Color");
            tintColorField.AddToClassList("unity-base-field__aligned");
            tintColorField.showAlpha = false;
            tintColorField.value = m_tintColor;
            root.Add(tintColorField);

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
