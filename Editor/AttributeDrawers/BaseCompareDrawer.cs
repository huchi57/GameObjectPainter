using System.IO;
using UnityEditor;

namespace UrbanFox.GameObjectPainter.Editor
{
    [CustomPropertyDrawer(typeof(BaseCompareAttribute), true)]
    public class BaseCompareDrawer : PropertyDrawer
    {
        protected BaseCompareAttribute Attribute => (BaseCompareAttribute)attribute;

        protected bool IsComparedPropertyEqualsComparedValue(SerializedProperty property)
        { 
            var targetPropertyPath = property.propertyPath.Contains(".") ?
                Path.ChangeExtension(property.propertyPath, Attribute.ComparedPropertyName) :
                Attribute.ComparedPropertyName;

            var targetField = property.serializedObject.FindProperty(targetPropertyPath);

            if (targetField != null)
            {
                switch (targetField.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        return targetField.intValue.Equals(Attribute.ComparedValue);
                    case SerializedPropertyType.Boolean:
                        return targetField.boolValue.Equals(Attribute.ComparedValue);
                    case SerializedPropertyType.Enum:
                        return targetField.enumValueIndex.Equals((int)Attribute.ComparedValue);
                }
            }
            
            // Always return true if the property cannot be found
            return true;
        }
    }
}
