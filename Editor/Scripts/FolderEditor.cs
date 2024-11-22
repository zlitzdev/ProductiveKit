using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Zlitz.General.ProductiveKit
{
    [CustomEditor(typeof(DefaultAsset))]
    public class FolderEditor : Editor
    {
        [SerializeField]
        private Texture2D m_defaultFolderIcon;

        private AssetImporter m_folderImporter;

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

            m_folderImporter = AssetImporter.GetAtPath(assetPath);

            string folderIconGuid;
            (folderIconGuid, m_tintColor) = GetOrDefault(assetPath, m_defaultFolderIcon);
            m_folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(folderIconGuid));

            ObjectField folderIconField = new ObjectField("Folder Icon");
            folderIconField.objectType = typeof(Texture2D);
            folderIconField.value = m_folderIcon;
            root.Add(folderIconField);

            ColorField tintColorField = new ColorField("Tint Color");
            tintColorField.showAlpha = false;
            tintColorField.value = m_tintColor;
            root.Add(tintColorField);

            folderIconField.RegisterValueChangedCallback(e =>
            {
                if (e.newValue is Texture2D folderIcon)
                {
                    m_folderIcon = folderIcon;
                    Set(assetPath, AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_folderIcon)).ToString(), m_tintColor);
                }
            });

            tintColorField.RegisterValueChangedCallback(e =>
            {
                m_tintColor = e.newValue;
                Set(assetPath, AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_folderIcon)).ToString(), m_tintColor);
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

        private (string, Color) Get(string assetPath)
        {
            string[] entries = m_folderImporter.userData.Split('|');

            foreach (string entry in entries)
            {
                FolderIconData folderIconData = JsonUtility.FromJson<FolderIconData>(entry);
                if (folderIconData != null)
                {
                    return (folderIconData.folderIconGuid, folderIconData.tintColor);
                }
            }

            return (null, Color.white);
        }

        private (string, Color) GetOrDefault(string assetPath, Texture2D defaultFolderIcon)
        {
            List<string> entries = m_folderImporter.userData.Split('|').ToList();

            foreach (string entry in entries)
            {
                FolderIconData folderIconData = JsonUtility.FromJson<FolderIconData>(entry);
                if (folderIconData != null)
                {
                    return (folderIconData.folderIconGuid, folderIconData.tintColor);
                }
            }

            string defaultFolderIconGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(defaultFolderIcon)).ToString();

            entries.Add(JsonUtility.ToJson(new FolderIconData()
            {
                folderIconGuid = defaultFolderIconGuid,
                tintColor      = Color.white
            }));

            m_folderImporter.userData = string.Join("|", entries);

            return (defaultFolderIconGuid, Color.white);
        }

        private void Set(string assetPath, string folderIconGuid, Color tintColor)
        {
            List<string> entries = m_folderImporter.userData.Split('|').ToList();

            for (int index =  0; index < entries.Count; index++)
            {
                string entry = entries[index];
                FolderIconData folderIconData = JsonUtility.FromJson<FolderIconData>(entry);
                if (folderIconData != null)
                {
                    folderIconData.folderIconGuid = folderIconGuid;
                    folderIconData.tintColor      = tintColor;
                    entry = JsonUtility.ToJson(folderIconData);
                    entries[index] = entry;

                    m_folderImporter.userData = string.Join("|", entries);
                    return;
                }
            }

            entries.Add(JsonUtility.ToJson(new FolderIconData()
            {
                folderIconGuid = folderIconGuid,
                tintColor      = tintColor
            }));

            m_folderImporter.userData = string.Join("|", entries);
        } 
    }
}
