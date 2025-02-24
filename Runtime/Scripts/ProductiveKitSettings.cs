using System;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEditorInternal;

#endif

namespace Zlitz.General.ProductiveKit
{
    public sealed class ProductiveKitSettings : ScriptableObject
    {
        [SerializeField]
        private ColorPaletteAsset m_colors;

        public ColorPaletteAsset colors => m_colors;

        private static ProductiveKitSettings s_instance;

        public static ProductiveKitSettings instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Retrieve();
                }
                return s_instance;
            }
        }

        private static ProductiveKitSettings Retrieve()
        {
            #if UNITY_EDITOR
            return IO.RetrieveFromProjectSettings();
            #else
            return Resources.LoadAll<ProductiveKitSettings>("").FirstOrDefault();
            #endif
        }

        #if UNITY_EDITOR

        internal static class IO
        {
            private static readonly Type s_projectSettingsType = typeof(ProductiveKitSettings);
            private static readonly string s_formattedName = FormatName(s_projectSettingsType);
            private static readonly string s_savePath = SavePath(s_projectSettingsType);

            public static ProductiveKitSettings RetrieveFromProjectSettings()
            {
                if (s_instance != null)
                {
                    return s_instance;
                }

                ProductiveKitSettings instance = Load();
                if (instance == null)
                {
                    instance = Create();
                }

                return instance;
            }

            public static void Save(ProductiveKitSettings instance)
            {
                if (instance == null)
                {
                    return;
                }
                instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset;
                InternalEditorUtility.SaveToSerializedFileAndForget(new UnityEngine.Object[] { instance }, s_savePath, true);
            }

            private static ProductiveKitSettings Load()
            {
                ProductiveKitSettings instance = InternalEditorUtility.LoadSerializedFileAndForget(s_savePath).FirstOrDefault() as ProductiveKitSettings;
                if (instance != null)
                {
                    instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset;
                }

                return instance;
            }

            private static ProductiveKitSettings Create()
            {
                ProductiveKitSettings newInstance = CreateInstance<ProductiveKitSettings>();
                newInstance.name = s_formattedName;

                Save(newInstance);

                return newInstance;
            }

            static IO()
            {
                AssemblyReloadEvents.beforeAssemblyReload -= BeforeAssemblyReload;
                AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
            }

            private static void BeforeAssemblyReload()
            {
                if (s_instance != null)
                {
                    Save(s_instance);
                    DestroyImmediate(s_instance);
                    s_instance = null;
                }
            }

            private static string SavePath(Type type)
            {
                return $"ProjectSettings/{FormatName(type)}.asset";
            }

            private static string FormatName(Type type)
            {
                return type.FullName.Replace('.', '_');
            }
        }

        #endif
    }

    #if UNITY_EDITOR

    internal class ProductiveKitBuildProcess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private static readonly Type s_productiveKitSettingsType = typeof(ProductiveKitSettings);
        private static readonly string s_cachedPath = CachedPath(s_productiveKitSettingsType);

        public int callbackOrder => -200;

        public void OnPostprocessBuild(BuildReport report)
        {
            AssetDatabase.DeleteAsset(s_cachedPath);

            ProductiveKitSettings instance = ProductiveKitSettings.IO.RetrieveFromProjectSettings();
            instance.hideFlags = HideFlags.None;

            AssetDatabase.CreateAsset(instance, s_cachedPath);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            AssetDatabase.DeleteAsset(s_cachedPath);
            AssetDatabase.Refresh();
        }

        private static string CachedPath(Type type)
        {
            return $"Resources/{type.FullName.Replace('.', '_')}.asset";
        }
    }

    #endif
}
