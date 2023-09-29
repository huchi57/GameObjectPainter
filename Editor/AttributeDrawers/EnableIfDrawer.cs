using UnityEngine;
using UnityEditor;

namespace UrbanFox.GameObjectPainter.Editor
{
    [CustomPropertyDrawer(typeof(EnableIfAttribute), true)]
    public class EnableIfDrawer : BaseCompareDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var cacheGUIState = GUI.enabled;
            GUI.enabled = IsComparedPropertyEqualsComparedValue(property);
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = cacheGUIState;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
