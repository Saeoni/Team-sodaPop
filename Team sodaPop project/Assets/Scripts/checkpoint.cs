using UnityEngine;
using System.Collections;
public class checkpoint : MonoBehaviour
{
    [SerializeField] Renderer model;

    Color colorOrig;

    private void Start()
    {
        colorOrig = model.material.color;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Gamemanager.Instance.playerSpawnPos.transform.position != transform.position)
        {
            Gamemanager.Instance.playerSpawnPos.transform.position = transform.position;
            StartCoroutine(checkpointFeedback());
        }
    }

    IEnumerator checkpointFeedback()
    {
        Gamemanager.Instance.checkpointPopup.SetActive(true);
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        model.material.color = colorOrig;
        Gamemanager.Instance.checkpointPopup.SetActive(false);
    }
}
