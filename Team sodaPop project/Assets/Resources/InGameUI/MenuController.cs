using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;



public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [SerializeField] string[] youLosePhrases;
    [SerializeField] string[] youWinPhrases;
    
    private VisualElement contentContainer;
    
    
    
    private Label menuTitle;
 
    private Button resumeButton;
    private Button restartButton;
    private Button quitButton;
    private Button respawnButton;
    public int restartCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        
    }

    void OnEnable()
    {
            InitializeUI();
           
    }

    //void Update()
    //{
    //     if (!gamemanager.instance.isPaused && contentContainer.style.display == DisplayStyle.Flex)
    //        CloseMenu();
        
    //}


    private void InitializeUI()
    {
    var root = GetComponent<UIDocument>().rootVisualElement;
    contentContainer = root.Q<VisualElement>("Content_Container");
    contentContainer.style.display = DisplayStyle.None;
    resumeButton = root.Q<Button>("Button_Resume");
    restartButton = root.Q<Button>("Button_Restart");
    quitButton = root.Q<Button>("Button_Quit");
    respawnButton = root.Q<Button>("Button_Respawn");
        
        resumeButton.RegisterCallback<ClickEvent>(OnResumeButtonClicked);
    restartButton.RegisterCallback<ClickEvent>(OnRestartButtonClicked);
    quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClicked);
    respawnButton.RegisterCallback<ClickEvent>(OnRespawnButtonClicked);
        


    }

    public void OpenPauseMenu()
    {
        InitializeUI();
       
        menuTitle = contentContainer.Q<Label>("Menu_Title");
        menuTitle.text = "Paused";
        HideRespawnButton();
        contentContainer.style.display = DisplayStyle.Flex;
        
        contentContainer.AddToClassList("scrim--fadein");
        //resumeButton.Focus();



    }
    public void CloseMenu()
    {
        if (contentContainer == null) return;

        if (contentContainer.ClassListContains("scrim--fadein"))
        contentContainer.RemoveFromClassList("scrim--fadein");
        contentContainer.style.display = DisplayStyle.None;

    }

    public void ShowRespawnButton()
        {
        if (respawnButton == null) return;
        respawnButton.style.display = DisplayStyle.Flex;
    }
    public void HideRespawnButton()
        {
        if (respawnButton == null) return;
        respawnButton.style.display = DisplayStyle.None;
    }

    public void OpenLoseMenu()
        {
        contentContainer.AddToClassList("scrim--fadein");
        menuTitle = contentContainer.Q<Label>("Menu_Title");
        Color color = new Color(1f, 0f, 0f); // Red color
        menuTitle.text = "";
        SayRandomFromList(menuTitle, youLosePhrases, 5f, color);

        resumeButton.style.display = DisplayStyle.None;
        respawnButton.style.display = DisplayStyle.Flex;
        //settingsButton
        contentContainer.style.display = DisplayStyle.Flex;

        StartCoroutine(FlashButtonText(respawnButton, color, 5f));
        
        // restartButton.Focus();

    }

    public void OpenWinMenu()
        {
        
        menuTitle = contentContainer.Q<Label>("Menu_Title");
        Color color = new Color(1f, 0.84f, 0f); // Gold color
        
        contentContainer.style.display = DisplayStyle.Flex;
        contentContainer.AddToClassList("scrim--fadein");
        resumeButton.style.display = DisplayStyle.None;
        respawnButton.style.display = DisplayStyle.None;
        SayRandomFromList(menuTitle, youWinPhrases, 5f, color);
        


        //mainMenuButtonFocus();
    }
    private void OnResumeButtonClicked(ClickEvent evt)
    {
        gamemanager.instance.stateUnpause();

    }
    private void OnRestartButtonClicked(ClickEvent evt)
    {


        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gamemanager.instance.stateUnpause();
        restartCount += 1;


    }
    private void OnQuitButtonClicked(ClickEvent evt)
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    private void OnRespawnButtonClicked(ClickEvent evt)
    {
        gamemanager.instance.playerScript.spawnPlayer();
        gamemanager.instance.stateUnpause();
        
    }

    IEnumerator FlashText(Label textElement, Color flashColor, float duration)
    {
        Color originalColor = textElement.style.color.value;
        textElement.style.color = flashColor;

        while (true)
        {
            yield return new WaitForSeconds(duration);
            textElement.style.color = originalColor;
        }
    }

    IEnumerator FlashButtonText(Button button, Color flashColor, float duration)
    {
        Color originalColor = button.style.color.value;
        button.style.color = flashColor;

        while (true)
        {
            
            yield return new WaitForSeconds(duration);
            button.style.color = originalColor;
        }
    }

    void SayRandomFromList(Label textElement, string[] phrases, float duration, Color color)
    {
        int randomIndex = Random.Range(0, phrases.Length);
        string selectedPhrase = phrases[randomIndex];
        textElement.text = selectedPhrase;
        StartCoroutine(FlashText(textElement, color, duration));
        
    }



}
