using UnityEngine;

public class spawner : MonoBehaviour
{
    [Header("Spawn settings")]
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] int numToSpawn;
    [SerializeField] int spawnRate;
    [SerializeField] Transform[] spawnPos;

    float spawnTimer;
    int spawnCount;
    bool startSpawn;

    void Start()
    {
        
    }

    void Update()
    {
        if (startSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnCount < numToSpawn && spawnTimer >= spawnRate)
            {
                spawn();
            }
          
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            startSpawn = true;
        }
    }

    void spawn()
    {
        int arrayPos = Random.Range(0, spawnPos.Length);

        Instantiate(objectToSpawn, spawnPos[arrayPos].position, spawnPos[arrayPos].rotation);
        spawnCount++;
        spawnTimer = 0;
        
    }
}
