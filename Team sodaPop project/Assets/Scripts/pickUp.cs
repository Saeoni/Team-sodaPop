using UnityEngine;
using System.Collections;

public class pickUp : MonoBehaviour
{

    enum pickupType { health, key, stealth, gun}

    [SerializeField] pickupType type;

    [SerializeField] int healAmount;
    [SerializeField] Gunstats gun;


    //    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //    void Start()
    //    {

    //    }

    //    // Update is called once per frame
    //    void Update()
    //    {

    //    }

    private void OnTriggerEnter(Collider other)
    {
        IPickup pickupable = other.GetComponent<IPickup>();
        if (type == pickupType.health)
        {
            other.GetComponent<PlayerController>().heal(healAmount);
            Destroy(gameObject);
        }

        else if (type == pickupType.key)
        {
            Gamemanager.Instance.keyCount++;
            Gamemanager.Instance.UpdateKeyCount();

            Destroy(gameObject);
        }

        else if (type == pickupType.stealth)
        {
            Gamemanager.Instance.StealthTimer(10.0f);
            Destroy(gameObject);
        }

        else if (pickupable != null)
        {
            gun.ammoCur = gun.ammoMax;
            pickupable.getGunStats(gun);
            Destroy(gameObject);
        }

    }
}
