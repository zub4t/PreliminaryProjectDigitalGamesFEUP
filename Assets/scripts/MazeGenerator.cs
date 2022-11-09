using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{
    public int size = 10;
    public float waitingTimeBeforeStart = 1f;
    public bool timeLimited = true;
    public float timeIteration = 0.1f;
    public int stepIteration = 10;
    public bool generate = false;
    public GameObject[] enimies;
    public GameObject Trophy;
    [SerializeField] private GameObject _PlayerBundle;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private GameObject _coin;
    [Range(0, 100)]
    [SerializeField] private int _maxEnemies;
    [Range(0, 200)]
    [SerializeField] private int _maxCoins;

    public static Cell[] cell;
    private float _wallSize;
    private DisjointSet _sets;
    private List<GameObject> navMeshElements = new List<GameObject>();
   

    private void Start()
    {
        if (generate)
        {
            generate = false;
            cell = new Cell[size * size];
            SpawnEntireGrid(size);
            StartCoroutine(RanMaze());
        }
    }


    private void SpawnEntireGrid(int size)
    {
        //deleting all walls in order to generate a new maze
        var prefabs = (GameObject.FindGameObjectsWithTag("prefabs"));
        foreach (GameObject prefab in prefabs) GameObject.Destroy(prefab);


        //spawning walls
        _wallSize = _wallPrefab.transform.localScale.y;
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                var positionCell = new Vector3(x * _wallSize, 0, z * _wallSize);
                Cell newCell = new Cell(x, z, size, positionCell);
                cell[x * size + z] = newCell;

                Quaternion wallRotation = Quaternion.Euler(0f, 90f, 0f);
                var positionWall = positionCell + new Vector3(_wallSize / 2f, 0f, 0f);
                var newWall = Instantiate(_wallPrefab, positionWall, wallRotation);
                newCell.AddWall(1, newWall);

                positionWall = positionCell + new Vector3(0f, 0f, _wallSize / 2f);
                newWall = Instantiate(_wallPrefab, positionWall, Quaternion.identity);
                newCell.AddWall(0, newWall);

                if (x == 0)
                {
                    Quaternion wallRotated = Quaternion.Euler(0f, 90f, 0f);
                    positionWall = positionCell + new Vector3(-_wallSize / 2f, 0f, 0f);
                    newWall = Instantiate(_wallPrefab, positionWall, wallRotated);
                    newCell.AddWall(3, newWall);
                }
                else
                {
                    newCell.AddWall(3, cell[(x - 1) * size + z].GetWall(1));
                }

                if (z == 0)
                {
                    positionWall = positionCell + new Vector3(0f, 0f, -_wallSize / 2f);
                    newWall = Instantiate(_wallPrefab, positionWall, Quaternion.identity);
                    newCell.AddWall(2, newWall);
                }
                else
                {
                    newCell.AddWall(2, cell[x * size + z - 1].GetWall(0));
                }
            }
        }

    }



    private IEnumerator RanMaze()
    {
        _sets = new DisjointSet(size * size);
        for (int i = 0; i < size * size; i++) _sets.MakeSet(i);
        yield return new WaitForSeconds(waitingTimeBeforeStart);
        var source = 0; //low left corner
        var target = size * size - 1; //top right corner
        var iterations = 0; //only for 'visualizing' purposes 
        while (_sets.FindSet(source) != _sets.FindSet(target))
        {
            if (size <= 5)
            {
                // _sets.print();
            }

            var randomIndexCell = Random.Range(0, size * size);
            var randomCell = cell[randomIndexCell];
            var randomWall = Random.Range(0, 4);
            if (randomCell.GetWall(randomWall) == null) continue;

            var indexNeighbour = -1;
            if (!IsPerimetralWall(randomIndexCell, randomWall))
            {
                if (randomWall == 0) indexNeighbour = randomCell.GetIndex() + 1;
                if (randomWall == 1) indexNeighbour = randomCell.GetIndex() + size;
                if (randomWall == 2) indexNeighbour = randomCell.GetIndex() - 1;
                if (randomWall == 3) indexNeighbour = randomCell.GetIndex() - size;
            }

            if (indexNeighbour >= 0 && indexNeighbour < size * size)
            {
                if (_sets.FindSet(indexNeighbour) != _sets.FindSet(randomIndexCell)) //not reachable
                {
                    randomCell.DestroyWall(randomWall);
                    _sets.UnionSet(indexNeighbour, randomIndexCell);
                }
            }

            iterations++;
            if (timeLimited && iterations % stepIteration == 0) yield return new WaitForSeconds(timeIteration);
        }

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(Dfs());


        SetUPenemies();
        SetUCoins();
        _PlayerBundle.active = true;


        GameObject.FindGameObjectWithTag("Ground").GetComponent<NavMeshSurface>().BuildNavMesh();
        Instantiate(Trophy, cell[cell.Length - 1].GetWorldPosition() + Vector3.up * 2, Quaternion.identity);


    }
    private void SetUPenemies()
    {

        for (var i = 0; i < _maxEnemies; i++)
        {

            var randomIndexCell = Random.Range(0, size * size);
            var randomCell = cell[randomIndexCell];
            Instantiate(enimies[Random.Range(0, enimies.Length)], randomCell.GetWorldPosition() + (Vector3.up * 2), Quaternion.identity);
        }
    }

    private void SetUCoins()
    {

        for (var i = 0; i < _maxCoins; i++)
        {

            var randomIndexCell = Random.Range(0, size * size);
            var randomCell = cell[randomIndexCell];
            Instantiate(_coin, randomCell.GetWorldPosition() + (Vector3.up * 2), Quaternion.identity);
        }
    }

    private bool IsPerimetralWall(int cellIndex, int wallIndex)
    {
        if (cellIndex % size == 0 && wallIndex == 2) return true;
        if (cellIndex % size == size - 1 && wallIndex == 0) return true;
        if ((cellIndex / size) == size - 1 && wallIndex == 1) return true;
        return (cellIndex / size) == 0 && wallIndex == 3;
    }


    private IEnumerator Dfs()
    {
        Stack<Cell> stack = new Stack<Cell>();
        var predecessors = new int[size * size];
        for (int i = 0; i <= size * size - 1; i++) predecessors[i] = -1;

        var source = cell[0];
        stack.Push(source);
        Cell node = source;

        while (predecessors[size * size - 1] == -1)
        {
            var indexNode = node.GetIndex();
            var z = indexNode % size;
            var x = (indexNode - z) / size;
            if (x > 0 && predecessors[indexNode - size] == -1 && _sets.FindSet(indexNode - size) == _sets.FindSet(0))
            {
                if (node.GetWall(3) == null)
                {
                    predecessors[indexNode - size] = indexNode;
                    stack.Push(cell[indexNode - size]);
                }
            }

            if (x < size - 1)
            {
                if (predecessors[indexNode + size] == -1 && _sets.FindSet(indexNode + size) == _sets.FindSet(0))
                {
                    if (node.GetWall(1) == null)
                    {
                        predecessors[indexNode + size] = indexNode;
                        stack.Push(cell[indexNode + size]);
                    }
                }
            }

            if (z < size - 1)
            {
                if (predecessors[indexNode + 1] == -1 && _sets.FindSet(indexNode + 1) == _sets.FindSet(0))
                {
                    if (node.GetWall(0) == null)
                    {
                        predecessors[indexNode + 1] = indexNode;
                        stack.Push(cell[indexNode + 1]);
                    }
                }
            }

            if (z > 0)
            {
                if (predecessors[indexNode - 1] == -1 && _sets.FindSet(indexNode - 1) == _sets.FindSet(0))
                {
                    if (node.GetWall(2) == null)
                    {
                        predecessors[indexNode - 1] = indexNode;
                        stack.Push(cell[indexNode - 1]);
                    }
                }
            }
            node = stack.Pop();
        }

        //visualizing path 
        var backtrackerIndex = size * size - 1;
        while (backtrackerIndex != 0) //0 is index of the source
        {
            Instantiate(_cubePrefab, cell[backtrackerIndex].GetWorldPosition()+Vector3.up, Quaternion.identity);
            backtrackerIndex = predecessors[backtrackerIndex];
        }
        Instantiate(_cubePrefab, cell[backtrackerIndex].GetWorldPosition(), Quaternion.identity);
        yield return null;
    }



}