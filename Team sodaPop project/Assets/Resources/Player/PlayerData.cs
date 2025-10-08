using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public struct PlayerStats
    {
        public string playerName;
        public int maxHealth;
        public float currentHealth;
        public int speed;
        public int stealth;
        public int ammoCount;
        public int maxAmmo;
        public int score;
        public int roomsClear;
        public int keysCollected;
        public int notesCollected;
        public int jumpSpeed;
        public int shootDamage;
        public float shootRate;
        public int shootDist;
        public List<gunstats> gunStats;

    }

    [SerializeField] private string playerName = "Player";

    [SerializeField] private string playerHPPercent = "100%";

    [SerializeField] private GameObject player;
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Sprite playerAvatar;
    [SerializeField] private Sprite playerHPBar;
    [SerializeField] private Sprite playerStealthBar;
    [SerializeField] private Sprite playerHPBarFill;
    [SerializeField] private Sprite playerStealthBarFill;

    [SerializeField] private int maxHealth = 100;

    [SerializeField, Range(0, 100f)] private float currentHealth = 100;
    [SerializeField, Range(0, 15f)] private int speed = 5;
    [SerializeField, Range(0, 1f)] private float scopeZoomSpeed = 0.5f;
    [SerializeField, Range(0, 150f)] private float scopeZoomScale = 100f;

    [SerializeField] private int stealth = 5;
    [SerializeField] private int ammoCount = 30;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int score = 0;
    [SerializeField] private int roomsClear = 1;

    [SerializeField] private int keysCollected = 0;
    [SerializeField] private int notesCollected = 0;
    [SerializeField] private int gravity = 30;
    [SerializeField] private int jumpMax = 2;
    [SerializeField] private int sprintStamina = 5;
    [SerializeField] private int staminaRegen = 1;
    [SerializeField, Range(0, 100f)] private float mouseSensitivity = 1.0f;
    [SerializeField] private int sprintMod = 2;
    [SerializeField] private int jumpSpeed = 5;
    [SerializeField] private int jumpMod = 2;
    [SerializeField] private int shootDamage = 5;
    [SerializeField] private float shootRate = 0.5f;
    [SerializeField] private int shootDist = 15;

    private int collectedModules;
    private int keyCount = 0;
    private int jumpCount = 0;
    private int sprintStaminaCurrent = 5;
    private int currentEnemyCount = 0;
    private float stealthTimer = 0f;

    private GameObject currentBoss;



    public List<gunstats> gunStats = new List<gunstats>();
    public List<gunstats> GetGunStats() { return gunStats; }

    // Track current gun index




    public PlayerStats stats;

    public GameObject Player { get { return player; } }
    public Sprite PlayerIcon { get { return playerIcon; } }
    public Sprite PlayerAvatar { get { return playerAvatar; } }
    public Sprite PlayerHPBar { get { return playerHPBar; } }
    public Sprite PlayerStealthBar { get { return playerStealthBar; } }
    public Sprite PlayerHPBarFill { get { return playerHPBarFill; } }
    public Sprite PlayerStealthBarFill { get { return playerStealthBarFill; } }

    public float ScopeZoomSpeed { get { return scopeZoomSpeed; } set { scopeZoomSpeed = value; } }
    public string PlayerName { get { return playerName; } set { playerName = value; } }

    public string PlayerHPPercent { get { return playerHPPercent; } set { playerHPPercent = value; } }
    public int MaxHealth { get { return maxHealth; } set { maxHealth = value; } }

    public float CurrentHealth { get { return currentHealth; } set { currentHealth = value; } }

    public int AmmoCount { get { return ammoCount; } set { ammoCount = value; } }
    public int Speed { get { return speed; } set { speed = value; } }


    public int Stealth { get { return stealth; } set { stealth = value; } }

    public int MaxAmmo { get { return maxAmmo; } set { maxAmmo = value; } }
    public int Score { get { return score; } set { score = value; } }
    public int RoomsClear { get { return roomsClear; } set { roomsClear = value; } }

    public int CollectedModules { get { return collectedModules; } set { collectedModules = value; } }
    public int KeyCount { get { return keyCount; } set { keyCount = value; } }

    public int KeysCollected { get { return keysCollected; } set { keysCollected = value; } }

    public int NotesCollected { get { return notesCollected; } set { notesCollected = value; } }
    public int Gravity { get { return gravity; } set { gravity = value; } }
    public int JumpMax { get { return jumpMax; } set { jumpMax = value; } }
    public int SprintStamina { get { return sprintStamina; } set { sprintStamina = value; } }
    public int StaminaRegen { get { return staminaRegen; } set { staminaRegen = value; } }

    public float MouseSensitivity { get { return mouseSensitivity; } set { mouseSensitivity = value; } }
    public int SprintMod { get { return sprintMod; } set { sprintMod = value; } }
    public int JumpSpeed { get { return jumpSpeed; } set { jumpSpeed = value; } }
    public int JumpMod { get { return jumpMod; } set { jumpMod = value; } }
    public int ShootDamage { get { return shootDamage; } set { shootDamage = value; } }
    public float ShootRate { get { return shootRate; } set { shootRate = value; } }
    public int ShootDist { get { return shootDist; } set { shootDist = value; } }
    public int JumpCount { get { return jumpCount; } set { jumpCount = value; } }
    public int SprintStaminaCurrent { get { return sprintStaminaCurrent; } set { sprintStaminaCurrent = value; } }

    public int CurrentEnemyCount { get { return currentEnemyCount; } set { currentEnemyCount = value; } }
    public GameObject CurrentBoss { get { return currentBoss; } set { currentBoss = value; } }
    public float StealthTimer { get { return stealthTimer; } set { stealthTimer = value; } }
    public float ScopeZoomScale { get { return scopeZoomScale; } set { scopeZoomScale = value; } }




    private void OnEnable()
    {
        ResetPlayerData();

    }


    public void ResetPlayerData()
    {
        ResetAmmo();
        ResetHealth();
        speed = 5;
        stealth = 5;
        ammoCount = 30;
        maxAmmo = 30;
        score = 0;
        roomsClear = 0;
        currentBoss = null;
        currentEnemyCount = 0;

        keyCount = 0;
        jumpCount = 0;
        sprintStaminaCurrent = sprintStamina;
        stealthTimer = 0f;
        scopeZoomScale = 100f;


        ClearInventory();


    }
    public void CopyFrom(PlayerData other)
    {
        maxHealth = other.maxHealth;
        currentHealth = other.currentHealth;
        speed = other.speed;
        stealth = other.stealth;
        ammoCount = other.ammoCount;
        maxAmmo = other.maxAmmo;
        score = other.score;
        roomsClear = other.roomsClear;
        keysCollected = other.keysCollected;
        notesCollected = other.notesCollected;
        gunStats = new List<gunstats>(other.gunStats);

    }

    public PlayerStats GetPlayerStats()
    {
        stats.maxHealth = maxHealth;
        stats.currentHealth = currentHealth;
        stats.speed = speed;
        stats.stealth = stealth;
        stats.ammoCount = ammoCount;
        stats.maxAmmo = maxAmmo;
        stats.score = score;
        stats.roomsClear = roomsClear;
        stats.keysCollected = keysCollected;
        stats.notesCollected = notesCollected;
        stats.gunStats = gunStats;

        return stats;
    }

    public void SetPlayerStats(PlayerStats stats)
    {
        this.maxHealth = stats.maxHealth;
        this.currentHealth = stats.currentHealth;
        this.speed = stats.speed;
        this.stealth = stats.stealth;
        this.ammoCount = stats.ammoCount;
        this.maxAmmo = stats.maxAmmo;
        this.score = stats.score;
        this.roomsClear = stats.roomsClear;
        this.keysCollected = stats.keysCollected;
        this.notesCollected = stats.notesCollected;
        this.gunStats = stats.gunStats;

    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;

    }
    public void ResetAmmo()
    {
        ammoCount = maxAmmo;
    }
    public void AddGun(gunstats gun)
    {
        if (!gunStats.Contains(gun))
        {
            gunStats.Add(gun);
        }
    }
    public void RemoveGun(gunstats gun)
    {
        if (gunStats.Contains(gun))
        {
            gunStats.Remove(gun);
        }
    }
    public void ClearGuns()
    {
        gunStats.Clear();
        gunStats = null;
        gunStats = new List<gunstats>();

    }
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    public void ClearInventory()
    {
        keysCollected = 0;
        notesCollected = 0;
        ClearGuns();
    }
    public void AddKey()
    {
        keysCollected++;
    }
    public void AddNote()
    {
        notesCollected++;
    }
    public void AddScore(int amount)
    {
        score += amount;
    }
    public void ClearScore()
    {
        score = 0;
    }
    public void FillHealthBar(int amount)
    {
        // Lerp health bar fill amount

        currentHealth += amount;
        playerHPPercent = ((int)((currentHealth / maxHealth) * 100)).ToString() + "%";

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
        }
        GameManager.instance.PlayerUICtrl.healthChanged = true;

        GameManager.instance.PlayerUICtrl.UpdateHealthBar();
    }



}
