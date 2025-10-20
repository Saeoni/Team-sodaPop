using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class gamemanager : MonoBehaviour
{

    public static gamemanager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuStart;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuNote;
    [SerializeField] TMP_Text gameTimerText;
    [SerializeField] TMP_Text KeyText;
    [SerializeField] TMP_Text stealthTimerText;

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

    int gameGoalCount;
    int gameTimerMinute;
    float gameTimerSecond;
    float stealthTimeLeft;

    float timeScaleOrig;

    /*void Start()
    {
        statePause();
        menuActive = menuStart;
        menuActive.SetActive(true);
    }*/

    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;
        NoteDisplayed = false;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

        keyCount = 0;

    }

    void Update()
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
            else if(menuActive == menuNote)
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
        if (menuActive == null){
            gameTimerSecond += Time.deltaTime;
            timeElapsed += Time.deltaTime;

            int displaySecond = Mathf.FloorToInt(gameTimerSecond);
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

       if (playerDamageFlash != null)
       {
          playerDamageFlash.SetActive(true);
       }

       if (player != null)
       {
            Destroy(player);
       }

       youLose();
    }

    public void NoteDisplay()
    {
        if(NoteDisplayed == true)
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