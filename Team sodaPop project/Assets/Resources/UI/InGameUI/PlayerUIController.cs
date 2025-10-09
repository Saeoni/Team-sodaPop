
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;



public class PlayerUIController : MonoBehaviour
{
    //public static HUDController instance;


    [SerializeField] string[] errorPhrases;
    [SerializeField] string[] warningMessages;

    [SerializeField] private GameObject reticle;

    private VisualElement contentContainer;
    private VisualElement minimap;

    private Label enemiesTitle;
    private Label enemyCountText;
    private int enemyCount;


    public PlayerController playerController;
    private Label collectedModulesTitle;
    private Label collectedModulesCount;
    private Label gunLabel;

    private VisualElement minimapBorder;
    private VisualElement minimapIcon;
    private VisualElement healthContanter;
    private VisualElement healthFill;

    private Label stealthTimer;
    private Label stealthTimerText;
    private Label gameTimerText;

    //private VisualElement bossHPBar;
    //private Image bossHPFill;

    private Label ammoCur;
    private Label ammoMax;
    private PlayerController player;
    int gameGoalCount;
    int gameTimerMinute;
    float gameTimerSecond;
    float stealthTimeLeft;


    public float timeElapsed;
    public bool healthChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created




    void OnEnable()
    {

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        contentContainer = root.Q<VisualElement>("Player_UI");

        healthContanter = root.Q<VisualElement>("HealthBar_Container");
        healthFill = root.Q<VisualElement>("HealthBar_Fill");
        enemyCountText = root.Q<Label>("Enemy_Count");
        enemiesTitle = root.Q<Label>("Enemies_Title");


        stealthTimer = root.Q<Label>("StealthTimer_Label");
        stealthTimerText = root.Q<Label>("StealthTimer_Text");
        stealthTimerText.text = "";
        gameTimerText = root.Q<Label>("Game_Timer");
        gameTimerText.text = "00:00";
        ammoCur = root.Q<Label>("AmmoCurr_Text");
        ammoMax = root.Q<Label>("AmmoMax_Text");


        //bossHPFill = root.Q<Image>("Boss_HP_Fill");
        //bossHPBar.style.display = DisplayStyle.None; // Hide boss HP bar initially





        collectedModulesTitle = root.Q<Label>("Collected_Modules_Title");
        collectedModulesCount = root.Q<Label>("Collected_Modules_Count");
        collectedModulesCount.text = "0";
        gunLabel = root.Q<Label>("Gun_Label");
        //gunLabel.text = player.CurrentGun.gunModel.name;
        minimap = root.Q<VisualElement>("Minimap_Container");


        //minimapBorder = root.Q<VisualElement>("Minimap_Border");
        //minimapBorder.style.display = DisplayStyle.None; // Hide minimap border initially
        //minimapIcon = root.Q<VisualElement>("Minimap_Icon");
        //minimapIcon.style.display = DisplayStyle.None; // Hide minimap icon initially


        gameTimerMinute = 0;
        gameTimerSecond = 0;
        timeElapsed = 0;
        contentContainer.style.display = DisplayStyle.Flex;
        minimap.style.display = DisplayStyle.None; // Hide minimap initially
        HideUI();

    }



    void Start()
    {
        // Example of starting a stealth timer for 5 seconds
        //StealthTimer(5f);
        player = GameManager.instance.playerScript;

        ShowUI();

    }

    public void ShowUI()
    {
        contentContainer.style.display = DisplayStyle.Flex;
        UpdateHealthBar();
    }

    public void HideUI()
    {
        contentContainer.style.display = DisplayStyle.None;
    }
    // Update is called once per frame
    void Update()
    {
        // Check if collected modules is empty and hide minimap if so

        UpdatePlayerUI();





    }
    public void UpdatePlayerUI()
    {
        UpdateGameTimer();
        UpdateHealthBar();
        if (reticle != null)
        {
            if (player.GetComponent<PlayerData>().gunStats.Count > 0 && player.GetComponent<PlayerData>().AmmoCount > 0)
            {
                reticle.SetActive(true);
            }
            else
            {
                reticle.SetActive(false);
            }
        }
        //if (GameManager.instance.boss != null)
        //{
        //    bossHPBar.style.display = DisplayStyle.Flex;
        //    float bossHealthPercent = GameManager.instance.boss.CurrentHealth / GameManager.instance.boss.MaxHealth;
        //    bossHPFill.fillAmount = bossHealthPercent;
        //}
        //else
        //{
        //    bossHPBar.style.display = DisplayStyle.None;
        //}




        //UpdateEnemyCount(0);
        //UpdateCollectedModules(0);
        //UpdateGunLabel(player.CurrentGun.gunModel.name);
        //UpdateAmmoCount(player.CurrentGun.ammoCur, player.CurrentGun.ammoMax);
    }

    public void UpdateEnemyCount(int amount)
    {
        player.GetComponent<PlayerData>().CurrentEnemyCount += amount;

    }
    public void UpdateCollectedModules(int amount)
    {
        player.GetComponent<PlayerData>().CollectedModules += amount;
        collectedModulesCount.text = player.GetComponent<PlayerData>().CollectedModules.ToString();
        if (player.GetComponent<PlayerData>().CollectedModules > 0)
        {
            minimap.style.display = DisplayStyle.Flex; // Show minimap when at least one module is collected
        }
        else
        {
            minimap.style.display = DisplayStyle.None; // Hide minimap if no modules are collected
        }
    }
    public void UpdateGunLabel(string gunName)
    {
        gunLabel.text = gunName;
    }
    public void UpdateKeyCount()
    {
        player.GetComponent<PlayerData>().KeysCollected++;
        //enemiesTitle.text = "Keys Collected";
        //enemyCountText.text = player.KeysCollected.ToString();
    }
    public void UpdateAmmoCount(int current, int max)
    {
        ammoCur.text = current.ToString();
        ammoMax.text = max.ToString();
        //if (current == 0)
        //{
        //    StartCoroutine(SayRandomFromList(enemyCountText, errorPhrases, 2f, Color.red));
        //}
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
        gameTimerText.text = gameTimerMinute.ToString("F0") + ":" + displaySecond.ToString("F0");
    }

    public void UpdateHealthBar()
    {
        if (!healthChanged && player.IsFullyHealed())
        {
            return;
        }
        if (!healthChanged)
            return;

        float healthPercent = player.GetComponent<PlayerData>().CurrentHealth / player.GetComponent<PlayerData>().MaxHealth;

        healthFill.style.height = new Length(healthPercent * 100, LengthUnit.Percent);



        if (healthPercent <= 0.3f)
        {

            //StartCoroutine(SayRandomFromList(enemyCountText, warningMessages, 2f, Color.yellow));


            healthFill.style.backgroundColor = Color.red;
            if (player.GetComponent<PlayerData>().CurrentHealth <= 0)
            {
                healthFill.style.height = new StyleLength(new Length(0, LengthUnit.Percent));
                //StartCoroutine(SayRandomFromList(enemyCountText, errorPhrases, 2f, Color.red));
            }
            healthChanged = false;
        }
        else
        {
            healthFill.style.backgroundColor = Color.Lerp(Color.red, Color.green, healthPercent);
            healthChanged = false;
        }

    }

    public void StealthTimer(float length)
    {
        StartCoroutine(StealthCountdown(length));

    }

    private IEnumerator StealthCountdown(float length)
    {
        GameManager.instance.isStealthed = true;

        float countDown = length;
        stealthTimerText.text = countDown.ToString("F0");
        while (countDown > 0)
        {
            stealthTimerText.text = "Invisible: " + Mathf.CeilToInt(countDown) + "s";

            countDown -= Time.deltaTime;

            yield return null;
        }
        GameManager.instance.isStealthed = false;
        stealthTimerText.text = "";


        stealthTimer.text = "";

    }

    IEnumerator FlashText(Label textElement, Color flashColor, float duration)
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
        StartCoroutine(FlashText(textElement, color, duration));
        yield return null;

    }
    IEnumerator ShootCamAndScope()
    {
        float originalFOV = playerController.GetLensView();
        while (GameManager.instance.playerScript.isShooting)
            player.GetComponent<PlayerData>().ScopeZoomScale = 125f;
        yield return new WaitForSeconds(0.1f);
        player.GetComponent<PlayerData>().ScopeZoomScale = 0f;


    }

}
