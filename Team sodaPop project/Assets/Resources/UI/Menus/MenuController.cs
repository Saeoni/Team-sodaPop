using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;



public class MenuController : MonoBehaviour
{
    public enum PauseMenuTabs { Settings, Inventory, Controls }

    public struct MenuTab
    {
        public string name;
        public VisualElement content;
        public MenuTab(string name)
        {
            this.name = name;
            this.content = new VisualElement();
            this.content.name = name + "Content";
        }
    }

    [SerializeField] string[] youLosePhrases;
    [SerializeField] string[] youWinPhrases;


    private VisualElement menuContainer;
    private VisualElement menuPauseContainer;
    private VisualElement menuSettingsContainer;
    private TabView pauseMenuTabs;
    private PauseMenuTabs currentTab;

    private VisualElement menuInventoryContainer;
    private VisualElement menuItemContainer;




    private Label menuTitle;

    public UnityEvent onResumeButtonClicked;
    public UnityEvent onRestartButtonClicked;
    public UnityEvent onQuitButtonClicked;
    public UnityEvent onRespawnButtonClicked;


    private Button resumeButton;
    private Button restartButton;
    private Button quitButton;
    private Button respawnButton;
    public int restartCount = 0;
    private bool uiInitialized = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OnEnable()
    {

        InitializeUI();


    }


    void InitializeUI()
    {

        var root = GetComponent<UIDocument>().rootVisualElement;

        menuPauseContainer = root.Q<VisualElement>("MenuPauseContainer");
        pauseMenuTabs = root.Q<TabView>("PauseMenuTabs");


        if (menuPauseContainer == null) return;
        menuPauseContainer.style.display = DisplayStyle.None;


        AddMenuTab(pauseMenuTabs);
        menuSettingsContainer = root.Q<VisualElement>("MenuSettingsContainer");
        menuSettingsContainer.style.display = DisplayStyle.None;
        menuInventoryContainer = root.Q<VisualElement>("MenuInventoryContainer");
        menuInventoryContainer.style.display = DisplayStyle.None;
        menuItemContainer = root.Q<VisualElement>("MenuItemContainer");
        menuItemContainer.style.display = DisplayStyle.None;

        resumeButton = root.Q<Button>("Button_Resume");
        restartButton = root.Q<Button>("Button_Restart");
        quitButton = root.Q<Button>("Button_Quit");
        respawnButton = root.Q<Button>("Button_Respawn");

        resumeButton.clicked += () => OnResumeButtonClicked();
        restartButton.clicked += () => OnRestartButtonClicked();
        quitButton.clicked += () => OnQuitButtonClicked();
        respawnButton.clicked += () => OnRespawnButtonClicked();

        //uiInitialized = true;
    }

    private void AddMenuTab(TabView tabView)
    {




        foreach (PauseMenuTabs tab in System.Enum.GetValues(typeof(PauseMenuTabs)))
        {

            var tabViewItem = new Tab(tab.ToString());
            var content = menuPauseContainer.Q<VisualElement>($"{tab.ToString()}Container");
            tabViewItem.name = tab.ToString();
            tabViewItem.label = tab.ToString();


            if (content != null)
            {
                tabViewItem.contentContainer.Add(content);
            }

            tabView.Add(tabViewItem);
        }

    }

    private void SetupMenu(string title, Color titleColor, bool showRespawn, string[] phrases = null)
    {
        InitializeUI();

        menuTitle = menuPauseContainer.Q<Label>("Menu_Title");
        menuTitle.text = title;
        menuTitle.style.color = new StyleColor(titleColor);

        resumeButton.style.display = title == "Paused" ? DisplayStyle.Flex : DisplayStyle.None;
        respawnButton.style.display = showRespawn ? DisplayStyle.Flex : DisplayStyle.None;

        menuPauseContainer.style.display = DisplayStyle.Flex;
        menuPauseContainer.AddToClassList("scrim--fadein");

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
        if (menuPauseContainer == null) return;

        RemoveEffect(resumeButton, "flash");
        RemoveEffect(restartButton, "flash");
        RemoveEffect(respawnButton, "flash");
        RemoveEffect(menuPauseContainer, "scrim--fadein");

        menuPauseContainer.style.display = DisplayStyle.None;

    }

    private void OnResumeButtonClicked()
    {
        Debug.Log("Resume Button Clicked");

        GameManager.instance.stateUnpause();
        onResumeButtonClicked?.Invoke();
    }
    private void OnRestartButtonClicked()
    {
        Debug.Log("Restart Button Clicked");

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        GameManager.instance.stateUnpause();
        //restartCount += 1;
        onRestartButtonClicked?.Invoke();

    }
    private void OnQuitButtonClicked()
    {
        Debug.Log("Quit Button Clicked");

        // If we are running in a standalone build of the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

        onQuitButtonClicked?.Invoke();
    }
    private void OnRespawnButtonClicked()
    {
        Debug.Log("Respawn Button Clicked");

        RespawnPlayer();
        onRespawnButtonClicked?.Invoke();
    }

    private void RespawnPlayer()
    {
        //gamemanager.instance.playerScript.spawnPlayer();
        //gamemanager.instance.playerScript.heal(gamemanager.instance.playerScript.HPOrig);
        GameManager.instance.playerIsDead = false;
        GameManager.instance.stateUnpause();

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
