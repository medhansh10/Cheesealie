using UnityEngine;

public class WinPoint : MonoBehaviour
{
    [Header("References")]
    public Transform botTransform;    // Drag your Bot here
    public GameObject victoryMenuUI;  // Drag your Victory Panel here

    [Header("Settings")]
    public float winDistance = 0.8f; 
    private bool hasWon = false;

    void Start()
    {
        // Ensure victory screen is hidden at the start
        if(victoryMenuUI != null) victoryMenuUI.SetActive(false);
    }

    void Update()
    {
        if (hasWon || botTransform == null) return;

        float distance = Vector3.Distance(transform.position, botTransform.position);

        if (distance <= winDistance)
        {
            Win();
        }
    }

    void Win()
    {
        hasWon = true;
        
        // 1. Show the Victory Screen
        if(victoryMenuUI != null) victoryMenuUI.SetActive(true);

        // 2. Freeze the game
        Time.timeScale = 0f;

        // 3. Enable Mouse for clicking buttons
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log(">>> Victory Screen Triggered!");
    }

    // Call this from MazeGenerator when clicking "Regenerate"
    public void ResetGoal()
    {
        hasWon = false;
        if(victoryMenuUI != null) victoryMenuUI.SetActive(false);
    }
}