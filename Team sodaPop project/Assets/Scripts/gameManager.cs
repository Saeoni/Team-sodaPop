using UnityEngine;
using UnityEngine.UI;
using static MenuController;
using static HUDController;
using static StartMenuController;








public class gamemanager : MonoBehaviour
{

    public static gamemanager instance;
    

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuUI;

    public HUDController hudUI;
    public MenuController menuController;
    public StartMenuController startMenuController;

    public GameObject playerHPBar;
    public Image playerHPBarFill;


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
    public bool playerIsDead;

    float timeScaleOrig;

    void Awake()
    {

        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
        
        

    }
    void Start()
    {
        playerIsDead = false;

        menuController = MenuController.instance;
        hudUI = HUDController.instance;
        startMenuController = StartMenuController.instance;
        


    }

    void Update()
    {
        
        if (Input.GetButtonDown("Cancel") && !playerIsDead)
        {
            
            if (menuActive == null && !startMenuController.isShowing)
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
        menuController.CloseMenu();
        
        if (menuActive != null)
            {menuActive.SetActive(false);

            menuActive = null;
        }



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


    }


}