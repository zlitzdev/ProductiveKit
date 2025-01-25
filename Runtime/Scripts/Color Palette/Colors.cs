using UnityEngine;

namespace Zlitz.General.ProductiveKit
{
    public static class Colors
    {
        public static bool HasColorPalette() => ProductiveKitSettings.instance.colors != null;

        public static Color GetColor(string name) => ProductiveKitSettings.instance.colors?.Get(name) ?? Color.black;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            ProductiveKitSettings.instance?.colors?.ClearCache();
        }
    }
}
