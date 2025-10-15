using UnityEngine;

public class MazeExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Gamemanager.Instance.WinGame();
        }
    }
}
