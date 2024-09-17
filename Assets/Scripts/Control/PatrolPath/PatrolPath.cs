using UnityEngine;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        [SerializeField] private Waypoint[] _waypoints;

        public Waypoint[] Waypoints => _waypoints;

        [System.Serializable]
        public struct Waypoint
        {
            public Vector3 position;
        }

        public int GetNextIndex(int currentIndex)
        {
            return currentIndex + 1 == _waypoints.Length ? 0 : currentIndex + 1;
        }

        public Vector3 GetWaypoint(int index)
        {
            return _waypoints[index].position;
        }
    }
}