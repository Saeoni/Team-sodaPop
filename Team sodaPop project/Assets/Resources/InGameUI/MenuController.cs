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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        


    }
     void Start()
    {
        InitializeUI();
        CloseMenu();
    }

    public void OnEnable()
    {
        OpenPauseMenu();
    }

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
        InitializeUI();
        menuTitle = contentContainer.Q<Label>("Menu_Title");
        Color color = new Color(1f, 0f, 0f); // Red color
        menuTitle.text = "";
        StartCoroutine(SayRandomFromList(menuTitle, youLosePhrases, 5f, color));
        

        contentContainer.style.display = DisplayStyle.Flex;
        contentContainer.AddToClassList("scrim--fadein");
        resumeButton.style.display = DisplayStyle.None;
        respawnButton.style.display = DisplayStyle.Flex;
    }

    public void OpenWinMenu()
        {
        InitializeUI();
        menuTitle = contentContainer.Q<Label>("Menu_Title");
        Color color = new Color(1f, 0.84f, 0f); // Gold color
        SayRandomFromList(menuTitle, youWinPhrases, 5f, color).ToString();
        contentContainer.style.display = DisplayStyle.Flex;
        contentContainer.AddToClassList("scrim--fadein");
        resumeButton.style.display = DisplayStyle.None;
        respawnButton.style.display = DisplayStyle.None;
    }
    private void OnResumeButtonClicked(ClickEvent evt)
    {
        gamemanager.instance.stateUnpause();

    }
    private void OnRestartButtonClicked(ClickEvent evt)
    {


        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gamemanager.instance.stateUnpause();
        StartMenuController.instance.restartCount += 1;


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
