using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace UrbanFox.GameObjectPainter.Editor
{
    public class HandlesDrawScope : IDisposable
    {
        private Matrix4x4 m_cachedMatrix;
        private Color m_cachedHandlesColor;
        private CompareFunction m_cachedZTest;

        public HandlesDrawScope(Vector3 position, Quaternion rotation, Vector3 scale, Color color, CompareFunction zTest)
        {
            Initialize(Matrix4x4.TRS(position, rotation, scale), color, zTest);
        }

        public HandlesDrawScope(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Initialize(Matrix4x4.TRS(position, rotation, scale), Handles.color, Handles.zTest);
        }

        public HandlesDrawScope(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
        {
            Initialize(Matrix4x4.TRS(position, rotation, scale), color, Handles.zTest);
        }

        public HandlesDrawScope(Color color)
        {
            Initialize(Handles.matrix, color, Handles.zTest);
        }

        public HandlesDrawScope(Color color, CompareFunction zTest)
        {
            Initialize(Handles.matrix, color, zTest);
        }

        public void Dispose()
        {
            Handles.matrix = m_cachedMatrix;
            Handles.color = m_cachedHandlesColor;
            Handles.zTest = m_cachedZTest;
        }

        private void Initialize(Matrix4x4 matrix, Color color, CompareFunction zTest)
        {
            m_cachedMatrix = Handles.matrix;
            m_cachedHandlesColor = Handles.color;
            m_cachedZTest = Handles.zTest;
            Handles.matrix = matrix;
            Handles.color = color;
            Handles.zTest = zTest;
        }
    }
}
