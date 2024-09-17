using RPG.Combat;
using RPG.Movement;
using RPG.Stats;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private CursorMapping[] _cursorMappings;
        private Mover _mover;
        private Fighter _fighter;
        private Health _health;

        /// <summary> Радиус, в пределах которого можно навести курсор и атаковать врага.</summary>
        private const float SphereCastRadius = 1f;
        private const float MaxNavMeshProjectionDistance = 1f;

        private bool IsPlayerDead => _health.IsDead;

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
            _fighter = GetComponent<Fighter>();
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
            RaycastHit[] hits = SphereCastAllSorted();
            foreach (RaycastHit hit in hits)
            {
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

        private RaycastHit[] SphereCastAllSorted()
        {
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), SphereCastRadius);

            // Sort the hits by distance
            var distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            System.Array.Sort(distances, hits);
            return hits;
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

        private static Ray GetMouseRay() => Camera.main.ScreenPointToRay(Input.mousePosition);
    }
}
