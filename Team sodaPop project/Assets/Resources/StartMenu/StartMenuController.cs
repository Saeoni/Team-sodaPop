using System;
using System.Collections;
//using Unity.Engine.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEngine.EventSystems;




public class StartMenuController : MonoBehaviour
{

    public static StartMenuController instance;

    private VisualElement contentContainer;
    private VisualElement scrim1;
    private VisualElement scrim2;
    private VisualElement title;
    private VisualElement subtitle;

    private Button startButton;
    private Button quitButton;
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
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        InitializeUI();
        contentContainer.style.display = DisplayStyle.Flex;
        isShowing = true;



    }
   

    void Start()
    {
        gamemanager.instance.statePause();

        // bring focus to first button
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


        gamemanager.instance.stateUnpause();
        contentContainer.style.display = DisplayStyle.None;
        
        

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
        
        if (gamemanager.instance.isPaused == false)
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
