using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace UrbanFox.GameObjectPainter.Editor
{
    public static class HandlesExtensions
    {
        public static void DrawColoredWireCube_ZTest(Vector3 center, Quaternion rotation, Vector3 scale, Color mainColor, Color coveredColor)
        {
            using (new HandlesDrawScope(center, rotation, scale, mainColor, CompareFunction.Less))
            {
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
            using (new HandlesDrawScope(center, rotation, scale, coveredColor, CompareFunction.GreaterEqual))
            {
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }

        public static void DrawColoredWireCube(Vector3 center, Quaternion rotation, Vector3 scale, Color color)
        {
            DrawColoredWireCube_ZTest(center, rotation, scale, color, color);
        }

        public static void DrawColoredWireDisc_ZTest(Vector3 center, Vector3 normal , float radius, Color mainColor, Color coveredColor)
        {
            using (new HandlesDrawScope(mainColor, CompareFunction.Less))
            {
                Handles.DrawWireDisc(center, normal, radius);
            }
            using (new HandlesDrawScope(coveredColor, CompareFunction.GreaterEqual))
            {
                Handles.DrawWireDisc(center, normal, radius);
            }
        }

        public static void DrawColoredWireDisc(Vector3 center, Vector3 normal, float radius, Color color)
        {
            DrawColoredWireDisc_ZTest(center, normal, radius, color, color);
        }

        public static void DrawColoredSolidDisc_ZTest(Vector3 center, Vector3 normal, float radius, Color mainColor, Color coveredColor)
        {
            using (new HandlesDrawScope(mainColor, CompareFunction.Less))
            {
                Handles.DrawSolidDisc(center, normal, radius);
            }
            using (new HandlesDrawScope(coveredColor, CompareFunction.GreaterEqual))
            {
                Handles.DrawSolidDisc(center, normal, radius);
            }
        }

        public static void DrawColoredSolidDisc(Vector3 center, Vector3 normal, float radius, Color color)
        {
            DrawColoredSolidDisc_ZTest(center, normal, radius, color, color);
        }
    }
}
