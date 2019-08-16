using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    //area of flow field to store the direction
    public Vector3[,] flowField;

    //center of pond area
    public Vector3 flowFieldCenter;

    //width and height of the area
    public float width;
    public float height;

    //position of the flow field
    public float x;
    public float y;
    public float z;

    //the size of the flow field
    public int rows;
    public int columns;

    //size of the indivual cells
    public float cellWidth;
    public float cellHeight;

    //for drawing the debug lines through the center of the flow field
    public float debugLinesRadius;

    //material for the debug lines
    public Material debugMaterial;

    //for turning the debug on and off
    public bool showFlowDebug;

    // Use this for initialization
    void Start()
    {
        //instantiate the array
        flowField = new Vector3[columns, rows];

        //get the width and height of each individual cell in the flow field
        cellWidth = width / columns;
        cellHeight = height / rows;

        //get the center of the flow field
        flowFieldCenter = new Vector3(width / 2, 0, height / 2);

        //so the debug lines can be drawn through the point and be as big as possible
        debugLinesRadius = Mathf.Min(cellWidth, cellHeight) / 2;

        //creates the flow field
        CreateFlowField();
    }

    // Update is called once per frame
    void Update()
    {
        //check to see if show debug is turned on or off
        if(Input.GetKeyDown(KeyCode.F))
        {
            showFlowDebug = !showFlowDebug;
        }
    }

    /// <summary>
    /// creates the flow field array that is circular (because its a pond)
    /// </summary>
    public void CreateFlowField()
    {
        //nested for loop to get to each cell
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                //get the center of each cell
                Vector3 cellCenter = new Vector3((cellWidth / 2 + cellWidth * i), 0, (cellHeight / 2 + cellHeight * j));

                //subtract the center of the flow field from the center of the current cell
                Vector3 centerDistance = flowFieldCenter - cellCenter;

                //handle case where center = cell center
                if(centerDistance.sqrMagnitude == 0)
                {
                    centerDistance = new Vector3(2, 0, 2);
                }

                //get the perpendicular vector from the center distance
                Vector3 perpendicular = new Vector3(-centerDistance.z, 0, centerDistance.x);

                //store the direction value into the flow field
                flowField[i, j] = perpendicular.normalized;

            }
        }
    }

    public Vector3 GetDirection(Vector3 futurePosition)
    {
        //convert the future position of the flow field follower from global to local space
        Vector3 localFuturePos = futurePosition - new Vector3(x, futurePosition.y, z);

        //convert physical space of the array into i and j values to work with (for locating what cell the flow field follower is in)
        int iArrayPosition = (int) Mathf.Floor(localFuturePos.x / cellWidth);
        int jArrayPosition = (int) Mathf.Floor(localFuturePos.z / cellHeight);

        //check to make sure the iArrayPosition and jArrayPosition never go below or above the actual index of the flowField 
        if(iArrayPosition >= columns)
        {
            iArrayPosition = columns-1;
        }
        else if(iArrayPosition < 0)
        {
            iArrayPosition = 0;
        }

        if (jArrayPosition >= rows)
        {
            jArrayPosition = rows-1;
        }
        else if (jArrayPosition < 0)
        {
            jArrayPosition = 0;
        }

        //return the direction vector 
        return flowField[iArrayPosition, jArrayPosition];
    }

    void OnRenderObject()
    {
        if (showFlowDebug)
        {
            //nested for loop to get to each cell
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    //get the center of each cell
                    Vector3 cellCenter = new Vector3((cellWidth / 2 + cellWidth * i), 0, (cellHeight / 2 + cellHeight * j));

                    // Set the material to be used for the first line  
                    debugMaterial.SetPass(0);
                    
                    // Draws one line    
                    GL.Begin(GL.LINES);
                    // Begin to draw lines 
                    // First endpoint of this line
                    GL.Vertex(cellCenter - (flowField[i, j] * debugLinesRadius) + new Vector3(x, y, z));
                    // Second endpoint of this line 
                    GL.Vertex(cellCenter + (flowField[i, j] * debugLinesRadius) + new Vector3(x, y, z));
                    // Finish drawing the line 
                    GL.End();
                }
            }
        }
    }
}

