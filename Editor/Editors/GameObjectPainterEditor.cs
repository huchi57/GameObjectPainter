using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UrbanFox.GameObjectPainter.Editor
{
    [CustomEditor(typeof(GameObjectPainter))]
    public class GameObjectPainterEditor : UnityEditor.Editor
    {
        // If enabled, preview objects will be visible in the hierarchy.
        private const bool m_isDryTest = false;

        private struct BrushElementGenerationData
        {
            public GameObject PrefabReference;
            public LayerMask OriginalLayer;
            public Vector2 LocalPositionFromTopSurfaceCenter;
            public float LocalYRotationAngle;
            public float LocalScale;
        }

        private const int m_brushButtonSize = 100;
        private GameObjectPainter m_gameObjectPainter;

        private SerializedProperty m_layerMask;
        private SerializedProperty m_reverseScrollWheelControl;
        private SerializedProperty m_brushShape;
        private SerializedProperty m_brushRadius;
        private SerializedProperty m_brushDensity;
        private SerializedProperty m_brushHeight;
        private SerializedProperty m_maxRandomRotateAngle;
        private SerializedProperty m_maxSlopeAngle;
        private SerializedProperty m_offsetRotateAngle;
        private SerializedProperty m_brushes;
        private SerializedProperty m_currentSelectedIndex;

        private GameObject m_brushPreviewContainer;

        private Vector2 m_brushBrowserScroll;
        private Vector3 m_mouseRayHitPoint;
        private Vector3 m_mouseRayHitNormal;
        private Dictionary<GameObject, BrushElementGenerationData> m_brushGenerationData;
        private List<GameObject> m_gameObjectsToBeDeleted;

        private SerializedProperty CurrentBrush => m_currentSelectedIndex.intValue < m_brushes.arraySize ? m_brushes.GetArrayElementAtIndex(m_currentSelectedIndex.intValue) : null;
        private SerializedProperty CurrentBrushName => CurrentBrush == null ? null : CurrentBrush.FindPropertyRelative("m_brushName");
        private SerializedProperty CurrentBrushMinScale => CurrentBrush == null ? null : CurrentBrush.FindPropertyRelative("m_minScale");
        private SerializedProperty CurrentBrushMaxScale => CurrentBrush == null ? null : CurrentBrush.FindPropertyRelative("m_maxScale");
        private SerializedProperty CurrentBrushElements=> CurrentBrush == null ? null : CurrentBrush.FindPropertyRelative("m_brushElements");

        private bool IsHoldingControl => Event.current.control;
        private bool IsHoldingShift => Event.current.shift;
        private bool IsHoldingAlt => Event.current.alt;

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        [MenuItem("GameObject/OwO/GameObject Painter")]
        private static void AddGameObjectPainter(MenuCommand menuCommand)
        {
            var newPainter = new GameObject("GameObject Painter");
            newPainter.AddComponent<GameObjectPainter>();
            GameObjectUtility.SetParentAndAlign(newPainter, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(newPainter, "Create GameObject Painter");
            Selection.activeObject = newPainter;
        }

        private void OnEnable()
        {
            m_gameObjectPainter = (GameObjectPainter)target;
            m_layerMask = serializedObject.FindProperty(nameof(m_layerMask));
            m_reverseScrollWheelControl = serializedObject.FindProperty(nameof(m_reverseScrollWheelControl));
            m_brushShape = serializedObject.FindProperty(nameof(m_brushShape));
            m_brushRadius = serializedObject.FindProperty(nameof(m_brushRadius));
            m_brushDensity = serializedObject.FindProperty(nameof(m_brushDensity));
            m_brushHeight = serializedObject.FindProperty(nameof(m_brushHeight));
            m_maxRandomRotateAngle = serializedObject.FindProperty(nameof(m_maxRandomRotateAngle));
            m_maxSlopeAngle = serializedObject.FindProperty(nameof(m_maxSlopeAngle));
            m_offsetRotateAngle = serializedObject.FindProperty(nameof(m_offsetRotateAngle));
            m_brushes = serializedObject.FindProperty(nameof(m_brushes));
            m_currentSelectedIndex = serializedObject.FindProperty(nameof(m_currentSelectedIndex));

            if (m_brushPreviewContainer)
            {
                DestroyImmediate(m_brushPreviewContainer);
            }
            m_brushPreviewContainer = new GameObject(nameof(m_brushPreviewContainer));
            m_brushPreviewContainer.hideFlags = m_isDryTest ? HideFlags.None : HideFlags.HideAndDontSave;
            CreateNewBrushPreview();
        }

        private void OnDisable()
        {
            if (m_brushPreviewContainer)
            {
                DestroyImmediate(m_brushPreviewContainer);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // Draw base Inspector
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayoutExtensions.ColoredButton("Delete All Children", Color.red))
            {
                m_gameObjectPainter.DeleteAllChildren();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            GUILayout.Label("Controls", EditorStyles.boldLabel);
            GUILayoutExtensions.HorizontalLine();
            EditorGUILayout.HelpBox("Left Click: Paint\nHold Alt + Left Click: Erase\nHold Control + Mouse Scroll: Brush Radius\nHold Shift + Mouse Scroll: Brush Density\nHold Alt + Mouse Scroll: Brush Height\n\"[\" and \"]\": Offset Rotate Angle", MessageType.Info);

            EditorGUILayout.Space();

            GUILayout.Label("Brushes", EditorStyles.boldLabel);
            GUILayoutExtensions.HorizontalLine();

            if (m_currentSelectedIndex.intValue < m_brushes.arraySize)
            {
                var previewTextures = new List<Texture2D>();
                for (int i = 0; i < m_brushes.arraySize; i++)
                {
                    var brush = m_gameObjectPainter.GetBrushByIndex(i);
                    if (brush != null && !brush.BrushElements.IsNullOrEmpty())
                    {
                        previewTextures.Add(AssetPreview.GetAssetPreview(brush.BrushElements[0]));
                    }
                    else
                    {
                        previewTextures.Add(EditorGUIUtility.whiteTexture);
                    }
                }

                using (var checkSelectedBrush = new EditorGUI.ChangeCheckScope())
                {
                    // At least 1 item should be rendered
                    var xCountOnGrid = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth - 20) / m_brushButtonSize);
                    var gridAreaHeight = Mathf.CeilToInt((float)m_brushes.arraySize / xCountOnGrid) * m_brushButtonSize;
                    m_currentSelectedIndex.intValue = GUILayout.SelectionGrid(m_currentSelectedIndex.intValue, previewTextures.ToArray(), xCountOnGrid, GUILayout.Height(gridAreaHeight));
                    if (checkSelectedBrush.changed)
                    {
                        GUI.FocusControl(null);
                        CreateNewBrushPreview();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"At least one prefab is needed to start painting.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            if (GUILayoutExtensions.ColoredButton("Add a New Brush", Color.green))
            {
                m_brushes.arraySize++;
            }

            EditorGUILayout.Space();

            if (m_currentSelectedIndex.intValue < m_brushes.arraySize)
            {
                var previewBrushSettings = m_brushes.GetArrayElementAtIndex(m_currentSelectedIndex.intValue);
                var brushName = previewBrushSettings.FindPropertyRelative("m_brushName");

                GUILayout.Label($"Brush Settings: {CurrentBrushName.stringValue}", EditorStyles.boldLabel);
                GUILayoutExtensions.HorizontalLine();
                EditorGUILayout.PropertyField(CurrentBrushName);
                EditorGUILayout.PropertyField(CurrentBrushMinScale);
                EditorGUILayout.PropertyField(CurrentBrushMaxScale);
                EditorGUILayout.PropertyField(CurrentBrushElements);

                EditorGUILayout.HelpBox("Brushes are consisted with random combinations of brush elements based on Min Scale and Max Scale settings.", MessageType.Info);

                if (GUILayoutExtensions.ColoredButton($"Delete This Brush: {brushName.stringValue}", Color.red))
                {
                    if (m_brushes.arraySize > 1)
                    {
                        m_brushes.MoveArrayElement(m_currentSelectedIndex.intValue, m_brushes.arraySize - 1);
                        m_brushes.arraySize--;
                    }
                    else
                    {
                        m_brushes.arraySize = 0;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                CreateNewBrushPreview();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!m_brushPreviewContainer)
            {
                return;
            }

            SceneView.RepaintAll();

            var mousePosition = Event.current.mousePosition;
            var mouseRay = HandleUtility.GUIPointToWorldRay(mousePosition);

            var defaultColor = IsHoldingAlt ? Color.red : Color.white;
            var occludeColor = defaultColor.SetAlpha(0.25f);

            if (Physics.Raycast(mouseRay, out var hit, float.MaxValue, m_layerMask.intValue))
            {
                m_mouseRayHitPoint = hit.point;
                m_mouseRayHitNormal = hit.normal.normalized;

                switch (m_gameObjectPainter.CurrentBrushShape)
                {
                    case GameObjectPainter.BrushShape.Sphere:
                        HandlesExtensions.DrawColoredWireDisc_ZTest(m_mouseRayHitPoint, Vector3.up, m_brushRadius.floatValue, defaultColor, occludeColor);
                        HandlesExtensions.DrawColoredWireDisc_ZTest(m_mouseRayHitPoint, Vector3.right, m_brushRadius.floatValue, defaultColor, occludeColor);
                        HandlesExtensions.DrawColoredWireDisc_ZTest(m_mouseRayHitPoint, Vector3.forward, m_brushRadius.floatValue, defaultColor, occludeColor);
                        break;
                    case GameObjectPainter.BrushShape.Cylinder:
                        HandlesExtensions.DrawColoredWireDisc_ZTest(m_mouseRayHitPoint, m_mouseRayHitNormal, m_brushRadius.floatValue, defaultColor, occludeColor);
                        HandlesExtensions.DrawColoredWireDisc_ZTest(m_mouseRayHitPoint + m_brushHeight.floatValue * m_mouseRayHitNormal, m_mouseRayHitNormal, m_brushRadius.floatValue, defaultColor, occludeColor);
                        HandlesExtensions.DrawColoredWireDisc_ZTest(m_mouseRayHitPoint - m_brushHeight.floatValue * m_mouseRayHitNormal, m_mouseRayHitNormal, m_brushRadius.floatValue, defaultColor, occludeColor);
                        break;
                    case GameObjectPainter.BrushShape.Box:
                        var boxRotation = Quaternion.LookRotation(m_mouseRayHitNormal.GetPerpendicularVector().RotateVectorAlongAxis(m_mouseRayHitNormal, m_offsetRotateAngle.floatValue), m_mouseRayHitNormal);
                        var boxSize = new Vector3(m_brushRadius.floatValue, 2 * m_brushHeight.floatValue, m_brushRadius.floatValue);
                        HandlesExtensions.DrawColoredWireCube_ZTest(m_mouseRayHitPoint, boxRotation, boxSize, defaultColor, occludeColor);
                        break;
                    default:
                        break;
                }

                if (IsHoldingAlt)
                {
                    CheckForGameObjectsToBeDeletedInsideBrushVolume();
                }

                m_brushPreviewContainer.SetActive(true);
                UpdateBrushElementInstances();
            }
            else
            {
                m_brushPreviewContainer.SetActive(false);
            }

            #region Inputs
            HandleEventInputs();

            void HandleEventInputs()
            {
                var cacheControlID = GUIUtility.GetControlID(FocusType.Passive);
                var delta = (Event.current.delta.x + Event.current.delta.y) / 30 * (m_reverseScrollWheelControl.boolValue ? -1 : 1);

                switch (Event.current.type)
                {
                    case EventType.KeyDown:
                        HandleKeyCodes(Event.current.keyCode);
                        break;

                    case EventType.ScrollWheel:

                        // Holding control: Modify brush radius
                        if (IsHoldingControl)
                        {
                            serializedObject.Update();
                            m_brushRadius.floatValue *= delta > 0 ? 1.1f : 0.9f;
                            m_brushRadius.floatValue = Mathf.Max(m_brushRadius.floatValue, 0);
                            CreateNewBrushPreview();
                            serializedObject.ApplyModifiedProperties();
                            Event.current.Use();
                        }

                        // Holding shift: Modify brush density
                        else if (IsHoldingShift)
                        {
                            serializedObject.Update();
                            m_brushDensity.intValue += delta > 0 ? 1 : -1;
                            m_brushDensity.intValue = Mathf.Max(m_brushDensity.intValue, 0);
                            CreateNewBrushPreview();
                            serializedObject.ApplyModifiedProperties();
                            Event.current.Use();
                        }

                        // Holding alt: Modify brush height
                        else if (IsHoldingAlt)
                        {
                            serializedObject.Update();
                            m_brushHeight.floatValue *= delta > 0 ? 1.1f : 0.9f;
                            m_brushHeight.floatValue = Mathf.Max(m_brushHeight.floatValue, 0);
                            CreateNewBrushPreview();
                            serializedObject.ApplyModifiedProperties();
                            Event.current.Use();
                        }

                        break;

                    case EventType.MouseDown:

                        // 0: Left click
                        if (Event.current.button == 0)
                        {
                            if (IsHoldingAlt)
                            {
                                ExecuteErase();
                            }
                            else
                            {
                                ExecutePaint();
                            }
                            GUIUtility.hotControl = cacheControlID;
                            Event.current.Use();
                        }
                        break;
                }
            }

            void HandleKeyCodes(KeyCode keyCode)
            {
                switch (keyCode)
                {
                    case KeyCode.LeftBracket:
                        serializedObject.Update();
                        m_offsetRotateAngle.floatValue -= 10;
                        m_offsetRotateAngle.floatValue = m_offsetRotateAngle.floatValue.Angle360();
                        serializedObject.ApplyModifiedProperties();
                        break;
                    case KeyCode.RightBracket:
                        serializedObject.Update();
                        m_offsetRotateAngle.floatValue += 10;
                        m_offsetRotateAngle.floatValue = m_offsetRotateAngle.floatValue.Angle360();
                        serializedObject.ApplyModifiedProperties();
                        break;

                    // Do not use current event for invalid keycodes
                    default:
                        return;
                }
                Event.current.Use();
            }
            #endregion
        }

        #region Paint and Erase Functions
        private void ExecutePaint()
        {
            if (m_brushGenerationData.IsNullOrEmpty())
            {
                return;
            }

            Undo.RecordObject(this, "Paint Objects");
            var undoStepGroup = Undo.GetCurrentGroup();
            foreach (var data in m_brushGenerationData)
            {
                var instancePreview = data.Key;
                var instanceData = data.Value;
                if (instancePreview.activeSelf)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(instanceData.PrefabReference, m_gameObjectPainter.transform);
                    instance.transform.SetPositionAndRotation(instancePreview.transform.position, instancePreview.transform.rotation);
                    instance.transform.localScale = instancePreview.transform.localScale;
                    instance.layer = instanceData.OriginalLayer;
                    instance.hideFlags = HideFlags.None;
                    Undo.RegisterCreatedObjectUndo(instance, null);
                }
                DestroyImmediate(instancePreview);
            }
            Undo.CollapseUndoOperations(undoStepGroup);
            CreateNewBrushPreview();
        }

        private void ExecuteErase()
        {
            if (m_gameObjectsToBeDeleted.IsNullOrEmpty())
            {
                return;
            }

            Undo.RecordObject(this, "Erase Objects");
            var undoStepGroup = Undo.GetCurrentGroup();
            foreach (var gameObject in m_gameObjectsToBeDeleted)
            {
                if (gameObject)
                {
                    Undo.DestroyObjectImmediate(gameObject);
                }
            }
            Undo.CollapseUndoOperations(undoStepGroup);
        }
        #endregion

        #region Brush Manipulation Functions
        private void CreateNewBrushPreview()
        {
            if (!m_brushPreviewContainer)
            {
                return;
            }

            while (m_brushPreviewContainer.transform.childCount > 0)
            {
                DestroyImmediate(m_brushPreviewContainer.transform.GetChild(0).gameObject);
            }

            if (CurrentBrushElements == null)
            {
                return;
            }

            m_brushGenerationData = new Dictionary<GameObject, BrushElementGenerationData>();
            for (int i = 0; i < m_brushDensity.intValue; i++)
            {
                if (CurrentBrushElements.arraySize > 0)
                {
                    var selectedBrushElement = (GameObject)CurrentBrushElements.GetArrayElementAtIndex(Random.Range(0, CurrentBrushElements.arraySize)).objectReferenceValue;
                    if (selectedBrushElement)
                    {
                        var brushElementInstance = (GameObject)PrefabUtility.InstantiatePrefab(selectedBrushElement, m_brushPreviewContainer.transform);
                        m_brushGenerationData.Add(brushElementInstance, new BrushElementGenerationData
                        {
                            PrefabReference = selectedBrushElement,
                            OriginalLayer = brushElementInstance.layer,
                            LocalPositionFromTopSurfaceCenter = GenerateRandomLocalPosition(),
                            LocalYRotationAngle = Random.Range(0, m_maxRandomRotateAngle.floatValue),
                            LocalScale = Random.Range(CurrentBrushMinScale.floatValue, CurrentBrushMaxScale.floatValue)
                        });
                        brushElementInstance.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }
                }
            }

            Vector3 GenerateRandomLocalPosition()
            {
                return m_gameObjectPainter.CurrentBrushShape switch
                {
                    GameObjectPainter.BrushShape.Sphere => m_brushRadius.floatValue * Random.insideUnitCircle,
                    GameObjectPainter.BrushShape.Cylinder => m_brushRadius.floatValue * Random.insideUnitCircle,
                    GameObjectPainter.BrushShape.Box => new Vector2(Random.Range(-m_brushRadius.floatValue, m_brushRadius.floatValue), Random.Range(-m_brushRadius.floatValue, m_brushRadius.floatValue)) / 2,
                    _ => m_brushRadius.floatValue * Random.insideUnitCircle
                };
            }
        }

        private void UpdateBrushElementInstances()
        {
            if (m_brushGenerationData.IsNullOrEmpty())
            {
                return;
            }

            var forwardOffsetDirection = m_mouseRayHitNormal.GetPerpendicularVector().RotateVectorAlongAxis(m_mouseRayHitNormal, m_offsetRotateAngle.floatValue);
            var rightOffsetDirection = forwardOffsetDirection.RotateVectorAlongAxis(m_mouseRayHitNormal, 90);

            foreach (var data in m_brushGenerationData)
            {
                var instance = data.Key;
                var instanceData = data.Value;
                var instanceRayOrigin = m_mouseRayHitPoint + m_brushHeight.floatValue * m_mouseRayHitNormal;
                instanceRayOrigin += instanceData.LocalPositionFromTopSurfaceCenter.x * rightOffsetDirection + instanceData.LocalPositionFromTopSurfaceCenter.y * forwardOffsetDirection;

                if (instance)
                {
                    if (!IsHoldingAlt
                        && Physics.Raycast(instanceRayOrigin, -m_mouseRayHitNormal, out var hit, 2 * m_brushHeight.floatValue, m_layerMask.intValue)
                        && Vector3.Angle(Vector3.up, hit.normal) < m_maxSlopeAngle.floatValue)
                    {
                        // Remove out-of-sphere instances for sphere brushes
                        if (m_gameObjectPainter.CurrentBrushShape == GameObjectPainter.BrushShape.Sphere && Vector3.Distance(hit.point, m_mouseRayHitPoint) > m_brushRadius.floatValue)
                        {
                            instance.SetActive(false);
                            continue;
                        }

                        instance.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal.GetPerpendicularVector(), hit.normal));
                        instance.transform.localRotation *= Quaternion.AngleAxis(instanceData.LocalYRotationAngle, Vector3.up);
                        instance.transform.localScale = instanceData.LocalScale * Vector3.one;
                        instance.SetActive(true);
                    }
                    else
                    {
                        // If raycast failed: hide instance
                        instance.SetActive(false);
                    }
                }
            }
        }

        private void CheckForGameObjectsToBeDeletedInsideBrushVolume()
        {
            if (!m_gameObjectPainter || m_gameObjectPainter.transform.childCount < 1)
            {
                return;
            }

            m_gameObjectsToBeDeleted = new List<GameObject>();

            for (int i = 0; i < m_gameObjectPainter.transform.childCount; i++)
            {
                var testInstance = m_gameObjectPainter.transform.GetChild(i);
                if (testInstance)
                {
                    switch (m_gameObjectPainter.CurrentBrushShape)
                    {
                        case GameObjectPainter.BrushShape.Sphere:
                            if (Vector3.Distance(testInstance.transform.position, m_mouseRayHitPoint) <= m_brushRadius.floatValue)
                            {
                                HandlesExtensions.DrawColoredWireDisc(testInstance.transform.position, Vector3.up, 1, Color.red);
                                m_gameObjectsToBeDeleted.Add(testInstance.gameObject);
                            }
                            break;

                        case GameObjectPainter.BrushShape.Cylinder:
                            var brushCylinderTopSurfaceCenter = m_mouseRayHitPoint + m_brushHeight.floatValue * m_mouseRayHitNormal;
                            var brushCylinderBottomSurfaceCenter = m_mouseRayHitPoint - m_brushHeight.floatValue * m_mouseRayHitNormal;
                            if (testInstance.transform.position.IsPointInCylinder(brushCylinderTopSurfaceCenter, brushCylinderBottomSurfaceCenter, m_brushRadius.floatValue))
                            {
                                HandlesExtensions.DrawColoredWireDisc(testInstance.transform.position, Vector3.up, 1, Color.red);
                                m_gameObjectsToBeDeleted.Add(testInstance.gameObject);
                            }
                            break;

                        case GameObjectPainter.BrushShape.Box:
                            var boxRotation = Quaternion.LookRotation(m_mouseRayHitNormal.GetPerpendicularVector().RotateVectorAlongAxis(m_mouseRayHitNormal, m_offsetRotateAngle.floatValue), m_mouseRayHitNormal);
                            var boxForward = boxRotation * Vector3.forward;
                            var boxUp = boxRotation * Vector3.up;
                            var boxRight = boxRotation * Vector3.right;
                            var boxHalfSize = new Vector3(m_brushRadius.floatValue / 2, m_brushHeight.floatValue, m_brushRadius.floatValue / 2);
                            var boxPoints = new Vector3[]
                            {
                                m_mouseRayHitPoint + boxHalfSize.x * boxForward + boxHalfSize.y * boxUp + boxHalfSize.z * boxRight,
                                m_mouseRayHitPoint + boxHalfSize.x * boxForward + boxHalfSize.y * boxUp - boxHalfSize.z * boxRight,
                                m_mouseRayHitPoint + boxHalfSize.x * boxForward - boxHalfSize.y * boxUp + boxHalfSize.z * boxRight,
                                m_mouseRayHitPoint + boxHalfSize.x * boxForward - boxHalfSize.y * boxUp - boxHalfSize.z * boxRight,
                                m_mouseRayHitPoint - boxHalfSize.x * boxForward + boxHalfSize.y * boxUp + boxHalfSize.z * boxRight,
                                m_mouseRayHitPoint - boxHalfSize.x * boxForward + boxHalfSize.y * boxUp - boxHalfSize.z * boxRight,
                                m_mouseRayHitPoint - boxHalfSize.x * boxForward - boxHalfSize.y * boxUp + boxHalfSize.z * boxRight,
                                m_mouseRayHitPoint - boxHalfSize.x * boxForward - boxHalfSize.y * boxUp - boxHalfSize.z * boxRight,
                            };
                            if (testInstance.transform.position.IsPointInConvexPolygon(boxPoints))
                            {
                                HandlesExtensions.DrawColoredWireDisc_ZTest(testInstance.transform.position, Vector3.up, 1, Color.red, Color.red.SetAlpha(0.25f));
                                m_gameObjectsToBeDeleted.Add(testInstance.gameObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
