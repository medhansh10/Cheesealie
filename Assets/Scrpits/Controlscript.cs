using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class BotController : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public GameObject markerPrefab;
    public GameObject loseMenuUI; 
    
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask baitLayer;

    [Header("Settings")]
    public float clickCooldown = 0.5f;
    public float maxPathDistance = 20f; 
    public float arrivalThreshold = 0.5f; 

    private GameObject currentMarker;
    private float nextClickTime = 0f;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // 1. If Time is frozen (Lose Screen is up), stop all logic.
        // This effectively replaces 'hasLost'
        if (Time.timeScale == 0) return;

        // 2. Input handling (Using unscaledTime so pause doesn't break the clock)
        if (Input.GetMouseButtonDown(0) && Time.unscaledTime >= nextClickTime)
        {
            if (!EventSystem.current.IsPointerOverGameObject()) 
            {
                HandleClick();
                nextClickTime = Time.unscaledTime + clickCooldown;
            }
        }

        CheckIfBotReachedBait();
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                if (GetPathLength(path) > maxPathDistance) return; 

                currentMarker = Instantiate(markerPrefab, hit.point, Quaternion.identity);
                currentMarker.layer = Mathf.RoundToInt(Mathf.Log(baitLayer.value, 2));

                agent.isStopped = false;
                agent.SetDestination(hit.point);
            }
        }
    }

    void CheckIfBotReachedBait()
    {
        if (currentMarker != null)
        {
            if (Vector3.Distance(transform.position, currentMarker.transform.position) <= arrivalThreshold)
            {
                TriggerLose();
            }
        }
    }

    void TriggerLose()
    {
        // 1. Immediately destroy the bait so the distance check 
        // stops returning 'true' on the very next frame.
        ClearBait();

        // 2. Show the Screen
        if (loseMenuUI != null) loseMenuUI.SetActive(true);
        
        // 3. Freeze the game
        Time.timeScale = 0f;
    }

    // Call this from your Replay button (GameMenuManager)
    public void ResetBot(Vector3 startPos)
    {
        // Wipe everything
        ClearBait();
        nextClickTime = 0;

        // Move the bot
        agent.enabled = false; 
        transform.position = startPos;
        agent.enabled = true;
        agent.ResetPath();
        
        // Unfreeze time
        Time.timeScale = 1f;
    }

    public void ClearBait()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null; // Important: nullify so distance check stops
        }

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private float GetPathLength(NavMeshPath path)
    {
        float length = 0.0f;
        if (path.corners.Length < 2) return 0f;
        for (int i = 1; i < path.corners.Length; i++)
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        return length;
    }
}