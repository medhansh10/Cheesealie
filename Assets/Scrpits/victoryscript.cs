using UnityEngine;

public class WinPoint : MonoBehaviour
{
    [Header("References")]
    public Transform botTransform;    
    public GameObject victoryMenuUI;  

    [Header("Settings")]
    public float winDistance = 0.8f; 
    private bool hasWon = false;

    void Start()
    {
        
        if(victoryMenuUI != null) 
        victoryMenuUI.SetActive(false);
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
        
        
        if(victoryMenuUI != null) victoryMenuUI.SetActive(true);

        
        Time.timeScale = 0f;

      
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        
    }

    
    public void ResetGoal()
    {
        hasWon = false;
        if(victoryMenuUI != null) victoryMenuUI.SetActive(false);
    }
}