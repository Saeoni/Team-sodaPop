using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class Gamemanager : MonoBehaviour
{

    public static Gamemanager Instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text gameTimerText;
    [SerializeField] TMP_Text keyText;
    [SerializeField] TMP_Text stealthTimerText;

    public Image playerHealthBar;
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

    int _gameGoalCount;
    int _gameTimerMinute;
    float _gameTimerSecond;
    float _stealthTimeLeft;

    float _timeScaleOrig;
    
    public bool playerIsDead;
    // Noise logic 
    public float noiseLevel;
    public float noiseDecayRate = 1f;
    public float noiseThreshold = 10f;

    void Awake()
    {

        Instance = this;
        _timeScaleOrig = Time.timeScale;

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

                StatePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

            }
            else if (menuActive == menuPause)
            {
                StateUnpause();
            }
        }

        UpdateGameTimer();
        noiseLevel = Mathf.Max(0f, noiseLevel - noiseDecayRate * Time.deltaTime);
    }

    public void AddNoise(float amount)
    {
        noiseLevel += amount;
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

    private void UpdateGameTimer()
    {
        if (menuActive == null){
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


}