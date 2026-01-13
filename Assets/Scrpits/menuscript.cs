using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startMenuUI;   // Drag your Start Menu Panel here
    public GameObject pauseMenuUI;   // Drag your Pause Menu Panel here

    [Header("Maze Reference")]
    public MazeGenerator mazeGenerator; // Drag your MazeGenerator object here

    private bool isPaused = false;

    void Start()
    {
        OpenStartMenu();
    }

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.Escape) && !startMenuUI.activeSelf)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

   
    public void StartGame()
    {
        startMenuUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        
        Time.timeScale = 1f; // Run the game
        isPaused = false;
        SetMouseState(true);
    }

    // Called by "Resume" button on Pause Menu
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        startMenuUI.SetActive(false);
        
        Time.timeScale = 1f;
        isPaused = false;
        SetMouseState(true);
    }

    public void OpenPauseMenu()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze the bot and logic
        isPaused = true;
        SetMouseState(true);
    }

    // Called by "Regenerate" button on Pause Menu
    public void RegenerateAndPlay()
    {
        if (mazeGenerator != null)
        {
            mazeGenerator.Generate(); // Rebuild the maze
            ResumeGame();             // Instantly close menu and play
        }
    }

    // Called by "Back" button to return to Start Screen
    public void OpenStartMenu()
    {
        startMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);
        
        Time.timeScale = 0f; // Freeze game
        isPaused = true;
        SetMouseState(true);
    }

    private void SetMouseState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}