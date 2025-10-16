
using UnityEngine;
using UnityEngine.UIElements;


public class StartMenuController : MonoBehaviour
{

    public static StartMenuController Instance;

    private VisualElement _contentContainer;
    private VisualElement _scrim1;
    private VisualElement _scrim2;
    private VisualElement _title;
    private VisualElement _subtitle;

    private Button _startButton;
    private Button _quitButton;
    public Button Setting;
    //private Button creditsButton;
    //private Button backButton;
    private VisualElement _creditsScreen;
    private VisualElement _mainMenuScreen;
   
    private protected Coroutine Coroutine = null;

    private string _transName;
    private VisualElement _currentElement;

    public bool isShowing;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    { 
        Instance = this;

    }


    public void Start()
    {
        Gamemanager.Instance.StatePause();
        InitializeUI();
        _contentContainer.style.display = DisplayStyle.Flex;
        isShowing = true;
        _startButton.Focus();

    }

   

    private void InitializeUI()
    {

        var root = GetComponent<UIDocument>().rootVisualElement;
        _contentContainer = root.Q<VisualElement>("ContentContainer");
        _scrim1 = root.Q<VisualElement>("Scrim1");
        _scrim2 = root.Q<VisualElement>("Scrim2");

        _startButton = root.Q<Button>("StartButton");
        _quitButton = root.Q<Button>("QuitButton");
        _title = root.Q<VisualElement>("Title");
        _subtitle = root.Q<VisualElement>("Subtitle");
        //creditsButton = root.Q<Button>("CreditsButton");
        //backButton = root.Q<Button>("BackButton");
        //creditsScreen = root.Q<VisualElement>("CreditsScreen");
        //mainMenuScreen = root.Q<VisualElement>("MainMenuScreen");
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
        _quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClicked);
        //creditsButton.clicked += OnCreditsButtonClicked;
        //backButton.clicked += OnBackButtonClicked;
        //creditsScreen.style.display = DisplayStyle.None;
        //mainMenuScreen.style.display = DisplayStyle.Flex;
        

    }

    private void OnStartButtonClicked(ClickEvent evt)
    {

        isShowing = false;
        _contentContainer.style.display = DisplayStyle.None;
        Gamemanager.Instance.StateUnpause();

        enabled = false;


    }

    private static void OnQuitButtonClicked(ClickEvent evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    private void ToggleTransitions(VisualElement element, string transitionName)
    {
        
        if (!Gamemanager.Instance.isPaused)
        {
            return;
        }
        else
        {_transName = transitionName;

            element.ToggleInClassList(_transName);
            element.RegisterCallback<TransitionEndEvent>(ToggleBackTransition);
        }
    }

    private void ToggleBackTransition(TransitionEndEvent evt)
    {
        _currentElement.ToggleInClassList(_transName);
    }
}
