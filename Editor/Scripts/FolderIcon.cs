using UnityEngine;
using UnityEditor;

namespace Zlitz.General.ProductiveKit
{
    [InitializeOnLoad]
    internal static class FolderIcon
    {
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
    
        private static bool GetFolderIcon(string path, out Texture2D folderIcon, out Color tintColor)
        {
            folderIcon = null;
            tintColor  = Color.white;

            AssetImporter folderImporter = AssetImporter.GetAtPath(path);
            string[] entries = folderImporter.userData.Split('|');

            foreach (string entry in entries)
            {
                FolderIconData folderIconData = JsonUtility.FromJson<FolderIconData>(entry);
                if (folderIconData != null)
                {
                    folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(folderIconData.folderIconGuid));
                    tintColor = folderIconData.tintColor;
                    break;
                }
            }

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
