using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;


    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject playerUIObject;
    [SerializeField] GameObject startMenuUI;



    public GameObject playerDamageFlash;
    public GameObject playerHealFlash;
    //public GameObject playerRespawnFlash;
    //public GameObject playerLowHealthFlash;


    public GameObject checkpointPopup;

    public GameObject playerSpawnPos;
    public GameObject player;
    public PlayerController playerScript;


    public bool isPaused;
    public bool isStealthed;
    public bool playerIsDead;

    public PlayerUIController PlayerUICtrl;
    public MenuController MenuUICtrl;
    public StartMenuController StartMenuCtrl;

    private int gameGoalTotal;
    float timeScaleOrig;

    void Awake()
    {

        instance = this;

        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
        MenuUICtrl = menuUI.GetComponent<MenuController>();
        PlayerUICtrl = playerUIObject.GetComponent<PlayerUIController>();
        StartMenuCtrl = startMenuUI.GetComponent<StartMenuController>();


        playerIsDead = false;
        statePause();
        playerUIObject.SetActive(true);


        startMenuUI.SetActive(true);

    }



    void Update()
    {

        if (Input.GetButtonDown("Cancel"))
        {
            if (StartMenuCtrl.isShowing || playerIsDead) return;

            if (menuActive == null)
            {

                statePause();
                menuActive = menuUI;

                menuActive.SetActive(true);
                MenuUICtrl.OpenPauseMenu();


            }
            else if (menuActive != null)
            {

                stateUnpause();



            }
        }



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
        MenuUICtrl.CloseMenu();
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (menuActive != null)
        {
            menuActive.SetActive(false);

            menuActive = null;
        }



    }

    public void WinGame()
    {
        statePause();
        menuActive = menuUI;
        menuActive.SetActive(true);
        MenuUICtrl.OpenWinMenu();

        Debug.Log("Player exited the maze. You win!");
    }



    public void updateGameGoal(int amount)
    {
        PlayerUICtrl.UpdateEnemyCount(amount);
        playerScript.AddEnemy(amount);

    }


    public void youLose()
    {
        playerIsDead = true;
        statePause();
        menuActive = menuUI;

        menuActive.SetActive(true);
        MenuUICtrl.OpenLoseMenu();


    }

    public void RespawnPlayer()
    {
        playerIsDead = false;
        playerScript.spawnPlayer();
        stateUnpause();
    }
    public void UpdateKeyCount()
    {
        PlayerUICtrl.UpdateKeyCount();
    }

}