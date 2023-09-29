using System;

namespace UrbanFox.GameObjectPainter
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class EnableIfAttribute : BaseCompareAttribute
    {
        public EnableIfAttribute(string comparePropertyName, object compareValue) : base(comparePropertyName, compareValue) { }
    }
}
