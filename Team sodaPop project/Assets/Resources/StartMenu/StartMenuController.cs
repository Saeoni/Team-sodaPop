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
    private VisualElement title;
    private VisualElement subtitle;

    private Button startButton;
    private Button quitButton;
    //private Button creditsButton;
    //private Button backButton;
    private VisualElement creditsScreen;
    private VisualElement mainMenuScreen;
   
    private Coroutine coroutine = null;

    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        contentContainer = root.Q<VisualElement>("ContentContainer");
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
        instance = this;
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        



    }
    void OnEnable()
    {
        InitializeUI();
    }

    void Start()
    {
        gamemanager.instance.statePause();
        
        coroutine = StartCoroutine(flashSubtitle());
        contentContainer.style.display = DisplayStyle.Flex;
    }
    void OnStartButtonClicked(ClickEvent evt)
    {

        StopCoroutine(coroutine);
        coroutine = null;
        contentContainer.style.display = DisplayStyle.None;
        gamemanager.instance.stateUnpause();
        

    }
   
    void OnQuitButtonClicked(ClickEvent evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    IEnumerator flashSubtitle()
    {
        while (true)
        {
            subtitle.style.display = DisplayStyle.Flex;
            yield return new WaitForSeconds(0.5f);
            subtitle.style.display = DisplayStyle.None;
            yield return new WaitForSeconds(0.5f);
        }
    }

}
