using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class gamemanager : MonoBehaviour
{

    public static gamemanager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text gameTimerText;
    [SerializeField] TMP_Text KeyText;

    public Image playerHPBar;
    public GameObject playerDamageFlash;
    public GameObject playerHealFlash;

    public GameObject player;
    public playerController playerScript;

    public int keyCount;

    public bool isPaused;
    public bool isStealthed;

    int gameGoalCount;
    int gameTimerMinute;
    float gameTimerSecond;

    float timeScaleOrig;

    void Awake()
    {

        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();

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
        }

        updateGameTimer();

    }

    // Still need to get the timer to count down.
    //
    //public void stealthTimer(float length)
    //{
    //    float countDown = length;


    //    while (countDown > 0)
    //    {
    //        countDown -= Time.deltaTime;

    //        if (countDown == 0)
    //        {
    //            isStealthed = false;
    //        }
    //    }
    //}

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
}