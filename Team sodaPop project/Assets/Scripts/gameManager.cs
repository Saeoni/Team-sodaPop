using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using static UIController;

public class gamemanager : MonoBehaviour
{

    public static gamemanager instance;
    

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuUI;
    //[SerializeField] GameObject menuWin;
    //[SerializeField] GameObject menuLose;


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
    public bool isStealthed;
    public float timeElapsed; 

    int gameGoalCount;
    int gameTimerMinute;
    float gameTimerSecond;
    float stealthTimeLeft;

    float timeScaleOrig;

    void Awake()
    {

        instance = this;
        timeScaleOrig = Time.timeScale;

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

                menuActive = menuUI;

                menuActive.SetActive(true);
                UIController.instance.OpenPauseMenu();

            }
            else if (menuActive == menuUI)
            {
             
                stateUnpause();
                
                


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
        UIController.instance.CloseMenu();
        menuActive.SetActive(false);
        menuActive = null;



    }

    public void WinGame()
    {
       statePause();
       menuActive = menuUI;
       menuActive.SetActive(true);
       UIController.instance.OpenWinMenu();
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
        menuActive = menuUI;
        menuActive.SetActive(true);
        UIController.instance.OpenLoseMenu();
        
        
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


}