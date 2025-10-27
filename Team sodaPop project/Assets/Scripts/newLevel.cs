using UnityEngine;
using UnityEngine.SceneManagement;

public class newLevel : MonoBehaviour
{
    [Header("Transport to new level settings")] [SerializeField]
    private string newLevelName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) SceneManager.LoadScene(newLevelName);
    }
}