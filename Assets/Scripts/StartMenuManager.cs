using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    public UnityEngine.UI.Button startButton;
    public UnityEngine.UI.Button quitButton;

    void Start()
    {
        // setup button listeners if buttons are assigned
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void Update()
    {
        // also allow space/enter to start
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
        
        // escape to quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    public void StartGame()
    {
        // load the main game scene (change "Main" to whatever your game scene is called)
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Application.Quit();
        
        // in editor, just stop playing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

