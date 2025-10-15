using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Serialization;

public class Gamemanager : MonoBehaviour
{

    public static Gamemanager Instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuNote;
    [SerializeField] TMP_Text gameTimerText;
    [FormerlySerializedAs("KeyText")] [SerializeField] TMP_Text keyText;
    [SerializeField] TMP_Text stealthTimerText;

    [FormerlySerializedAs("playerHPBar")] public Image playerHpBar;
    public GameObject playerDamageFlash;
    public GameObject playerHealFlash;
    public GameObject checkpointPopup;
    public int ammoCur;
    public int ammoMax;

    public GameObject playerSpawnPos;
    public GameObject player;
    public PlayerController playerScript;

    public float noiseLevel;
    public float noiseThreshold;
    public event Action<float> OnNoiseChanged;
    
    public int keyCount;

    public bool isPaused;
    [FormerlySerializedAs("NoteDisplayed")] public bool noteDisplayed;
    public bool isStealthed;
    public float timeElapsed;

    private int _gameGoalCount;
    private int _gameTimerMinute;
    private float _gameTimerSecond;
    private float _stealthTimeLeft;

    private float _timeScaleOrig;
    public bool _playerIsDead;

    private void Awake()
    {

        Instance = this;
        _timeScaleOrig = Time.timeScale;
        noteDisplayed = false;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

        keyCount = 0;

    }

    private void Update()
    {

        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {

                StatePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

            }
            else if (menuActive == menuPause)
            {
                StateUnpause();
            }
            else if(menuActive == menuNote)
            {
                NoteDisplay();
            }
        }

        UpdateGameTimer();

    }

    public void RegisterNoise(float amount)
    {
        noiseLevel += amount;
        OnNoiseChanged?.Invoke(noiseLevel);
    }
    public void StealthTimer(float length)
    {
        StartCoroutine(StealthCountdown(length));

    }

    private IEnumerator StealthCountdown(float length)
    {
        isStealthed = true;

        float countDown = length;
        stealthTimerText.gameObject.SetActive(true);
        while (countDown > 0)
        {
            stealthTimerText.text = "Invisible: " + Mathf.CeilToInt(countDown) + "s";

            countDown -= Time.deltaTime;
            yield return null;
        }
        isStealthed = false;
        stealthTimerText.gameObject.SetActive(false);

    }
    
    public void StatePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = _timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void WinGame()
    {
       StatePause();
       menuActive = menuWin;
       menuActive.SetActive(true);
       Debug.Log("Player exited the maze. You win!");      
    }

    public void UpdateGameTimer()
    {
        if (menuActive != null) return;
        _gameTimerSecond += Time.deltaTime;
        timeElapsed += Time.deltaTime;

        int displaySecond = Mathf.FloorToInt(_gameTimerSecond);
        if (displaySecond >= 60)
        {
            _gameTimerMinute++;
            _gameTimerSecond = 0;
            displaySecond = 0;
        }
        gameTimerText.text = _gameTimerMinute.ToString("00") + ":" + displaySecond.ToString("00");
    }

    public void UpdateGameGoal(int amount)
    {

    }

    public void UpdateKeyCount()
    {
        keyText.text = keyCount.ToString();
    }

    public void YouLose()
    {
        StatePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void OnPlayerKilledByReaper()
    {
        Debug.Log("Player Killed by Reaper!");

       if (playerDamageFlash != null)
       {
          playerDamageFlash.SetActive(true);
       }

       if (player != null)
       {
            Destroy(player);
       }

       YouLose();
    }

    public void NoteDisplay()
    {
        if(noteDisplayed)
        {
            if (!Input.GetButtonDown("Cancel")) return;
            noteDisplayed = !noteDisplayed;
            Time.timeScale = _timeScaleOrig;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            menuActive.SetActive(false);
            menuActive = null;

        }
        else
        {
            noteDisplayed = !noteDisplayed;
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            menuActive = menuNote;
            menuActive.SetActive(true);
        }
    }
}