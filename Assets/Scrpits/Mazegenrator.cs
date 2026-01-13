using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation; 

public class MazeGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _mazeParent; 
    [SerializeField] private MazeCell _mazeCellPrefab;
    public NavMeshSurface navSurface;
    public BotController botScript; 
    
   
    public WinPoint winPoint; 

    [Header("Settings")]
    [SerializeField] private int _mazeWidth;
    [SerializeField] private int _mazeDepth;
    
    private MazeCell[,] _mazeGrid;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        
        if (winPoint != null)
        {
            winPoint.ResetGoal();
        }

       
        ClearOldMaze();

        
        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                _mazeGrid[x, z] = Instantiate(_mazeCellPrefab, new Vector3(x, 0, z), Quaternion.identity, _mazeParent);
            }
        }

        GenerateMaze(null, _mazeGrid[0, 0]);

        
        if (navSurface != null) navSurface.BuildNavMesh();

    
        if (botScript != null) botScript.agent.Warp(Vector3.zero);
    }

    private void ClearOldMaze()
    {
        if (_mazeParent == null) return;
        var children = _mazeParent.Cast<Transform>().ToList();
        foreach (var child in children)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);
        MazeCell nextCell;
        do
        {
            nextCell = GetNextUnvisitedCell(currentCell);
            if (nextCell != null) GenerateMaze(currentCell, nextCell);
        } while (nextCell != null);
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell);
        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        int x = Mathf.RoundToInt(currentCell.transform.position.x);
        int z = Mathf.RoundToInt(currentCell.transform.position.z);
        if (x + 1 < _mazeWidth)
         { var c = _mazeGrid[x + 1, z]; 
         if (!c.IsVisited) yield return c; }
        if (x - 1 >= 0) { var c = _mazeGrid[x - 1, z]; if (!c.IsVisited) yield return c; }
        if (z + 1 < _mazeDepth) { var c = _mazeGrid[x, z + 1]; if (!c.IsVisited) yield return c; }
        if (z - 1 >= 0) { var c = _mazeGrid[x, z - 1]; if (!c.IsVisited) yield return c; }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null) return;
        if (previousCell.transform.position.x < currentCell.transform.position.x) { previousCell.ClearRightWall(); currentCell.ClearLeftWall(); }
        else if (previousCell.transform.position.x > currentCell.transform.position.x) { previousCell.ClearLeftWall(); currentCell.ClearRightWall(); }
        else if (previousCell.transform.position.z < currentCell.transform.position.z) { previousCell.ClearFrontWall(); currentCell.ClearBackWall(); }
        else if (previousCell.transform.position.z > currentCell.transform.position.z) { previousCell.ClearBackWall(); currentCell.ClearFrontWall(); }
    }
}