using UnityEngine;
using UnityEngine.UI;


public class gamemanager : MonoBehaviour
{

    public static gamemanager instance;
    public playerController _player;


    [SerializeField] GameObject menuActive;
<<<<<<< HEAD
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuNote;
    [SerializeField] TMP_Text gameTimerText;
    [SerializeField] TMP_Text KeyText;
    [SerializeField] TMP_Text stealthTimerText;
=======
    [SerializeField] GameObject menuUI;

    public HUDController hudUI;
    public MenuController menuController;
    public StartMenuController startMenuController;

    public GameObject playerHPBar;
    public Image playerHPBarFill;

>>>>>>> enemyAI_Tobias

    public GameObject playerDamageFlash;
    public GameObject playerHealFlash;
    //public GameObject playerRespawnFlash;
    //public GameObject playerLowHealthFlash;


    public GameObject checkpointPopup;

    public int ammoCur;
    public int ammoMax;
    public GameObject playerSpawnPos;
    public GameObject player;
    public playerController playerScript;


    public bool isPaused;
    public bool NoteDisplayed;
    public bool isStealthed;

    public float timeElapsed;

    // Noise logic 
    public float noiseLevel = 0f;
    public float noiseDecayRate = 1f;
    public float noiseThreshold = 10f;

    int gameGoalCount;
    int gameTimerMinute;
    float gameTimerSecond;
    float stealthTimeLeft;

    public bool playerIsDead;


    float timeScaleOrig;

    void Awake()
    {

        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

        playerIsDead = false;

    }
    void Start()
    {


        menuController = MenuController.instance;
        hudUI = HUDController.instance;
        startMenuController = StartMenuController.instance;



    }

    void Update()
    {

        if (Input.GetButtonDown("Cancel"))
        {
            if (startMenuController.isShowing || playerIsDead) return;

            if (menuActive == null)
            {

                statePause();
                menuActive = menuUI;

                menuActive.SetActive(true);
                menuController.OpenPauseMenu();


            }
            else if (menuActive == menuUI)
            {

                stateUnpause();



            }
        }



        // Noise decay logic
        noiseLevel = Mathf.Max(0f, noiseLevel - noiseDecayRate * Time.deltaTime);
    }

    public void AddNoise(float amount)
    {
        noiseLevel += amount;
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
        menuController.CloseMenu();
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;


        if (menuActive == null) return;

        menuActive.SetActive(false);

        menuActive = null;



    }

    public void WinGame()
    {
        statePause();
        menuActive = menuUI;
        menuActive.SetActive(true);
        menuController.OpenWinMenu();

        Debug.Log("Player exited the maze. You win!");
    }



    public void updateGameGoal(int amount)
    {
        HUDController.instance.UpdateEnemyCount(amount);

    }


    public void youLose()
    {
        playerIsDead = true;
        statePause();
        menuActive = menuUI;

        menuActive.SetActive(true);
        menuController.OpenLoseMenu();



       if (playerDamageFlash != null && player != null)
       {
          playerDamageFlash.SetActive(true);
          _player.KillPlayer();
            
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