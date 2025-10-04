
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;





public class StartMenuController : MonoBehaviour
{

    public static StartMenuController instance;



    public GameObject lookAtObject;
    [Range(.01f, 2f), SerializeField] float timeSpeed = 1;
    private VisualElement contentContainer;
    private VisualElement scrim1;
    private VisualElement scrim2;
    private VisualElement title;
    private VisualElement subtitle;


    public UnityEvent onStartButtonClicked;
    public UnityEvent onQuitButtonClicked;
    public UnityEvent onMainMenuButtonClicked;
    public UnityEvent onSettingsButtonClicked;
    public Button startButton;
    public Button quitButton;
    public Button mainMenuButton;
    public Button settingsButton;




    //private Button creditsButton;
    //private Button backButton;
    private VisualElement creditsScreen;
    private VisualElement mainMenuScreen;

    private Coroutine coroutine = null;

    private string transName;
    private VisualElement currentElement;

    public bool isShowing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;


    }



    void Start()
    {
        gamemanager.instance.statePause();
        PlayInBackground(lookAtObject);

        InitializeUI();
        contentContainer.style.display = DisplayStyle.Flex;
        isShowing = true;
        startButton.Focus();

    }



    private void InitializeUI()
    {

        var root = GetComponent<UIDocument>().rootVisualElement;
        contentContainer = root.Q<VisualElement>("ContentContainer");
        scrim1 = root.Q<VisualElement>("Scrim1");
        scrim2 = root.Q<VisualElement>("Scrim2");

        startButton = root.Q<Button>("StartButton");
        quitButton = root.Q<Button>("QuitButton");
        title = root.Q<VisualElement>("Title");
        subtitle = root.Q<VisualElement>("Subtitle");
        //creditsButton = root.Q<Button>("CreditsButton");

        //creditsScreen = root.Q<VisualElement>("CreditsScreen");
        //mainMenuScreen = root.Q<VisualElement>("MainMenuScreen");
        startButton.clicked += () => OnStartButtonClicked();
        quitButton.clicked += () => OnQuitButtonClicked();
        mainMenuButton.clicked += () => OnMainMenuButtonClicked();
        settingsButton.clicked += () => OnSettingsButtonClicked();


    }
    void PlayInBackground(GameObject lookAt)
    {
        Time.timeScale = timeSpeed;
        // This function can be used to play background music if needed
    }

    void OnStartButtonClicked()
    {

        isShowing = false;
        contentContainer.style.display = DisplayStyle.None;
        gamemanager.instance.stateUnpause();

        // Deactivate the start menu UI
        gameObject.SetActive(false);

        onStartButtonClicked?.Invoke();
    }

    void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        onQuitButtonClicked?.Invoke();
    }
    void OnMainMenuButtonClicked()
    {
        onMainMenuButtonClicked?.Invoke();
    }
    void OnSettingsButtonClicked()
    {
        onSettingsButtonClicked?.Invoke();
    }



    void toggleTransitions(VisualElement element, string transitionName)
    {

        if (gamemanager.instance.isPaused == false)
        {
            return;
        }
        else
        {
            transName = transitionName;

            element.ToggleInClassList(transName);
            element.RegisterCallback<TransitionEndEvent>(ToggleBackTransition);
        }
    }

    void ToggleBackTransition(TransitionEndEvent evt)
    {
        currentElement.ToggleInClassList(transName);
    }
}
