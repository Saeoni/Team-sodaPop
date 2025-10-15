
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;




public class StartMenuController : MonoBehaviour
{

    public static StartMenuController instance;

    private VisualElement contentContainer;
    private VisualElement scrim1;
    private VisualElement scrim2;
    private VisualElement title;
    private VisualElement subtitle;

    public Button startButton;
    public Button quitButton;
    public Button setting;
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
        Gamemanager.Instance.StatePause();
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
        //backButton = root.Q<Button>("BackButton");
        //creditsScreen = root.Q<VisualElement>("CreditsScreen");
        //mainMenuScreen = root.Q<VisualElement>("MainMenuScreen");
        startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
        quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClicked);
        //creditsButton.clicked += OnCreditsButtonClicked;
        //backButton.clicked += OnBackButtonClicked;
        //creditsScreen.style.display = DisplayStyle.None;
        //mainMenuScreen.style.display = DisplayStyle.Flex;
        

    }

    void OnStartButtonClicked(ClickEvent evt)
    {

        isShowing = false;
        contentContainer.style.display = DisplayStyle.None;
        Gamemanager.Instance.StateUnpause();

        enabled = false;


    }
   
    void OnQuitButtonClicked(ClickEvent evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    void toggleTransitions(VisualElement element, string transitionName)
    {
        
        if (Gamemanager.Instance.isPaused == false)
        {
            return;
        }
        else
            {transName = transitionName;

        element.ToggleInClassList(transName);
            element.RegisterCallback<TransitionEndEvent>(ToggleBackTransition);
        }
    }

    void ToggleBackTransition(TransitionEndEvent evt)
    {
        currentElement.ToggleInClassList(transName);
    }
}
