using UnityEngine;

public class PickUp : MonoBehaviour
{

    enum pickupType { health, key, stealth, gun }

    [SerializeField] pickupType type;

    [SerializeField] int healAmount;
    [SerializeField] gunstats gun;

    public bool pickUpable;
    public bool isPickedUp = false;


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

            GameManager.instance.UpdateKeyCount();

            Destroy(gameObject);
        }

        else if (type == pickupType.stealth)
        {
            GameManager.instance.PlayerUICtrl.StealthTimer(10.0f);
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
