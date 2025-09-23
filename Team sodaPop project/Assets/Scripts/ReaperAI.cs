using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class ReaperAI : MonoBehaviour, IDamage
{
    [Header("Core Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Renderer model;
    [SerializeField] private GameObject keyPrefab;

    private Transform player;
    private Color originalColor;






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void takeDamage(int amount)
    {

    }
}
