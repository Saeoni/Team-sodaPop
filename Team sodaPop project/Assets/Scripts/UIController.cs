using UnityEngine;
using UnityEngine.UIElements;


public class UIController : MonoBehaviour
{
    public static UIController instance;
    private VisualElement contentContainer;
    
    
    private Button resumeButton;
    private Button restartButton;
    private Button quitButton;
    private Button respawnButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        instance = this;
        InitializeUI();


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

    public void OpenMenu()
    {
        InitializeUI();
        contentContainer.style.display = DisplayStyle.Flex;
        contentContainer.AddToClassList("scrim--fadein");


    }
    public void CloseMenu()
    {
        contentContainer.RemoveFromClassList("scrim--fadein");
        contentContainer.style.display = DisplayStyle.None;

    }
    private void OnResumeButtonClicked(ClickEvent evt)
    {
        gamemanager.instance.stateUnpause();

    }
    private void OnRestartButtonClicked(ClickEvent evt)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        
        gamemanager.instance.stateUnpause();
       
        
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


}
