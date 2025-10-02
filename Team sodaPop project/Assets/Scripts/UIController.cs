using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private VisualElement root; 
    private Button resumeButton;
    private Button restartButton;
    private Button quitButton;
    private Button respawnButton;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        resumeButton = root.Q<Button>("ResumeButton");
        restartButton = root.Q<Button>("RestartButton");
        quitButton = root.Q<Button>("QuitButton");
        respawnButton = root.Q<Button>("RespawnButton");
        resumeButton.clicked += () => gamemanager.instance.stateUnpause();
        restartButton.clicked += () => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            gamemanager.instance.stateUnpause();
        };
        quitButton.clicked += () =>
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            };

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
