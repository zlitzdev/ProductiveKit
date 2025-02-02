using System.Linq;
using UnityEngine;

namespace Zlitz.General.ProductiveKit
{
    [ExecuteAlways]
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
                
                if (s_instance == null)
                {
                    Debug.LogError("Cannot find Productive Kit Settings");
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
