using System;
using UnityEngine;

namespace UrbanFox.GameObjectPainter
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public abstract class BaseCompareAttribute : PropertyAttribute
    {
        public string ComparedPropertyName { get; private set; }
        public object ComparedValue { get; private set; }

        public BaseCompareAttribute(string comparePropertyName, object compareValue)
        { 
            ComparedPropertyName = comparePropertyName;
            ComparedValue = compareValue;
        }
    }
}
