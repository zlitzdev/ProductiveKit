using UnityEngine;

namespace Zlitz.General.ProductiveKit
{
    public class FolderIconAsset : ScriptableObject
    {
        [SerializeField]
        private Texture2D m_folderIcon;

        [SerializeField, ColorUsage(showAlpha: false)]
        private Color m_tintColor = Color.white;

        internal Texture2D folderIcon => m_folderIcon;

        internal Color tintColor => new Color(m_tintColor.r, m_tintColor.g, m_tintColor.b, 1.0f);
    }
}
