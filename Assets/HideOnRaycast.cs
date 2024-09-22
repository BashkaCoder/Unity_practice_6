using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HideOnRaycast : MonoBehaviour
{
    private Camera _mainCamera; // ������ �� �������� ������
    private LayerMask _layerMask; // ����� �����, �� ������� ����� ����������� Raycast
    private Transform _player;
    private static List<GameObject> _hiddenObjects = new();
    private const float SphereCastRadius = 1f;
    private const int MaxHits = 10;
    private RaycastHit[] _hits = new RaycastHit[MaxHits];

    private void Start()
    {
        _layerMask = LayerMask.NameToLayer("NavMesh");
        _mainCamera = Camera.main;
        _player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {

        HideObjects();
        ShowObjects();

    }

    private void HideObjects()
    {
        RaycastHit[] hits = SphereCastAllSorted();
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Player")) break;

            if (!_hiddenObjects.Contains(hit.collider.gameObject))
            {
                hit.collider.gameObject.GetComponent<Renderer>().enabled = false;
                _hiddenObjects.Add(hit.collider.gameObject);
            }
        }
    }

    private void ShowObjects()
    {
        RaycastHit[] hits = SphereCastAllSorted();
        var objectsToRemove = new List<GameObject>();
        foreach (GameObject hiddenObject in _hiddenObjects)
        {
            if (NeedToOpen(hits, hiddenObject))
            {
                hiddenObject.GetComponent<Renderer>().enabled = true;
                objectsToRemove.Add(hiddenObject);
            }
        }
        _hiddenObjects.RemoveAll(item => objectsToRemove.Any(obj => obj == item));
    }

    private bool NeedToOpen(RaycastHit[] hits, GameObject obj)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == obj) return false;
        }
        return true;
    }

    private RaycastHit[] SphereCastAllSorted()
    {
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(_player.position);
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.SphereCastAll(ray, SphereCastRadius);

        // Sort the hits by distance
        var distances = new float[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            distances[i] = hits[i].distance;
        }
        System.Array.Sort(distances, hits);
        return hits;
    }
}