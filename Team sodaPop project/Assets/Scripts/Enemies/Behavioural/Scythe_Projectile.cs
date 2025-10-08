using UnityEngine;
using System.Collections;

public class Scythe_Projectile : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] float speed;
    [SerializeField] GameObject impactEffect;
    [SerializeField] Transform pullTarget;

    public bool pullComplete {  get; private set; }

    Transform player;
    bool hasImpaled;
    float arcProgress;

    Vector3 arcStart;
    Vector3 arcEnd;
    Vector3 arcCenter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameManager.instance?.player?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found by Scythe_Projectile.");
            Destroy(gameObject);
            return;
        }

        // Setup arc path
        arcStart = transform.position;
        arcEnd = player.position;
        arcCenter = (arcStart + arcEnd) * 0.5f - Vector3.up * 2f;

        arcStart -= arcCenter;
        arcEnd -= arcCenter;
        arcProgress = 0f;
    }

    void Update()
    {
        if (!hasImpaled)
        {
            arcProgress += Time.deltaTime * speed / Vector3.Distance(arcStart, arcEnd);
            transform.position = Vector3.Slerp(arcStart, arcEnd, arcProgress) + arcCenter;
            transform.LookAt(player);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpaled || !other.CompareTag("Player")) return;

        hasImpaled = true;

        // Freeze player and attach to scythe
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        other.transform.SetParent(transform);

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        StartCoroutine(PullBack());
    }

    IEnumerator PullBack()
    {
        while (Vector3.Distance(transform.position, pullTarget.position) > 0.5f)
        {
            Vector3 dir = (pullTarget.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            yield return null;
        }

        // Final face-to-face moment
        Transform playerTransform = transform.GetChild(0);
        playerTransform.position = pullTarget.position + pullTarget.forward * 0.5f;
        playerTransform.LookAt(pullTarget);

        pullComplete = true;

        // Optional delay before destruction
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    public void SetPullTarget(Transform target)
    {
        pullTarget = target;
    }
}
