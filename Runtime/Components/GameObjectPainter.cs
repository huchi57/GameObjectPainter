using System;
using System.Collections.Generic;
using UnityEngine;

// Do not warn about unused fields - they are actually referenced in the editor script
#pragma warning disable CS0414
#pragma warning disable IDE0052
namespace UrbanFox.GameObjectPainter
{
    [AddComponentMenu("OwO/GameObject Painter")]
    public class GameObjectPainter : MonoBehaviour
    {
        [Serializable]
        public enum BrushShape
        {
            Sphere,
            Cylinder,
            Box
        }

        [Serializable]
        public class GameObjectBrush
        {
            [SerializeField] private string m_brushName = "New Brush";
            [SerializeField] private float m_minScale = 1;
            [SerializeField] private float m_maxScale = 1;
            [SerializeField] private List<GameObject> m_brushElements = new List<GameObject>();

            public List<GameObject> BrushElements => m_brushElements;

            public GameObjectBrush(string brushName, float minScale, float maxScale)
            {
                m_brushName = brushName;
                m_minScale = minScale;
                m_maxScale = maxScale;
                m_brushElements = new List<GameObject>();
            }

            public void Validate()
            {
                m_minScale = Mathf.Max(m_minScale, 0);
                m_maxScale = m_maxScale < m_minScale ? m_minScale : m_maxScale;
            }
        }

        [SerializeField]
        private LayerMask m_layerMask = 1;

        [SerializeField]
        private bool m_reverseScrollWheelControl = true;

        [Header("Brush Settings"), SerializeField]
        private BrushShape m_brushShape = BrushShape.Cylinder;

        [SerializeField, Min(0)]
        private float m_brushRadius = 5;

        [SerializeField, Min(0)]
        private int m_brushDensity = 10;

        [SerializeField, ShowIfNot(nameof(m_brushShape), BrushShape.Sphere), Min(0)]
        private float m_brushHeight = 1;

        [Header("Angle Settings"), SerializeField, Range(0f, 360f)]
        private float m_maxRandomRotateAngle = 360;

        [SerializeField, Range(0f, 360f)]
        private float m_maxSlopeAngle = 360;

        [SerializeField, Range(0f, 360f)]
        private float m_offsetRotateAngle = 0;

        [SerializeField, HideInInspector]
        private List<GameObjectBrush> m_brushes = new List<GameObjectBrush>();

        [SerializeField, HideInInspector]
        private int m_currentSelectedIndex = 0;

        public BrushShape CurrentBrushShape => m_brushShape;
        
        public void DeleteAllChildren()
        {
            if (transform == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                while (transform.childCount > 0)
                {
                    Destroy(transform.GetChild(0).gameObject);
                }
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(transform.gameObject, "Delete All Children");
                var undoStepGroup = UnityEditor.Undo.GetCurrentGroup();
                while (transform.childCount > 0)
                {
                    UnityEditor.Undo.DestroyObjectImmediate(transform.GetChild(0).gameObject);
                }
                UnityEditor.Undo.CollapseUndoOperations(undoStepGroup);
#else
                while (transform.childCount > 0)
                {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                }
#endif
            }
        }

        public GameObjectBrush GetBrushByIndex(int index)
        {
            return index.IsInRange(m_brushes) ? m_brushes[index] : null;
        }

        private void OnValidate()
        {
            m_currentSelectedIndex = m_currentSelectedIndex.IsInRange(m_brushes) ? m_currentSelectedIndex : 0;
            if (!m_brushes.IsNullOrEmpty())
            {
                foreach (var brush in m_brushes)
                {
                    brush?.Validate();
                }
            }
        }

        private void Reset()
        {
            m_brushes = new List<GameObjectBrush>();
            m_brushes.Add(new GameObjectBrush("New Brush", 1, 1));
        }
    }
}
#pragma warning restore IDE0052
#pragma warning restore CS0414
