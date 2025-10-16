
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class HUDController : MonoBehaviour
{
    public static HUDController instance;

    
    [SerializeField] string[] errorPhrases;
    [SerializeField] string[] warningMessages;
    
    [SerializeField] private GameObject reticle;

    private VisualElement contentContainer;
    private VisualElement minimap;

    private Label enemiesTitle;
    private Label enemyCountText;
    private int enemyCount;

    private Label keyTitle;
    private Label keyCountText;
    [SerializeField] private int totalKeys;
    public int keyCount;
    private Label collectedModulesTitle;
    private Label collectedModulesCount;
    private VisualElement minimapBorder;
    private VisualElement minimapIcon;

    private Label stealthTimerText;
    private Label gameTimerText;
    private VisualElement playerHPBar;
    //public Image playerHPBarFill;
    //private VisualElement bossHPBar;
    //private Image bossHPFill;

    private Label ammoCur;
    private Label ammoMax;

    int gameGoalCount;
    int gameTimerMinute;
    float gameTimerSecond;
    float stealthTimeLeft;

    
    public float timeElapsed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        InitializeUI();
        HideUI();
        enemyCount = 0;
        
        //bossHPBar.SetActive(false);

    }
    
    private void FixedUpdate()
    {
        if (Gamemanager.Instance.isPaused)
        {
            HideUI();
        }
        else
        {
            InitializeUI();
        }
    }


    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        contentContainer = root.Q<VisualElement>("Player_UI");
        
        enemyCountText = root.Q<Label>("Enemy_Count");
        enemiesTitle = root.Q<Label>("Enemies_Title");
        keyTitle = root.Q<Label>("Key_Title");
        keyCountText = root.Q<Label>("Key_Count");
        keyCount = 0;
        totalKeys = 4;
        stealthTimerText = root.Q<Label>("Stealth_Timer");
        stealthTimerText.text = "";
        gameTimerText = root.Q<Label>("Game_Timer");
        gameTimerText.text = "00:00";
        ammoCur = root.Q<Label>("Ammo_Current");
        ammoMax = root.Q<Label>("Ammo_Max");
        //bossHPBar = root.Q<VisualElement>("Boss_HP_Bar");
        //bossHPFill = root.Q<Image>("Boss_HP_Fill");
        //bossHPBar.style.display = DisplayStyle.None; // Hide boss HP bar initially





        collectedModulesTitle = root.Q<Label>("Collected_Modules_Title");
        collectedModulesCount = root.Q<Label>("Collected_Modules_Count");
        minimap = root.Q<VisualElement>("Minimap_Container");
        minimap.style.display = DisplayStyle.None; // Hide minimap initially
        //minimapBorder = root.Q<VisualElement>("Minimap_Border");
        //minimapBorder.style.display = DisplayStyle.None; // Hide minimap border initially
        //minimapIcon = root.Q<VisualElement>("Minimap_Icon");
        //minimapIcon.style.display = DisplayStyle.None; // Hide minimap icon initially

        //collectedModulesCount.text = "0/0";
        enemyCountText.text = "0";
        keyCountText.text = "";
        keyCount = 0;
        enemiesTitle.text = "Enemies Remaining:";
        keyTitle.text = "Keys Collected:";
        collectedModulesTitle.text = "Modules Collected:";
        playerHPBar = root.Q<VisualElement>("Player_HP_Bar");


        contentContainer.style.display = DisplayStyle.Flex;
    }

    public void HideUI()
        {
        contentContainer.style.display = DisplayStyle.None;
    }
    // Update is called once per frame
    void Update()
    {
        // Check if collected modules is empty and hide minimap if so
        UpdateGameTimer();
       

    }
    public void UpdatePlayerUI()
    {
        ammoCur.text = Gamemanager.Instance.ammoCur.ToString();
        ammoMax.text = Gamemanager.Instance.ammoMax.ToString();
        // Update collected modules count
        //collectedModulesCount.text = gamemanager.instance.collectedModules.ToString() + "/5";

    }
    public void UpdateEnemyCount(int amount)
    {
        enemyCount += amount;
        enemyCountText.text = enemyCount.ToString("F0");

    }

    public void UpdateGameTimer()
    {
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
    public void UpdateKeyCount()
    {
        keyCount++;
        keyCountText.text = keyCount.ToString() + "/4";
    }   
    
    
    public void StealthTimer(float length)
    {
        StartCoroutine(StealthCountdown(length));

    }

    private IEnumerator StealthCountdown(float length)
    {
        Gamemanager.Instance.isStealthed = true;

        float countDown = length;
        stealthTimerText.text = countDown.ToString("F0");
        while (countDown > 0)
        {
            stealthTimerText.text = "Invisible: " + Mathf.CeilToInt(countDown) + "s";

            countDown -= Time.deltaTime;
            yield return null;
        }
        Gamemanager.Instance.isStealthed = false;
        stealthTimerText.text = "";
        // stealthTimerText.gameObject.SetActive(false);

    }

    IEnumerator flashText(Label textElement, Color flashColor, float duration)
    {
        Color originalColor = textElement.style.color.value;
        textElement.style.color = flashColor;
        yield return new WaitForSeconds(duration);
        textElement.style.color = originalColor;
    }

    IEnumerator SayRandomFromList(Label textElement, string[] phrases, float duration, Color color)
    {
        int randomIndex = Random.Range(0, phrases.Length);
        string selectedPhrase = phrases[randomIndex];
        textElement.text = selectedPhrase;
        flashText(textElement, color, duration);
        yield return new WaitForSeconds(duration);

    }

}
