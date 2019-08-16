using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollowingVehicle : Vehicle {
    //material for the debug lines
    public Material debugResistanceMaterial;
    public Material debugPathMaterial;

    // Use this for initialization
    public override void Start () {
        base.Start();
    }

    //a unique steering force calulcation for the path following vehicles
    public override void CalcSteeringForces()
    {
        //create an empty ultimate force
        Vector3 ultimateForce = Vector3.zero;

        //add the forces and their weight
        ultimateForce += PathFollow(points) * pathFollowWeight;

        //check the area where the resistance force should be applied
        if (transform.position.x > 633 && transform.position.x < 679 && transform.position.z > 449 && transform.position.z < 493)
        {
            ultimateForce += DragForce() * fluidWeight;
        }

        //limit by maxForce
        ultimateForce = ultimateForce.normalized * maxForce;

        //apply the force
        ApplyForce(ultimateForce);
    }

    void OnRenderObject()
    {
        if (pathManager.showResistanceDebug)
        {
            // Set the material to be used for the first line  
            debugResistanceMaterial.SetPass(0); 

            // Draws a quad shape   
            GL.Begin(GL.QUADS);
            // Begin to draw the quad
            //First corner
            GL.Vertex3(633, 2, 449);
            // Second corner 
            GL.Vertex3(633, 2, 493);
            //Third corner
            GL.Vertex3(679, 2, 493);
            //Fourth corner
            GL.Vertex3(679, 2, 449);
            // Finish drawing the line 
            GL.End();
        }

        if(pathManager.showPathDebug)
        {
            for (int i = 0; i < points.Count; i++)
            {
                // Set the material to be used for the first line  
                debugPathMaterial.SetPass(0);

                // Draws a quad shape   
                GL.Begin(GL.LINES);
                // Begin to draw the lines
                //when i equals the last point in the list...
                if(i == points.Count-1)
                {
                    //Starting point
                    GL.Vertex3(points[i].x, 2, points[i].z);
                    // Ending point 
                    GL.Vertex3(points[0].x, 2, points[0].z);
                }
                else
                {
                    //Starting point
                    GL.Vertex3(points[i].x, 2, points[i].z);
                    // Ending point 
                    GL.Vertex3(points[i + 1].x, 2, points[i + 1].z);
                }
                // Finish drawing the line 
                GL.End();
            }
        }
    }
}
