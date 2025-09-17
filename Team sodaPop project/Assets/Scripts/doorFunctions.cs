using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

public class doorFunctions : MonoBehaviour
{
    enum doorType { locked, enemy, shoot}

    [SerializeField] doorType type;
    [SerializeField] List<GameObject> enemies;

    bool canOpen;
    bool playerInTrigger;

    int enemyCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canOpen = false;
        enemyCount = enemies.Count;
    }

    // Update is called once per frame
    void Update()
    {
        if (type == doorType.locked)
        {
            if (playerInTrigger && Input.GetButtonDown("Interact"))
            {
                unlockDoor();
            }
        }

        if (type == doorType.enemy)
        {
            if (playerInTrigger && Input.GetButtonDown("Interact"))
            {
                enemyCheck();
            }
            if (canOpen)
            {
                Destroy(gameObject);
            }    
        }
    }

    void unlockDoor()
    {
        if (gamemanager.instance.keyCount > 0)
        {
            gamemanager.instance.keyCount--;
            gamemanager.instance.updateKeyCount();

            Destroy(gameObject);
        }
    }

    void enemyCheck()
    {
        foreach (GameObject go in enemies)
        {
            if (go == null)
            {
                enemyCount--;
            }
        }

        if(enemyCount==0)
        {
            canOpen = true;
        }
        else
        {
            canOpen = false;
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
}
