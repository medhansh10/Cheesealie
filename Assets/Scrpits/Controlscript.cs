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
        
        if (Time.timeScale == 0) return;

        
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
                if (GetPathLength(path) > maxPathDistance) 
                return; 

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
        
        ClearBait();

        
        if (loseMenuUI != null) loseMenuUI.SetActive(true);
        
  
        Time.timeScale = 0f;
    }

  
    public void ResetBot(Vector3 startPos)
    {
       
        ClearBait();
        nextClickTime = 0;

       
        agent.enabled = false; 
        transform.position = startPos;
        agent.enabled = true;
        agent.ResetPath();
        
       
        Time.timeScale = 1f;
    }

    public void ClearBait()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null; 
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