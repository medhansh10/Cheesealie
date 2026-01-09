using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public GameObject markerPrefab;
    public GameObject loseMenuUI;    // Drag your Lose Panel here
    
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask baitLayer;

    [Header("Settings")]
    public float clickCooldown = 0.5f;
    public float maxPathDistance = 20f; 
    public float arrivalThreshold = 0.5f; 

    private GameObject currentMarker;
    private float nextClickTime = 0f;
    private bool hasLost = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        // Ensure lose screen is hidden at start
        if (loseMenuUI != null) loseMenuUI.SetActive(false);
    }

    void Update()
    {
        // If the player has already lost, stop processing input and movement
        if (hasLost) return;

        // 1. INPUT HANDLING (Bait Placement)
        bool clickDetected = false;
        #if ENABLE_INPUT_SYSTEM
            clickDetected = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
        #else
            clickDetected = Input.GetMouseButtonDown(0);
        #endif

        if (clickDetected && Time.time >= nextClickTime)
        {
            HandleClick();
            nextClickTime = Time.time + clickCooldown;
        }

        // 2. POSITION CHECK
        CheckIfBotReachedBait();
    }

    void HandleClick()
    {
        Vector3 mousePos;
        #if ENABLE_INPUT_SYSTEM
            mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        #else
            mousePos = Input.mousePosition;
        #endif

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, baitLayer))
        {
            if (currentMarker != null && hit.collider.gameObject == currentMarker)
            {
                ClearBait();
                return;
            }
        }

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            if (currentMarker != null) return;

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.point, path))
            {
                float pathLength = GetPathLength(path);

                if (pathLength > maxPathDistance) return; 

                currentMarker = Instantiate(markerPrefab, hit.point, Quaternion.identity);
                int layerIndex = (int)Mathf.Log(baitLayer.value, 2);
                currentMarker.layer = layerIndex;

                agent.isStopped = false;
                agent.SetDestination(hit.point);
            }
        }
    }

    void CheckIfBotReachedBait()
    {
        if (currentMarker != null)
        {
            float distance = Vector3.Distance(transform.position, currentMarker.transform.position);

            if (distance <= arrivalThreshold)
            {
                // Instead of just clearing the bait, we trigger the Lose Screen
                TriggerLose();
            }
        }
    }

    void TriggerLose()
    {
        hasLost = true;
        Debug.Log(">>> OUTPUT: GAME OVER! The Bot reached the bait.");

        // 1. Stop the bot
        ClearBait();

        // 2. Show the Lose UI
        if (loseMenuUI != null) loseMenuUI.SetActive(true);

        // 3. Freeze time and show mouse
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResetBot(Vector3 startPos)
    {
        hasLost = false;
        if (loseMenuUI != null) loseMenuUI.SetActive(false);
        
        ClearBait();
        agent.Warp(startPos);
        Time.timeScale = 1f;
    }

    void ClearBait()
    {
        if (currentMarker != null) Destroy(currentMarker);
        currentMarker = null;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private float GetPathLength(NavMeshPath path)
    {
        float length = 0.0f;
        if (path.corners.Length < 2) return length;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }
}