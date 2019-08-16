using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour {
    //store a list of game objects
    //use these game objects' positions to seek a certain point (x and z only)
    public List<GameObject> pointMarkers;

    public bool showResistanceDebug;

    public bool showPathDebug;

    // Use this for initialization
    void Start () {
       
	}
	
	// Update is called once per frame
	void Update () {
        //turns the debug for the resistance field on and off
        if (Input.GetKeyDown(KeyCode.R))
        {
            showResistanceDebug = !showResistanceDebug;
        }

        //turns the debug for the path for the path on and off
        if(Input.GetKeyDown(KeyCode.P))
        {
            showPathDebug = !showPathDebug;
        }
	}

    //display text to tell the user how to navigate the game
    private void OnGUI()
    {
        GUI.Box(new Rect(20, 30, 300, 55), "Press 'F' key to turn on flow field debug lines \n Press 'P' key to turn on path debug lines \n Press 'R' key to turn on resistance debug lines");
    }
}
