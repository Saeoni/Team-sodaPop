using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{
    public static gamemanager instance;

    [SerializeField] private GameObject menuActive;
    [SerializeField] private GameObject menuStart;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuWin;
    [SerializeField] private GameObject menuLose;
    [SerializeField] private GameObject menuNote;
    [SerializeField] private TMP_Text gameTimerText;
    [SerializeField] private TMP_Text KeyText;
    [SerializeField] private TMP_Text stealthTimerText;

    public Image playerHPBar;
    public GameObject playerDamageFlash;
    public GameObject playerHealFlash;
    public GameObject checkpointPopup;
    public TMP_Text ammoCur, ammoMax;

    public GameObject playerSpawnPos;
    public GameObject player;
    public playerController playerScript;

    public int keyCount;

    public bool isPaused;
    public bool NoteDisplayed;
    public bool isStealthed;
    public float timeElapsed;

    private int gameGoalCount;
    private int gameTimerMinute;
    private float gameTimerSecond;
    private float stealthTimeLeft;

    private float timeScaleOrig;

    /*void Start()
    {
        statePause();
        menuActive = menuStart;
        menuActive.SetActive(true);
    }*/

    private void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;
        NoteDisplayed = false;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

        keyCount = 0;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
            else if (menuActive == menuNote)
            {
                NoteDisplay();
            }
        }

        updateGameTimer();
    }


    public void stealthTimer(float length)
    {
        StartCoroutine(StealthCountdown(length));
    }

    private IEnumerator StealthCountdown(float length)
    {
        isStealthed = true;

        var countDown = length;
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

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void WinGame()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
        Debug.Log("Player exited the maze. You win!");
    }

    public void updateGameTimer()
    {
        if (menuActive == null)
        {
            gameTimerSecond += Time.deltaTime;
            timeElapsed += Time.deltaTime;

            var displaySecond = Mathf.FloorToInt(gameTimerSecond);
            if (displaySecond >= 60)
            {
                gameTimerMinute++;
                gameTimerSecond = 0;
                displaySecond = 0;
            }

            gameTimerText.text = gameTimerMinute.ToString("00") + ":" + displaySecond.ToString("00");
        }
    }

    public void updateGameGoal(int amount)
    {
    }

    public void updateKeyCount()
    {
        KeyText.text = keyCount.ToString();
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void OnPlayerKilledByReaper()
    {
        Debug.Log("Player Killed by Reaper!");

        if (playerDamageFlash != null) playerDamageFlash.SetActive(true);

        if (player != null) Destroy(player);

        youLose();
    }

    public void NoteDisplay()
    {
        if (NoteDisplayed)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                NoteDisplayed = !NoteDisplayed;
                Time.timeScale = timeScaleOrig;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                menuActive.SetActive(false);
                menuActive = null;
            }
        }
        else
        {
            NoteDisplayed = !NoteDisplayed;
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            menuActive = menuNote;
            menuActive.SetActive(true);
        }
    }
}