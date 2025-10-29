using System.Collections.Generic;
using UnityEngine;

public class doorFunctions : MonoBehaviour
{
    [SerializeField] private doorType type;
    [SerializeField] private List<GameObject> enemies;

    private bool canOpen;

    private int enemyCount;
    private bool playerInTrigger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        canOpen = false;
        enemyCount = enemies.Count;
    }

    // Update is called once per frame
    private void Update()
    {
        if (type == doorType.locked)
            if (playerInTrigger && Input.GetButtonDown("Interact"))
                unlockDoor();

        if (type == doorType.enemy)
        {
            if (playerInTrigger && Input.GetButtonDown("Interact")) enemyCheck();
            if (canOpen) Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
    }

    private void unlockDoor()
    {
        if (gamemanager.instance.keyCount > 0)
        {
            gamemanager.instance.keyCount--;
            gamemanager.instance.updateKeyCount();

            Destroy(gameObject);
        }
    }

    private void enemyCheck()
    {
        foreach (var go in enemies)
            if (go == null)
                enemyCount--;

        if (enemyCount == 0)
            canOpen = true;
        else
            canOpen = false;
    }

    private enum doorType
    {
        locked,
        enemy,
        shoot
    }
}