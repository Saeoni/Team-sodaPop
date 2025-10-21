using UnityEngine;
using UnityEngine.UI;


public class Gamemanager : MonoBehaviour
{

    public static Gamemanager Instance;
    public playerController playerController;

    [SerializeField] private GameObject menuActive;
    [SerializeField] private GameObject menuUI;

    public HUDController hudUI;
    public MenuController menuController;
    public StartMenuController startMenuController;
    public GameObject playerHpBar; 
    public Image playerHpBarFill;


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
    public bool isStealthed;

    public float timeElapsed;

    // Noise logic 
    public float noiseLevel;
    public float noiseDecayRate = 1f;
    public float noiseThreshold = 10f;

    private int _gameGoalCount;
    private int _gameTimerMinute;
    private float _gameTimerSecond;
    private float _stealthTimeLeft;

    public bool playerIsDead;


    private float _timeScaleOrig;

    public Gamemanager(GameObject menuActive)
    {
        this.menuActive = menuActive;
    }

    public void Awake()
    {

        Instance = this;
        _timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag($"Player Spawn Pos");

        playerIsDead = false;

    }

    public void Start()
    {


        menuController = MenuController.instance;
        hudUI = HUDController.instance;
        startMenuController = StartMenuController.Instance;



    }

    private void Update()
    {

        if (Input.GetButtonDown("Cancel"))
        {
            if (startMenuController.isShowing || playerIsDead) return;

            if (menuActive == null)
            {

                StatePause();
                menuActive = menuUI;

                menuActive.SetActive(true);
                menuController.OpenPauseMenu();


            }
            else if (menuActive == menuUI)
            {

                StateUnpause();



            }
        }



        // Noise decay logic
        noiseLevel = Mathf.Max(0f, noiseLevel - noiseDecayRate * Time.deltaTime);
    }

    public void AddNoise(float amount)
    {
        noiseLevel += amount;
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
        menuController.CloseMenu();
        Time.timeScale = _timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;


        if (menuActive == null) return;

        menuActive.SetActive(false);

        menuActive = null;



    }

    public void WinGame()
    {
        StatePause();
        menuActive = menuUI;
        menuActive.SetActive(true);
        menuController.OpenWinMenu();

        Debug.Log("Player exited the maze. You win!");
    }



    public void UpdateGameGoal(int amount)
    {
        HUDController.instance.UpdateEnemyCount(amount);

    }


    public void YouLose()
    {
        playerIsDead = true;
        StatePause();
        menuActive = menuUI;

        menuActive.SetActive(true);
        menuController.OpenLoseMenu();


        if (playerDamageFlash == null || player == null) return;
        playerDamageFlash.SetActive(true);
       playerController.KillPlayer();
    }


}