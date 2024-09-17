using RPG.Saving;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        public const string DefaultSaveFile = "Save";

        private void Awake()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0) return;
            StartCoroutine(LoadLastScene());
        }

        public static IEnumerator LoadLastScene()
        {
            yield return SavingSystem.LoadLastScene(DefaultSaveFile);
            Fader.Instance.FadeOutImmediate();
            yield return Fader.Instance.FadeIn(Fader.FadeInTime);
        }

        public static void Load()
        {
            SavingSystem.Load(DefaultSaveFile);
        }

        public static void Save()
        {
            SavingSystem.Save(DefaultSaveFile);
        }

        public static void Delete()
        {
            SavingSystem.Delete(DefaultSaveFile);
            print("Delete Save File");
        }

        public static bool SaveFileExist() =>
            System.IO.File.Exists(SavingSystem.GetPathFromSaveFile(DefaultSaveFile));

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();
            }
        }
#endif
    }
}
