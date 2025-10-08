using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;


public class StartMenuController : MonoBehaviour
{

    //public static StartMenuController instance;



    //public CinemachineCamera lookAtCamera;
    //public GameObject[] lookAtObjects;
    [Range(.01f, 2f), SerializeField] float timeSpeed = 1;
    [SerializeField] private string title;
    [SerializeField] private string subtitle;
    private VisualElement contentContainer;
    private VisualElement scrim1;
    private VisualElement scrim2;

    private Label titleText;
    private Label subtitleText;


    public UnityEvent onStartButtonClicked;
    public UnityEvent onQuitButtonClicked;
    public UnityEvent onMainMenuButtonClicked;
    public UnityEvent onSettingsButtonClicked;

    public Button startButton;
    public Button quitButton;
    public Button mainMenuButton;
    public Button settingsButton;




    //private Button creditsButton;

    //private VisualElement creditsScreen;
    //private VisualElement mainMenuScreen;
    private Button backButton;
    private Button exitButton;

    private string transName;
    private VisualElement currentElement;

    public bool isShowing;



    void Start()
    {
        InitializeUI();
        Debug.Log("Start Button Is Focused");
        Time.timeScale = timeSpeed;
        // gamemanager.instance.statePause();
        isShowing = true;


        contentContainer.style.display = DisplayStyle.Flex;

        scrim1.AddToClassList("scrim1--smokey");
        scrim2.AddToClassList("scrim2--smokey");

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
        mainMenuButton = root.Q<Button>("MenuButton");
        titleText = root.Q<Label>("Title");
        subtitleText = root.Q<Label>("Subtitle");
        //creditsButton = root.Q<Button>("CreditsButton");

        //creditsScreen = root.Q<VisualElement>("CreditsScreen");
        //mainMenuScreen = root.Q<VisualElement>("MainMenuScreen");
        startButton.clicked += () => OnStartButtonClicked();
        quitButton.clicked += () => OnQuitButtonClicked();
        mainMenuButton.clicked += () => OnMainMenuButtonClicked();
        //creditsButton.clicked += () => OnCreditsButtonClicked();
        titleText.text = title;
        subtitleText.text = subtitle;


        //mainMenuButton.clicked += () => OnMainMenuButtonClicked();
        //settingsButton.clicked += () => OnSettingsButtonClicked();
        //startButton.RegisterCallback<PointerEnterEvent>(ev => startButton.Hover());
        //quitButton.RegisterCallback<PointerEnterEvent>(ev => quitButton.Focus());



        Debug.Log("Start Menu UI Initialized");

    }


    void OnStartButtonClicked()
    {

        isShowing = false;
        contentContainer.style.display = DisplayStyle.None;
        GameManager.instance.stateUnpause();

        // Deactivate the start menu UI
        //  gameObject.SetActive(false);

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
    //void OnSettingsButtonClicked()
    //{
    //    onSettingsButtonClicked?.Invoke();
    //}




    void toggleTransitions(VisualElement element, string transitionName)
    {

        if (GameManager.instance.isPaused == false)
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
