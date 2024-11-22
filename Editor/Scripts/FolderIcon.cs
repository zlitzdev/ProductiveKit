using System.IO;

using UnityEngine;
using UnityEditor;

namespace Zlitz.General.ProductiveKit
{
    [InitializeOnLoad]
    internal static class FolderIcon
    {
        private static readonly string s_folderIconMetadataFile = "FolderIcon.json~";

        private static Texture2D s_defaultFolderIcon;

        public static Texture2D defaultFolderIcon
        {
            get
            {
                if (s_defaultFolderIcon == null)
                {
                    s_defaultFolderIcon = Resources.Load<Texture2D>("Icon_Folder_Default");
                }

                return s_defaultFolderIcon;
            }
        }

        public static (string, Color) GetOrDefault(string assetPath)
        {
            string folderIconMetadataPath = Path.Combine(assetPath, s_folderIconMetadataFile);
            if (ReadFile(folderIconMetadataPath, out string folderIconMetadata))
            {
                FolderIconData folderIconData = JsonUtility.FromJson<FolderIconData>(folderIconMetadata);
                if (folderIconData != null)
                {
                    return (folderIconData.folderIconGuid, folderIconData.tintColor);
                }
            }

            string defaultFolderIconGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(defaultFolderIcon)).ToString();
            return (defaultFolderIconGuid, Color.white);
        }

        public static void Set(string assetPath, string folderIconGuid, Color tintColor)
        {
            string folderIconMetadataPath = Path.Combine(assetPath, s_folderIconMetadataFile);
            string folderIconMetadata = JsonUtility.ToJson(new FolderIconData() 
            {
                folderIconGuid = folderIconGuid,
                tintColor = tintColor
            });

            if (!WriteFile(folderIconMetadataPath, folderIconMetadata))
            {
                Debug.LogWarning($"Folder icon data is not saved at {assetPath}");
            }
        }

        static FolderIcon()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(assetPath) && GetFolderIcon(assetPath, out Texture2D folderIcon, out Color tintColor))
            {
                DrawFolderIcon(selectionRect, folderIcon, tintColor);
            }
        }
    
        private static bool ReadFile(string path, out string content)
        {
            if (File.Exists(path))
            {
                try
                {
                    string fileContent = File.ReadAllText(path);
                    content = fileContent;
                    return true;
                }
                catch (System.Exception)
                {
                    content = null;
                    return false;
                }
            }

            content = null;
            return false;
        }

        private static bool WriteFile(string path, string content)
        {
            try
            {
                File.Delete(path);
                File.WriteAllText(path, content);
                File.SetAttributes(path, FileAttributes.Hidden);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool GetFolderIcon(string path, out Texture2D folderIcon, out Color tintColor)
        {
            folderIcon = null;
            tintColor  = Color.white;

            string folderIconGuid;
            (folderIconGuid, tintColor) = GetOrDefault(path);

            folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(folderIconGuid));

            return folderIcon != null;
        }

        private static void DrawFolderIcon(Rect rect, Texture2D folderIcon, Color tintColor)
        {
            bool isTreeView = rect.width > rect.height;

            #if UNITY_2019_3_OR_NEWER
            bool isSideView = rect.x != 14;
            #else
            bool isSideView = rect.x != 13;
            #endif


            if (isTreeView)
            {
                rect.width = 16.0f;
                rect.width = 16.0f;

                if (!isSideView)
                {
                    rect.x += 3.0f;
                }
            }
            else
            {
                rect.height -= 14.0f;
            }

            if (folderIcon != null)
            {
                Color guiColor = GUI.color;
                GUI.color = new Color(guiColor.r * tintColor.r, guiColor.g * tintColor.g, guiColor.b * tintColor.b, guiColor.a);
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.height, rect.height), folderIcon);
                GUI.color = guiColor;
            }
        }
    }
}
