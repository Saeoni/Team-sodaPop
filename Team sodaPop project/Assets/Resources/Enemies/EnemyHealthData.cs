using UnityEngine;

[CreateAssetMenu(fileName = "EnemyHealthData", menuName = "Scriptable Objects/EnemyHealthData")]
public class EnemyHealthData : ScriptableObject
{
    public int maxHealth;
    public int health;
    public float healthRegenRate;
    public float healthRegenDelay;


    public int GetMaxHealth() => maxHealth;
    public float GetHealthRegenRate() => healthRegenRate;
    public float GetHealthRegenDelay() => healthRegenDelay;

    public void SetHealth(int value) => health = Mathf.Clamp(value, 0, maxHealth);
    public int GetHealth() => health;
    public void ResetHealth() => health = maxHealth;

    public void TakeDamage(int damage)
    {
        SetHealth(health - damage);
    }



}
