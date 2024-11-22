using System.IO;
using System.Linq;

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
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                string[] folderIconAssetGuids = AssetDatabase.FindAssets("t:FolderIconAsset", new string[] { assetPath });
                folderIconAssetGuids = folderIconAssetGuids.Where(id => Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(id)).Replace("\\", "/") == assetPath).ToArray();
            
                if (folderIconAssetGuids.Length > 0)
                {
                    string folderIconAssetPath = AssetDatabase.GUIDToAssetPath(folderIconAssetGuids[0]);

                    if (folderIconAssetGuids.Length > 1)
                    {
                        Debug.LogWarning($"Folder {assetPath} contains multiple FolderIconAsset. Using {folderIconAssetPath} and ignored the rest.");
                    }

                    FolderIconAsset folderIconAsset = AssetDatabase.LoadAssetAtPath<FolderIconAsset>(folderIconAssetPath);

                    if (folderIconAsset != null)
                    {
                        DrawFolderIcon(selectionRect, folderIconAsset);
                    }
                }
            }
        }
    
        private static void DrawFolderIcon(Rect rect, FolderIconAsset folderIconAsset)
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

            Texture2D iconTexture = folderIconAsset.folderIcon;
            if (iconTexture != null)
            {
                GUI.color = folderIconAsset.tintColor;
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.height, rect.height), iconTexture);
                GUI.color = Color.white;
            }
        }
    }
}
