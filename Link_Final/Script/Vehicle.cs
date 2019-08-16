using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Vehicle : MonoBehaviour{

    /*----------------------------FORCED-BASED MOVEMENT (VECTORS)----------------------------*/
    public Vector3 acceleration;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 direction;

    /*----------------------------FORCED-BASED MOVEMENT (FLOATS)----------------------------*/
    public float mass;
    public float maxSpeed;
    public float maxForce;

    /*----------------------------POTENTIAL COLLISION/COLLISION DETECTION (SEPERATION, OBSTACLE AVOIDANCE, PATH FOLLOWING)----------------------------*/
    //radius of the game object
    public float radius;
    //avoidance radius specifically for obstacle avoidance
    public float avoidanceRadius;
    //the maximum distance ahead the obstacle avoider needs to check
    public float maxDistanceAhead;

    /*----------------------------FUTURE POSITION----------------------------*/
    //for always having the future position the game object
    public Vector3 futurePos;

    /*----------------------------PATH FOLLOWING----------------------------*/
    //get the path script from the scene manager (getcomponent didn't work)
    public Path pathManager;
    //for getting the game object list from the scene manager
    public List<GameObject> pointMarkers;
    //for storing the marker position list from the scene manager
    public List<Vector3> points;
    //counter used to cycle through the path following list
    public int counter;

    /*----------------------------OBSTACLE AVOIDANCE----------------------------*/
    //get the Obstacles script from the scene manager
    public Obstacles obstacleManager;
    //get the list of obstacles for obstacle avoidance
    public List<GameObject> obstacles;
    //get the radius for calculating obstacle avoidance
    public float obstacleRadius;

    /*----------------------------FLOCKING----------------------------*/
    //get the FlockManager script from the scene manager
    public FlockManager flockManager;

    /*----------------------------WANDERING----------------------------*/
    public float wanderPerlinX;
    public float wanderPerlinStep;
    public float forwardAngle;

    /*----------------------------FLOW FIELD----------------------------*/
    public FlowField flowFieldManager;

    /*----------------------------FLUID RESISTANCE----------------------------*/
    //for testing the slow down of the fluid resistance-affected vehicle
    public float velocityMagnitude;

    /*----------------------------FORCE BALANCING----------------------------*/
    public float pathFollowWeight;
    public float wanderWeight;
    public float boundaryWeight;
    public float separateWeight;
    public float avoidObstacleWeight;
    public float alignWeight;
    public float cohesionWeight;
    public float flowFieldWeight;
    public float fluidWeight;

    // Use this for initialization
    public virtual void Start()
    {
        //instantiate the position list
        points = new List<Vector3>();

        //store the lists
        pointMarkers = pathManager.pointMarkers;
        obstacles = obstacleManager.obstacles;

        //get the position list from the game objects
        foreach (var pointMarker in pointMarkers)
        {
            points.Add(pointMarker.transform.position);
        }

        //set counter to the start
        counter = 0;

        //get the radius for the obstacles
        obstacleRadius = obstacleManager.radius;

        //get the initial random value for wandering
        wanderPerlinX = Random.Range(0f, 1f);
        //set the step value for wandering
        wanderPerlinStep = .001f;
    }

    // Update is called once per frame
    void Update()
    {
        //store the future position every frame
        futurePos = transform.position + velocity * 2;

        velocityMagnitude = velocity.magnitude;

        //called to calculate the steering force
        CalcSteeringForces();

        //update everything accordingly so the object actually moves
        UpdatePosition();
        SetTransform();
    }

    void UpdatePosition()
    {
        position = transform.position;

        //1. add acceleration to velocity
        velocity += acceleration * Time.deltaTime;

        //clamp the magnitude
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        //restrict that dumb y
        velocity.y = 0;

        //2. add velocity to position
        position += velocity * Time.deltaTime;

        //3. get a normalized velocity as direction
        direction = velocity.normalized;

        //4. refresh acceleration
        acceleration = Vector3.zero;
    }

    void SetTransform()
    {
        //get the current position of the game object
        transform.position = position;

        //make the object's forward face the direction vector
        transform.forward = direction;
    }

    //may or may not need this
    //for calculating force
    public void ApplyForce(Vector3 force)
    {
        //if we used just = and not +=, then the force/mass would overwrite the other one which is not what we want
        acceleration += force / mass;
    }

    //for calculating the steering forces of the victims and enemies
    public abstract void CalcSteeringForces();

    //for the enemies to seek the victims
    public Vector3 Seek(Vector3 targetPosition)
    {
        //calculate the desired velocity (desired = target pos - my pos)
        Vector3 desiredVelocity = targetPosition - position;

        //scale desired to max speed
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        //calculate the steering force = desired - current
        Vector3 seekingForce = desiredVelocity - velocity;

        //return the steering force to be applied (to acceleration)
        return seekingForce;
    }

    //for the victims to flee the enemies
    public Vector3 Flee(Vector3 targetPosition)
    {
        //calculate the desired velocity (desired = target pos - my pos)
        Vector3 desiredVelocity = targetPosition - position;

        //scale desired to max speed
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        //calculate the steering force = desired - current
        Vector3 fleeingForce = (-desiredVelocity) - velocity;

        //return the steering force to be applied (to acceleration)
        return fleeingForce;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="points">GameObjects for getting a position in the world from</param>
    /// <returns>a force for the follower</returns>
    public Vector3 PathFollow(List<Vector3> points)
    {
        //if the distance is less than the sum of the radii...
        if(Vector3.Distance(transform.position, points[counter]) < radius)
        {
            //check to make sure the counter is not greater than the list (aka at the last point)
            //if it is, set it back to 0 (aka the first point)
            if(counter == points.Count - 1)
            {
                counter = 0;
            }
            //else go to the next point
            else
            {
                counter++;
            }
        }

        //now seek the next point
        return Seek(new Vector3(points[counter].x, 0, points[counter].z));
    }

    //for calculating the force needed to avoid an obstacle
    public Vector3 AvoidObstacle()
    {
        //force for moving around an obstacle
        Vector3 obstacleForce = Vector3.zero;

        //go through the obstacle list and store those in front of the vehicle
        for (int i = 0; i < obstacles.Count; i++)
        {
            //stores the value of the dot product between the objects in front of the vehicle and the vehicle
            float checker = Vector3.Dot(transform.forward, obstacles[i].transform.position - transform.position);

            //only check for collisions within a certain distance (certain distance = maxDistanceAhead)
            if (checker > 0 && checker < maxDistanceAhead)
            {

                //check to see if the distance between the two objects is less than the unique radius + the obstacles' radius
                float distance = Vector3.Distance(transform.position, obstacles[i].transform.position);

                //check for the two game objects potentially colliding
                if (avoidanceRadius + obstacleRadius > distance)
                {
                    //if they are within the two radii of each other, get a new dot product between them to see if they will collide on the right or left
                    //(or in front)
                    checker = Vector3.Dot(transform.right, obstacles[i].transform.position - transform.position);

                    //treats the obstacle's radius as though it is next to obstacle avoider (so if the sum of the radii is less than the dot product's
                    //calculated distance, then they are colliding and have to be avoided)
                    if (checker < radius + obstacleRadius)
                    {
                        //if on the right
                        if (checker > 0)
                        {
                            Debug.DrawLine(transform.position, obstacles[i].transform.position);
                            return -transform.right;
                        }
                        //if on the left
                        if (checker <= 0)
                        {
                            Debug.DrawLine(transform.position, obstacles[i].transform.position);
                            return transform.right;
                        }
                    }
                }

            }
        }

        //return the force needed to avoid obstacles
        return obstacleForce;
    }

    public Vector3 Wander()
    {
        Vector3 wanderForce = Vector3.zero;

        //choose a distance ahead
        Vector3 distanceAhead = transform.forward * 3;

        //"project a circle" into that space of a certain radius
        float imaginaryRadius = 2;

        //get a random angle to determine the displacement vector
        float randomAngle = (Mathf.PerlinNoise(wanderPerlinX, 1) - 0.5f) * 2 * 1000 + transform.eulerAngles.y;
        randomAngle = randomAngle * Mathf.Deg2Rad;

        //print(transform.eulerAngles.y + " " + Mathf.Acos(transform.forward.x) * Mathf.Rad2Deg);

        //get the forward angle to rotate with
        forwardAngle = Mathf.Acos(transform.forward.x) * Mathf.Rad2Deg;
        wanderPerlinX += wanderPerlinStep;

        //get the x and z for the new location
        float x = Mathf.Cos(randomAngle) * imaginaryRadius;
        float z = Mathf.Sin(randomAngle) * imaginaryRadius;

        //get the vector of the small circle
        Vector3 smallCircleVector = new Vector3(x, 0, z);

        //get the vector from transform.position and the head of angled vector
        return Seek(transform.position + distanceAhead + smallCircleVector);
    }

    //makes any game object trying to leave the bounds of the world turn around to the center of the world
    public Vector3 StayInBounds(Vector3 targetFuturePos)
    {
        Vector3 returnPoint = new Vector3(310 / 2, 0, 365 / 2);

        return Seek(returnPoint);
    }

    //for finding the nearest target and returning the game object
    public GameObject NearestTargetObject(List<GameObject> targets)
    {
        //the GameObject to be returned
        GameObject nearestTarget = null;

        //for the inital comparison
        float minDistance = Mathf.Infinity;

        //check every victim's position to see which is the closest
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null && targets[i] != gameObject)
            {
                //gets the distance between two game objects
                float distance = Vector3.Distance(position, targets[i].transform.position);

                //compares the two distances
                if (distance < minDistance)
                {
                    //sets the nearest target to the new value
                    nearestTarget = targets[i];

                    //set a new minDistance to compare to
                    minDistance = distance;
                }
            }
        }
        return nearestTarget;
    }

    //for getting the flockers to face along the same direction
    public Vector3 Align()
    {
        //get the average direction of the flock from the flock manager object
        //set it to the velocity
        Vector3 alignForce = flockManager.avgDir - velocity;

        return alignForce;
    }

    //for getting the flockers to be around the same position
    public Vector3 Cohesion()
    {
        //get the average position of the flock and seek it
        return Seek(new Vector3(flockManager.avgPos.x,
            Terrain.activeTerrain.SampleHeight(new Vector3(position.x, 0, position.z)),
            flockManager.avgPos.z));
    }


    /// <summary>
    /// check to see who is nearest
    /// flee them
    /// </summary>
    /// <returns>the seperation force</returns>
    public Vector3 Separate()
    {
        //store nearest target's position
        Vector3 nearestPos = NearestTargetObject(flockManager.flockers).transform.position;

        //get the distance to compare
        float distance = Vector3.Distance(transform.position, nearestPos);

        //check to see if they need to seperate
        if (NearestTargetObject(flockManager.flockers).GetComponent<FlockingVehicle>().avoidanceRadius + avoidanceRadius > distance)
        {
            return Flee(NearestTargetObject(flockManager.flockers).transform.position);
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// to get the force of direction the flow field follower needs
    /// </summary>
    /// <returns></returns>
    public Vector3 FlowFieldForce()
    {
        //get the desired velocity from the flow field
        Vector3 desiredVelocity = flowFieldManager.GetDirection(futurePos) * maxSpeed;

        //return the force the flow field follower will need
        return desiredVelocity - velocity;
    }

    /// <summary>
    /// Creates the mud resistance force in the program for the flockers
    /// </summary>
    /// <returns>a resistance force</returns>
    public Vector3 DragForce()
    {
        //get the drag force
        Vector3 dragForce = velocity * -1;
        //normalize 
        dragForce.Normalize();

        return dragForce;
    }
}

