using UnityEngine;

public class spawner : MonoBehaviour
{
    [Header("Spawn settings")] [SerializeField]
    private GameObject objectToSpawn;

    [SerializeField] private int numToSpawn;
    [SerializeField] private int spawnRate;
    [SerializeField] private Transform[] spawnPos;
    private int spawnCount;

    private float spawnTimer;
    private bool startSpawn;

    private void Start()
    {
    }

    private void Update()
    {
        if (startSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnCount < numToSpawn && spawnTimer >= spawnRate) spawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) startSpawn = true;
    }

    private void spawn()
    {
        var arrayPos = Random.Range(0, spawnPos.Length);

        Instantiate(objectToSpawn, spawnPos[arrayPos].position, spawnPos[arrayPos].rotation);
        spawnCount++;
        spawnTimer = 0;
    }
}