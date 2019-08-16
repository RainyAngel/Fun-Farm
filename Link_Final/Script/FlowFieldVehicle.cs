using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldVehicle : Vehicle {

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
        ultimateForce += FlowFieldForce() * flowFieldWeight;

        //limit by maxForce
        ultimateForce = ultimateForce.normalized * maxForce;

        //apply the force
        ApplyForce(ultimateForce);
    }
}
