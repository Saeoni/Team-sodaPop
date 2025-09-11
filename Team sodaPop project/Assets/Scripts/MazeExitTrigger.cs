using UnityEngine;

public class MazeExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gamemanger.instance.WinGame();
        }
    }
}
