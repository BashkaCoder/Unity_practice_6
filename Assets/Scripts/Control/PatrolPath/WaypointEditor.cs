#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RPG.Control
{
    [CustomEditor(typeof(PatrolPath))]
    public class WaypointEditor : Editor
    {
        private PatrolPath _patrolPath;
        private SerializedProperty _waypointsProperty;

        private void OnEnable()
        {
            _patrolPath = (PatrolPath)target;
            _waypointsProperty = serializedObject.FindProperty("_waypoints");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Waypoints:", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            serializedObject.Update();

            for (int i = 0; i < _waypointsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_waypointsProperty.GetArrayElementAtIndex(i),
                    new GUIContent("Waypoint " + (i + 1)));

                MoveToElementButton(i);
                DeleteElementButton(i);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal();

            AddWaypointButton();
            DeleteAllButton();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void MoveToElementButton(int elementIndex)
        {
            if (GUILayout.Button("Move to", GUILayout.Width(60)))
            {
                float x = _waypointsProperty.GetArrayElementAtIndex(elementIndex).FindPropertyRelative("position").FindPropertyRelative("x").floatValue;
                float y = _waypointsProperty.GetArrayElementAtIndex(elementIndex).FindPropertyRelative("position").FindPropertyRelative("y").floatValue;
                float z = _waypointsProperty.GetArrayElementAtIndex(elementIndex).FindPropertyRelative("position").FindPropertyRelative("z").floatValue;

                SceneView.lastActiveSceneView.LookAt(new Vector3(x, y, z));
            }
        }

        private void DeleteElementButton(int elementIndex)
        {
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                _waypointsProperty.DeleteArrayElementAtIndex(elementIndex);
            }
        }

        private void AddWaypointButton()
        {
            if (GUILayout.Button("Add Waypoint"))
            {
                _waypointsProperty.InsertArrayElementAtIndex(_waypointsProperty.arraySize);
            }
        }

        private void DeleteAllButton()
        {
            if (GUILayout.Button("Delete All"))
            {
                for (int i = _waypointsProperty.arraySize - 1; i >= 0; i--)
                {
                    _waypointsProperty.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private void OnSceneGUI()
        {
            if (_patrolPath == null || _patrolPath.Waypoints == null) return;

            Handles.color = Color.cyan;

            for (int i = 0; i < _patrolPath.Waypoints.Length; i++)
            {
                Vector3 waypointPosition = _patrolPath.Waypoints[i].position;
                const float WaypointGizmosRadius = 0.3f;
                Handles.DrawSolidDisc(waypointPosition, Vector3.up, WaypointGizmosRadius);

                int j = _patrolPath.GetNextIndex(i);
                Handles.DrawLine(waypointPosition, _patrolPath.Waypoints[j].position);

                EditorGUI.BeginChangeCheck();
                waypointPosition = Handles.PositionHandle(waypointPosition, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_patrolPath, "Move Waypoint");
                    _patrolPath.Waypoints[i].position = waypointPosition;
                }
            }
        }
    }
}
#endif