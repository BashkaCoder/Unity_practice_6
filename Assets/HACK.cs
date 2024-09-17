using UnityEngine;

public class HACK : MonoBehaviour
{
    private void Start()
    {
        GameObject.FindWithTag("Player").GetComponent<Animator>().enabled = false;
        GameObject.FindWithTag("Player").GetComponent<Animator>().enabled = true;
    }
}
