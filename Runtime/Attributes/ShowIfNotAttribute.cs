using System;

namespace UrbanFox.GameObjectPainter
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfNotAttribute : BaseCompareAttribute
    {
        public ShowIfNotAttribute(string comparePropertyName, object compareValue) : base(comparePropertyName, compareValue) { }
    }
}
