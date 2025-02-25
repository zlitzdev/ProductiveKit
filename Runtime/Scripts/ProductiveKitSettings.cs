using System;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR

using System.IO;

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

        public static class IO
        {
            private static readonly Type s_projectSettingsType = typeof(ProductiveKitSettings);
            private static readonly string s_formattedName = FormatName(s_projectSettingsType);
            private static readonly string s_savePath = SavePath(s_projectSettingsType);

            private static ProductiveKitSettings s_loaded;

            public static ProductiveKitSettings loaded => s_loaded;

            public static ProductiveKitSettings RetrieveFromProjectSettings()
            {
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
                s_loaded = instance;
                InternalEditorUtility.SaveToSerializedFileAndForget(new UnityEngine.Object[] { instance }, s_savePath, true);
            }

            private static ProductiveKitSettings Load()
            {
                if (s_loaded != null)
                {
                    return s_loaded;
                }
                ProductiveKitSettings instance = InternalEditorUtility.LoadSerializedFileAndForget(s_savePath).OfType<ProductiveKitSettings>().FirstOrDefault();
                if (instance != null)
                {
                    s_loaded = instance;
                    instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset;
                }

                return instance;
            }

            private static ProductiveKitSettings Create()
            {
                ProductiveKitSettings newInstance = CreateInstance<ProductiveKitSettings>();
                newInstance.name = s_formattedName;
                newInstance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontUnloadUnusedAsset;

                s_loaded = newInstance;
                Save(newInstance);

                return newInstance;
            }

            private static string SavePath(Type type)
            {
                return $"ProjectSettings/{FormatName(type)}.asset";
            }

            private static string FormatName(Type type)
            {
                return type.FullName.Replace('.', '_');
            }
        
            static IO()
            {
                s_loaded = null;
                AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
            }

            private static void BeforeAssemblyReload()
            {
                if (s_loaded != null)
                {
                    DestroyImmediate(s_loaded);
                    s_loaded = null;
                }
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

        public void OnPreprocessBuild(BuildReport report)
        {
            Directory.CreateDirectory("Assets/Resources");

            AssetDatabase.DeleteAsset(s_cachedPath);

            ProductiveKitSettings instance = ProductiveKitSettings.IO.RetrieveFromProjectSettings();

            HideFlags hideFlags = instance.hideFlags;
            instance.hideFlags = HideFlags.None;

            AssetDatabase.CreateAsset(instance, s_cachedPath);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            AssetDatabase.DeleteAsset(s_cachedPath);
            AssetDatabase.Refresh();
        }

        private static string CachedPath(Type type)
        {
            return $"Assets/Resources/{type.FullName.Replace('.', '_')}.asset";
        }
    }

    #endif
}
