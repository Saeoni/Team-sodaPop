using UnityEngine;

public interface IPickup
{
    void pickupHealth(int amount);

    void pickupKey();

    void pickupStealth(float duration);
}
