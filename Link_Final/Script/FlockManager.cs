using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour {

    //vectors for getting the average position and average direction of the flock (for cohesion and alignment)
    public Vector3 avgPos;
    public Vector3 avgDir;

    //store the flockers so avgPos and avgDir can be calculated
    public List<GameObject> flockers;

    //for the camera to be in the center of the sheep
    public GameObject center;

    // Use this for initialization
    void Start(){

    }

    // Update is called once per frame
    void Update()
    {
        GetAvgDir();
        GetAvgPos();

        //positions the game object at the center of the sheep
        center.transform.position = avgPos;
        center.transform.forward = avgDir.normalized;
    }

    //get the average direction
    public void GetAvgDir()
    {
        //get the flock's average direction
        Vector3 avgDirection = Vector3.zero;

        //average of something = (sum of everything's value)/number of things
        for (int i = 0; i < flockers.Count; i++)
        {

            avgDirection += flockers[i].transform.forward;
        }

        avgDirection = avgDirection / flockers.Count;

        //set the avgDir so the flock vehicles can get it
        avgDir = avgDirection;
    }

    //get the average position
    public void GetAvgPos()
    {
        Vector3 avgPosition = Vector3.zero;

        //get the flock's average position
        //same math concept as above
        for (int i = 0; i < flockers.Count; i++)
        {
            if (flockers[i] == null)
            {

                print(i);
            }
            avgPosition += flockers[i].transform.position;
        }

        avgPosition = avgPosition / flockers.Count;

        //make sure the flock vehicle cana access the info
        avgPos = avgPosition;
    }
}
