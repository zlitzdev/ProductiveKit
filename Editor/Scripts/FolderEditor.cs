using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Zlitz.General.ProductiveKit
{
    [CustomEditor(typeof(DefaultAsset))]
    public class FolderEditor : Editor
    {
        private SerializedObject m_serializedIconAsset;

        private PropertyField m_iconAssetFolderIconProperty;
        private PropertyField m_iconAssetTintColorProperty;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            string assetPath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(assetPath) || !AssetDatabase.IsValidFolder(assetPath))
            {
                return root;
            }

            VisualElement inlineIconAssetEditor = new VisualElement();

            m_iconAssetFolderIconProperty = new PropertyField();
            inlineIconAssetEditor.Add(m_iconAssetFolderIconProperty);

            m_iconAssetTintColorProperty = new PropertyField();
            inlineIconAssetEditor.Add(m_iconAssetTintColorProperty);

            inlineIconAssetEditor.style.display = DisplayStyle.None;
            root.Add(inlineIconAssetEditor);

            Button addIconAssetButton = new Button();
            addIconAssetButton.text = "Add Folder Icon";
            addIconAssetButton.SetEnabled(assetPath.StartsWith("Assets/"));
            addIconAssetButton.clicked += () =>
            {
                FolderIconAsset iconAsset = CreateInstance<FolderIconAsset>();
                iconAsset.name = "FolderIconAsset";

                string iconAssetPath = Path.Combine(assetPath, iconAsset.name + ".asset");
                AssetDatabase.CreateAsset(iconAsset, iconAssetPath);
                AssetDatabase.SaveAssets();

                m_serializedIconAsset = new SerializedObject(iconAsset);

                SerializedProperty folderIconProperty = m_serializedIconAsset.FindProperty("m_folderIcon");
                m_iconAssetFolderIconProperty.BindProperty(folderIconProperty);

                SerializedProperty tintColorProperty = m_serializedIconAsset.FindProperty("m_tintColor");
                m_iconAssetTintColorProperty.BindProperty(tintColorProperty);

                inlineIconAssetEditor.style.display = DisplayStyle.Flex;
                addIconAssetButton.style.display = DisplayStyle.None;
            };
            addIconAssetButton.style.display = DisplayStyle.Flex;
            root.Add(addIconAssetButton);

            string[] folderIconAssetGuids = AssetDatabase.FindAssets("t:FolderIconAsset", new string[] { assetPath });
            folderIconAssetGuids = folderIconAssetGuids.Where(id => Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(id)).Replace("\\", "/") == assetPath).ToArray();

            if (folderIconAssetGuids.Length > 0)
            {
                string folderIconAssetPath = AssetDatabase.GUIDToAssetPath(folderIconAssetGuids[0]);

                FolderIconAsset folderIconAsset = AssetDatabase.LoadAssetAtPath<FolderIconAsset>(folderIconAssetPath);
                if (folderIconAsset != null)
                {
                    m_serializedIconAsset = new SerializedObject(folderIconAsset);

                    SerializedProperty folderIconProperty = m_serializedIconAsset.FindProperty("m_folderIcon");
                    m_iconAssetFolderIconProperty.BindProperty(folderIconProperty);

                    SerializedProperty tintColorProperty = m_serializedIconAsset.FindProperty("m_tintColor");
                    m_iconAssetTintColorProperty.BindProperty(tintColorProperty);
                }

                inlineIconAssetEditor.style.display = DisplayStyle.Flex;
                addIconAssetButton.style.display = DisplayStyle.None;
            }

            return root;
        }
    }
}
