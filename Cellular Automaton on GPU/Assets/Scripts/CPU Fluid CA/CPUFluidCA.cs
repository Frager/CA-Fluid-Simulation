using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUFluidCA : MonoBehaviour
{

    public static int ELEMENT_COUNT = 1;
    public int maxvolume;
    public int gridSize = 8;

    private Cell[,,] grid;
    private Element[] Elements;

    // Use this for initialization
    void Start()
    {
        initElements();
        initCells(gridSize);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void initElements()
    {
        Elements = new Element[ELEMENT_COUNT];

        for (int i = 0; i < ELEMENT_COUNT; ++i)
        {
            Elements[i] = new Element(i, i, i);
        }
    }

    private void initCells(int size)
    {
        grid = new Cell[size, size, size];
        for (int z = 0; z < size; ++z)
        {
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    grid[x, y, z] = new Cell(new Vector3(x,y,z));
                }
            }
        }
    }
    
    struct Element
    {
        int id;
        float viscosity;
        float density;

        public Element(int id, float viscosity, float density)
        {
            this.id = id;
            this.viscosity = viscosity;
            this.density = density;
        }
    }


    struct Cell
    {
        Vector3 position;
        int[] content;
        int volume;

        public Cell(Vector3 position)
        {
            this.position = position;
            content = new int[ELEMENT_COUNT];
            this.volume = 0;
        }

    }
}
