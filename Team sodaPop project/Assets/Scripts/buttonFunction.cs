using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public string firstLevel;

    public GameObject optionsMenu;

    private float timeScaleOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void resume()
    {
        Gamemanager.Instance.StateUnpause();
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Gamemanager.Instance.StateUnpause();
    }

    public void quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void respawn()
    {
        Gamemanager.Instance.playerScript.spawnPlayer();
        Gamemanager.Instance.StateUnpause();
    }

    public void Launch()
    {
        SceneManager.LoadScene(firstLevel);
    }

    public void Options()
    {
        
        gamemanager.instance.stateUnpause();
        timeScaleOrig = Time.timeScale;
        Time.timeScale = 0;
        Cursor.visible = true;
         Cursor.lockState = CursorLockMode.None;
        optionsMenu.SetActive(true);

    }

    public void CloseOptions()
    {
        
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        optionsMenu.SetActive(false);
    }
}