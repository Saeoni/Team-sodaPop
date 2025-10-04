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
    //private bool uiInitialized = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        instance = this;
        InitializeUI();
    }


    void InitializeUI()
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

    private void SetupMenu(string title, Color titleColor, bool showRespawn, string[] phrases = null)
    {
        InitializeUI();

        menuTitle = contentContainer.Q<Label>("Menu_Title");
        menuTitle.text = title;
        menuTitle.style.color = new StyleColor(titleColor);

        resumeButton.style.display = title == "Paused" ? DisplayStyle.Flex : DisplayStyle.None;
        respawnButton.style.display = showRespawn ? DisplayStyle.Flex : DisplayStyle.None;

        contentContainer.style.display = DisplayStyle.Flex;
        contentContainer.AddToClassList("scrim--fadein");

        if (phrases != null && phrases.Length > 0)
        {
            SayRandomFromList(menuTitle, phrases, 5f, titleColor);
        }

        restartButton.Focus();
    }

    public void OpenPauseMenu()
    {
        SetupMenu("Paused", Color.white, false);
        resumeButton.AddToClassList("flash");


    }

    public void OpenLoseMenu()
    {
        SetupMenu("", Color.red, true, youLosePhrases);
        respawnButton.AddToClassList("flash");

    }


    public void OpenWinMenu()
    {
        SetupMenu("", new Color(1f, 0.84f, 0f), false, youWinPhrases);
        restartButton.AddToClassList("flash");

    }

    public void CloseMenu()
    {
        if (contentContainer == null) return;

        RemoveEffect(resumeButton, "flash");
        RemoveEffect(restartButton, "flash");
        RemoveEffect(respawnButton, "flash");
        RemoveEffect(contentContainer, "scrim--fadein");

        contentContainer.style.display = DisplayStyle.None;

    }

    private void OnResumeButtonClicked(ClickEvent evt)
    {
        gamemanager.instance.stateUnpause();

    }
    private void OnRestartButtonClicked(ClickEvent evt)
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        gamemanager.instance.stateUnpause();
        //restartCount += 1;
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
        gamemanager.instance.playerScript.heal(gamemanager.instance.playerScript.HPOrig);
        gamemanager.instance.playerIsDead = false;
        gamemanager.instance.stateUnpause();

    }


    void SayRandomFromList(Label textElement, string[] phrases, float duration, Color color)
    {
        if (phrases == null || phrases.Length == 0) return;
        textElement.text = phrases[Random.Range(0, phrases.Length)];
        textElement.style.color = new StyleColor(color);

    }

    private void RemoveEffect(VisualElement element, string className)
    {

        if (element.ClassListContains(className))
            element.RemoveFromClassList(className);
    }

}
