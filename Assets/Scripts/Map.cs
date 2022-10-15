using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    [SerializeField]
    Material ground;
    [SerializeField]
    Material wall;
    private int randomColumnToEnd;
    private int randomColumnToStart;
    public int scale = 5;
    public int mapSize = 20;
    public int nHoles = 20;
    public GameObject cup;
    public GameObject player;
    public int cupScale = 15;

    Vector3[] verticesWall = new Vector3[] {
                        new Vector3(0, 0, 0), //0
                        new Vector3(0, 0, 1), //1
                        new Vector3(1, 0, 0), //2
                        new Vector3(1, 0, 1), //3
                        new Vector3(0, 1, 0), //4
                        new Vector3(1, 1, 0), //5
                        new Vector3(0, 1, 1), //6
                        new Vector3(1, 1, 1), //7

                    };
    int[] trianglesWall = new int[] {
                        2,1,0,
                        1,2,3,
                        5,2,0,
                        4,5,0,
                        1,3,7,
                        1,7,6,
                        1,6,4,
                        1,4,0,
                        3,2,5,
                        5,7,3,
                        4,6,5,
                        7,5,6,


                        };

    Vector3[] verticesGround = new Vector3[] {
                        new Vector3(0, 0, 0),
                        new Vector3(0, 0, 1),
                        new Vector3(1, 0, 0),
                        new Vector3(1, 0, 1)


                    };
    int[] trianglesGround = new int[] {
                        0,1,2,
                        3,2,1
                        };

    // Start is called before the first frame update

    private void initializeCorrectPath(ref int[,] map)
    {
        Debug.Log("Startign");
        // 0 - wall , 1 - free space , 2- starting position , -3 end position
        // the player will began in the bottom of the board this means at the row 19 and in a random column and has to reach the row 0 in a random column 
        randomColumnToStart = Random.Range(0, map.GetLength(1) - 1);

        int rowToEnd = 0;
        int rowToStart = map.GetLength(0);

        // making all edges of the board a wall except those select to be the start and the end
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if ((j != randomColumnToStart && i != rowToStart || (i != rowToEnd && j != randomColumnToEnd)))
                {
                    map[i, j] = 0;
                }
                else if ((j == randomColumnToStart && i == rowToStart))
                {
                    map[i, j] = 2;

                }
                else if (i == rowToEnd && j == randomColumnToEnd)
                {
                    map[i, j] = 3;

                }
            }
        }
        Debug.Log("1fst done");
        //making sure that are at least one path to reach the end position
        int auxR = rowToStart - 1;
        int auxC = randomColumnToStart;
        try
        {


            while (auxR != rowToEnd)
            {
                float roll = Random.Range(0, 1f);


                if (roll <= 0.45f)
                {
                    if (auxC > 1)
                    {
                        auxC--;
                    }
                }
                else if (roll > 0.45f && roll <= 0.90f)
                {
                    if (auxC < map.GetLength(1) - 2)
                    {
                        auxC++;

                    }

                }
                else
                {

                    auxR--;





                }
                map[auxR, auxC] = 1;
            }
            Debug.Log("All Done");
            randomColumnToEnd = auxC;
        }


        catch (IndexOutOfRangeException e)
        {
            Debug.Log(auxR + "  " + auxC);
            Debug.LogException(e);
        }

    }
    private void addRandomHoles(ref int[,] map, int n)
    {
        while (n > 0)
        {
            int i = Random.Range(0, map.GetLength(0));
            int j = Random.Range(0, map.GetLength(1));
            map[i, j] = 1;

            n--;
        }

    }
    void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;
        int[,] map = new int[mapSize, mapSize];
        initializeCorrectPath(ref map);
        addRandomHoles(ref map, nHoles);
        //print2DimensionalArray(ref map);
        StartCoroutine(DrawMap(map));


    }
    IEnumerator DrawMap(int[,] map)
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                yield return new WaitForSeconds(0.0005f);
                {


                    GameObject obj = new GameObject();
                    obj.AddComponent<MeshFilter>();
                    Mesh mesh = new Mesh();
                    obj.AddComponent<MeshRenderer>();

                    if (map[i, j] == 0)
                    {


                        mesh.vertices = verticesWall;

                        mesh.triangles = trianglesWall;
                        obj.GetComponent<Renderer>().material = ground;

                    }
                    else
                    {

                        obj.GetComponent<Renderer>().material = wall;

                        mesh.vertices = verticesGround;

                        mesh.triangles = trianglesGround;
                    }





                    mesh.RecalculateNormals();
                    obj.GetComponent<MeshFilter>().mesh = mesh;
                    obj.AddComponent<MeshCollider>();
                    //obj.AddComponent<Rigidbody>();

                    obj.transform.SetPositionAndRotation(new Vector3(i * scale, 0, j * scale), Quaternion.identity);
                    obj.transform.localScale = new Vector3(scale, scale, scale);


                }
            }

        }

        // moving character to the start position;
        //   GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(3 + ((map.GetLength(0) - 1) * 5), 10, 7 + (randomColumnToStart * 5));
        Debug.Log(randomColumnToStart);
        player.transform.position = new Vector3(2 + ((map.GetLength(0) - 1) * scale), 10, randomColumnToStart * scale);

        cup.transform.position = new Vector3(5, 0, 5 + (randomColumnToEnd * scale));
        cup.transform.localScale = new Vector3(cupScale, cupScale, cupScale);
        Instantiate(cup);
        Instantiate(player);

    }
    // Update is called once per frame
    void Update()
    {

    }


    void print2DimensionalArray(ref int[,] a)
    {

        for (int i = 0; i < a.GetLength(0); i++)
        {

            string str = "";
            for (int j = 0; j < a.GetLength(1); j++)
            {
                str += (a[i, j]) + " ";
            }
            Debug.Log(str);
        }
    }




}
