using System.Linq;
using UnityEngine;

namespace Zlitz.General.ProductiveKit
{
    public sealed class ProductiveKitSettings : ScriptableObject
    {
        #region Singleton

        private static ProductiveKitSettings s_instance;

        public static ProductiveKitSettings instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Resources.LoadAll<ProductiveKitSettings>("").FirstOrDefault();
                }

                return s_instance;
            }
        }

        #endregion

        [SerializeField]
        private ColorPaletteAsset m_colors;

        public ColorPaletteAsset colors => m_colors;
    }
}
