using UnityEngine;

public class spawner : MonoBehaviour
{
    [Header("Spawn settings")]
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] Transform[] spawnPos;

    float spawnTimer;
    bool startSpawn;

    void Start()
    {
        
    }

    void Update()
    {
        if (startSpawn)
        {
            spawnTimer += Time.deltaTime;
            spawn();
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
        spawnTimer = 0;
        
    }
}
