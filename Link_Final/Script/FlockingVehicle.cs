using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingVehicle : Vehicle {

    // Use this for initialization
    public override void Start()
    {
        base.Start();
    }

    //a unique steering force calulcation for the path following vehicles
    public override void CalcSteeringForces()
    {
        //create an empty ultimate force
        Vector3 ultimateForce = Vector3.zero;

        //add the forces and their weight
        ultimateForce += Wander() * wanderWeight;
        ultimateForce += Cohesion() * cohesionWeight;
        ultimateForce += Align() * alignWeight;
        ultimateForce += Separate() * separateWeight;

        //check the area where the sheep should be kept within
        if (futurePos.x > 310 || futurePos.z > 635 || futurePos.x < 8 || futurePos.z < 5)
        {
            ultimateForce += StayInBounds(futurePos) * boundaryWeight;
        }         

        //limit by maxForce
        ultimateForce = ultimateForce.normalized * maxForce;

        //apply the force
        ApplyForce(ultimateForce);
    }
}
