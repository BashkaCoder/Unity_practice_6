using System.Collections.Generic;
using UnityEngine;

namespace RPG.Saving
{
    [ExecuteAlways]
    public class SaveableEntity : MonoBehaviour
    {
        [SerializeField] private string _uniqueIdentifier;
        public string UniqueIdentifier => _uniqueIdentifier;

        public object CaptureState()
        {
            var state = new Dictionary<string, object>();
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                state[saveable.GetType().ToString()] = saveable.CaptureState();
            }
            return state;
        }

        public void RestoreState(object state)
        {
            var stateDict = (Dictionary<string, object>)state;
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                string typeString = saveable.GetType().ToString();
                if (stateDict.ContainsKey(typeString))
                {
                    saveable.RestoreState(stateDict[typeString]);
                }
            }
        }

        #region Setting GUID
#if UNITY_EDITOR
        private static readonly Dictionary<string, SaveableEntity> globalLookup = new();

        // В префабах не должен выставляться GUID.
        private bool IsPrefab => string.IsNullOrEmpty(gameObject.scene.path);

        private void Update()
        {
            if (Application.IsPlaying(gameObject) || IsPrefab) return;
            SetUniqueIdentifier();
        }

        /// <summary> Выставление GUID сущности на сцене.</summary>
        private void SetUniqueIdentifier()
        {
            var serializedObject = new UnityEditor.SerializedObject(this);
            var property = serializedObject.FindProperty("_uniqueIdentifier");
            if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                property.stringValue = System.Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
            }
            globalLookup[property.stringValue] = this;
        }

        private bool IsUnique(string candidate)
        {
            if (!globalLookup.ContainsKey(candidate) || globalLookup[candidate] == this)
                return true;

            if (globalLookup[candidate] == null ||
                globalLookup[candidate].UniqueIdentifier != candidate)
            {
                globalLookup.Remove(candidate);
                return true;
            }
            return false;
        }
#endif
        #endregion
    }
}