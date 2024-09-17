using System;
using RPG.Combat;
using RPG.Movement;
using RPG.Stats;
using RPG.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        private static Camera _camera;
        
        [SerializeField] private CursorMapping[] _cursorMappings;
        private Mover _mover;
        private Health _health;
        
        private const float SphereCastRadius = 1f;
        private const float MaxNavMeshProjectionDistance = 1f;
        private const int MaxSphereCastHits = 10; // Максимальное количество результатов, которые может вернуть SphereCastNonAlloc

        private bool IsPlayerDead => _health.IsDead;

        private RaycastHit[] _sphereCastHits = new RaycastHit[MaxSphereCastHits]; // Буфер для результатов SphereCastNonAlloc

        [System.Serializable]
        private struct CursorMapping
        {
            public CursorType CursorType;
            public Texture2D Texture;
            public Vector2 Hotspot;
        }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _mover = GetComponent<Mover>();
            _camera = Camera.main;
        }

        private void Update()
        {
            if (InteractWithUI()) return;
            if (IsPlayerDead)
            {
                SetCursor(CursorType.None);
                return;
            }

            if (InteractWithComponent()) return;
            if (InteractWithMovement()) return;

            SetCursor(CursorType.None);
        }

        private bool InteractWithUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }
            return false;
        }

        private bool InteractWithComponent()
        {
            int hitCount = SphereCastAllSorted(); // Используем обновленный метод
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _sphereCastHits[i];
                var raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (var raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }

        private int SphereCastAllSorted()
        {
            int hitCount = Physics.SphereCastNonAlloc(GetMouseRay(), SphereCastRadius, _sphereCastHits);

            // Сортируем результаты по дистанции с использованием IComparer
            Array.Sort(_sphereCastHits, 0, hitCount, new RaycastHitDistanceComparer());

            return hitCount;
        }

        private bool InteractWithMovement()
        {
            bool hasHit = RaycastNavMesh(out Vector3 target);
            if (hasHit)
            {
                if (!_mover.CanMoveTo(target)) return false;

                if (Input.GetMouseButton(1))
                {
                    _mover.StartMoveAction(target);
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();

            // Raycast to terrain
            bool hasHit = Physics.Raycast(GetMouseRay(), out RaycastHit hit);
            if (!hasHit) return false;

            // Find nearest NavMesh point
            bool hasCastToNavMesh = NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit,
                MaxNavMeshProjectionDistance, NavMesh.AllAreas);
            if (!hasCastToNavMesh) return false;

            target = navMeshHit.position;

            return true;
        }

        #region Cursor
        private void SetCursor(CursorType cursorType)
        {
            var mapping = GetCursorMapping(cursorType);
            Cursor.SetCursor(mapping.Texture, mapping.Hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType cursorType)
        {
            foreach (var mapping in _cursorMappings)
            {
                if (mapping.CursorType == cursorType) return mapping;
            }
            return _cursorMappings[0];
        }
        #endregion

        private static Ray GetMouseRay() => _camera.ScreenPointToRay(Input.mousePosition);
    }
}