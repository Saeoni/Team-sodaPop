using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{

    enum damageType { moving, stationary, DOT, homing}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] GameObject explosionPrefab;

    bool isDamaging;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //moving projectiles will disappear after a certain time
        if(type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if(type == damageType.moving)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (type == damageType.homing)
        {
            //checks player position and follows it
            rb.linearVelocity = (gamemanager.instance.player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg != null && (type == damageType.moving || type == damageType.homing))
        {
            dmg.takeDamage(damageAmount);
        }

        if(type == damageType.homing || type == damageType.moving)
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.isTrigger) 
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if(dmg != null & type == damageType.DOT)
        {
            if(!isDamaging)
            {
                StartCoroutine(damageother(dmg));
            }
        }
    }

    IEnumerator damageother(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}
