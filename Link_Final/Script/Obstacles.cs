using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour {
    //store a list of game objects
    //use these game objects' positions to seek a certain point (x and z only)
    public List<GameObject> obstacles;

    //give the obstacles a radius for obstacle avoidance
    public float radius;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
