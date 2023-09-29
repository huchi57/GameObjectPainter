using UnityEngine;
using UnityEditor;

namespace UrbanFox.GameObjectPainter.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfNotAttribute), true)]
    public class ShofIfNotDrawer : BaseCompareDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsComparedPropertyEqualsComparedValue(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !IsComparedPropertyEqualsComparedValue(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0;
        }
    }
}
