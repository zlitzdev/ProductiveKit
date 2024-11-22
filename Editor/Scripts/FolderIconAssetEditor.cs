using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Zlitz.General.ProductiveKit
{
    [CustomEditor(typeof(FolderIconAsset))]
    public class FolderIconAssetEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            SerializedProperty folderIconProperty = serializedObject.FindProperty("m_folderIcon");
            SerializedProperty tintColorProperty  = serializedObject.FindProperty("m_tintColor");

            VisualElement root = new VisualElement();

            PropertyField folderIconPropertyField = new PropertyField();
            folderIconPropertyField.style.flexGrow = 1.0f;
            folderIconPropertyField.BindProperty(folderIconProperty);
            root.Add(folderIconPropertyField);

            PropertyField tintColorPropertyField = new PropertyField();
            tintColorPropertyField.style.flexGrow = 1.0f;
            tintColorPropertyField.BindProperty(tintColorProperty);
            root.Add(tintColorPropertyField);

            return root;
        }
    }
}
