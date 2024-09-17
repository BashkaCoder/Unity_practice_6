using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Stats;
using RPG.Utils;
using System;
using UnityEngine;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        #region Fields and Properties
        [Header("Attack Behaviour")]
        [SerializeField, Range(1f, 100f), Tooltip("Радиус обнаружения врага.")]
        private float _detectionRadius = 5f;
        [SerializeField] private float _allowedDistanceDeparture = 30f;
        [SerializeField] private float _aggroCooldownTime = 5f;
        [SerializeField, Tooltip("Радиус крика, который привлекает внимание мобов вокруг.")] private float _shoutRadius = 5f;

        private Fighter _fighter;
        private GameObject _player;
        private float _timeSinceAggrevated = Mathf.Infinity;

        [Header("Suspition Behaviour")]
        [SerializeField] private float _suspicionTime = 5f;
        private float _timeSinceLastSawPlayer = Mathf.Infinity;

        [Header("Patrol Behaviour")]
        [SerializeField] private PatrolPath _patrolPath;
        [SerializeField] private float _waypointTolerance = 1f;
        [SerializeField] private float _waypointDwellTime = 3f;
        [SerializeField, Range(0, 1)] private float _patrolSpeedFraction = 0.2f;
        private float _timeSinceArrivedAtWaypoint = Mathf.Infinity;
        private int _currentWaypointIndex = 0;
        private Mover _mover;
        private LazyValue<Vector3> _guardPosition;

        public Vector3 GuardPosition
        {
            get => _guardPosition.Value;
            set => _guardPosition.Value = value;
        }

        private Health _health;
        #endregion

        private void Awake()
        {
            _fighter = GetComponent<Fighter>();
            _health = GetComponent<Health>();
            _mover = GetComponent<Mover>();
            _player = GameObject.FindWithTag("Player");
            _guardPosition = new(() => transform.position);
        }

        private void Start()
        {
            _guardPosition.ForceInit();
        }

        private void Update()
        {
            if (_health.IsDead) return;

            if (IsAggrevated() && _fighter.CanAttack(_player) &&
                !IsFarFromStartPosition(GuardPosition))
            {
                AttackBehaviour();
            }
            else if (_timeSinceLastSawPlayer < _suspicionTime)
            {
                SuspitionBehaviour();
            }
            else
            {
                PatrolBehaviour();
            }
            UpdateTimers();
        }

        #region AttackBehaviour
        private bool IsAggrevated()
        {
            float distanceToPlayer = Vector3.Distance(_player.transform.position, transform.position);
            bool inAttackRangeOfPlayer = distanceToPlayer < _detectionRadius;
            return inAttackRangeOfPlayer || _timeSinceAggrevated < _aggroCooldownTime;
        }

        private bool IsFarFromStartPosition(Vector3 startPosition)
        {
            return Vector3.Distance(startPosition, transform.position) > _allowedDistanceDeparture;
        }

        private void AttackBehaviour()
        {
            _timeSinceLastSawPlayer = 0;
            _fighter.Attack(_player);

            AggrevateNearbyEnemies();
        }

        private void AggrevateNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, _shoutRadius, Vector3.up, 0);
            foreach (RaycastHit hit in hits)
            {
                var ai = hit.collider.GetComponent<AIController>();
                if (ai == null) continue;

                ai.Aggrevate();
            }
        }

        public void Aggrevate()
        {
            _timeSinceAggrevated = 0;
        }
        #endregion

        #region SuspitionBehaviour
        private void SuspitionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
        #endregion

        #region PatrolBehaviour
        private void PatrolBehaviour()
        {
            Vector3 nextPosition = GuardPosition;

            if (_patrolPath != null && _patrolPath.Waypoints.Length > 0)
            {
                if (AtWaypoint())
                {
                    _timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }

            if (_timeSinceArrivedAtWaypoint > _waypointDwellTime)
            {
                _mover.StartMoveAction(nextPosition, _patrolSpeedFraction);
            }
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < _waypointTolerance;
        }

        private void CycleWaypoint()
        {
            _currentWaypointIndex = _patrolPath.GetNextIndex(_currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint() => _patrolPath.GetWaypoint(_currentWaypointIndex);
        #endregion

        private void UpdateTimers()
        {
            _timeSinceLastSawPlayer += Time.deltaTime;
            _timeSinceArrivedAtWaypoint += Time.deltaTime;
            _timeSinceAggrevated += Time.deltaTime;
        }

        #region Debug
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
        #endregion
    }
}
