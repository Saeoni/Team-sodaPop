using System.Collections;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    [SerializeField] private Renderer model;

    private Color colorOrig;

    private void Start()
    {
        colorOrig = model.material.color;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gamemanager.instance.playerSpawnPos.transform.position != transform.position)
        {
            gamemanager.instance.playerSpawnPos.transform.position = transform.position;
            StartCoroutine(checkpointFeedback());
        }
    }

    private IEnumerator checkpointFeedback()
    {
        gamemanager.instance.checkpointPopup.SetActive(true);
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        model.material.color = colorOrig;
        gamemanager.instance.checkpointPopup.SetActive(false);
    }
}