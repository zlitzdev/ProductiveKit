using System;

#if !UNITY_EDITOR
using System.Collections.Generic;
#endif

using UnityEngine;

namespace Zlitz.General.ProductiveKit
{
    [CreateAssetMenu(menuName = "Zlitz/General/Productive Kit/Color Palette")]
    public sealed class ColorPaletteAsset : ScriptableObject
    {
        [SerializeField]
        private Entry[] m_entries;

        #if !UNITY_EDITOR

        private Dictionary<string, Color> m_cachedColors;

        #endif

        public void ClearCache()
        {
            #if !UNITY_EDITOR

            m_cachedColors?.Clear();

            #endif
        }

        public Color Get(string name)
        {
            #if !UNITY_EDITOR
            
            if (m_cachedColors != null && m_cachedColors.TryGetValue(name, out Color cachedColor))
            {
                return cachedColor;
            }

            #endif

            if (m_entries != null)
            {
                foreach (Entry entry in m_entries)
                {
                    if (entry.name == name)
                    {
                        #if !UNITY_EDITOR
            
                        if (m_cachedColors == null)
                        {
                            m_cachedColors = new Dictionary<string, Color>();
                        }

                        m_cachedColors.TryAdd(entry.name, entry.color);

                        #endif

                        return entry.color;
                    }
                }
            }
            return Color.black;
        }

        [Serializable]
        private struct Entry
        {
            [SerializeField]
            private string m_name;

            [SerializeField]
            private Color m_color;

            public string name => m_name;

            public Color color => m_color;

#if UNITY_EDITOR

            [SerializeField]
            private bool m_showAlpha;

            [SerializeField]
            private bool m_hdr;

#endif
        }
    }
}
