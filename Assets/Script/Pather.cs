using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;
using PathCreation.Examples;

public class Pather : MonoBehaviour
{

    //Note to future me, change this to use all game objects instead of other things.
    public GameObject currentPath; //The Path the Train is on
    public EndOfPathInstruction endOfPathInstruction;
    public Rigidbody2D rb;
    public float speed = 10f;
    public float distanceTravelled;
    public float totalDistanceTravelled;
    float newDistanceTravelled; //A bunch of stuff that is going to get absolutely gutted soon
    float oldDistanceTravelled;
    public float smooth = 5.0f;
    public float train_speed = 0.0f;
    public bool Held { get; private set; } = false;
    public GameManager gm; //game manager
    public GameObject[] paths;
    public GameObject path_s; //The tracks on either side of current track
    public GameObject path_f;
    private bool switched; //One-way switch to stop constant jumping on end of track
    public bool attached;
    private PathCreator pathc;
    private PathGenerator pathg;
    public GameObject connectRef1;
    public GameObject connectRef2;
    public float connectRef1Speed;
    public float connectRef2Speed;
    float connectRef1SpeedPrev = 0;
    float connectRef2SpeedPrev = 0;
    public float flip;
    public GameObject trainModel;

    
    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        flip = 1;
        trainModel = this.gameObject.transform.GetChild(0).gameObject;
        oldDistanceTravelled = 0;
        totalDistanceTravelled = 0;
        if (currentPath != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathc = currentPath.GetComponent<PathCreator>();
            pathg = currentPath.GetComponent<PathGenerator>();
            pathc.pathUpdated += OnPathChanged;
            setPathEnds();
            
        }
        attached = false;
        Held = false;
        switched = false;
        paths = gm.paths;
    }

    // Update is called once per frame
    void Update()
    {

        if (paths.Length == 0)
        {
            paths = gm.paths;
        }
        //Update our Paths if they are not known, should only happen once realistically
        if(!GameObject.ReferenceEquals(path_f, pathg.path_f) || !GameObject.ReferenceEquals(path_s, pathg.path_s))
        {
            path_f = pathg.path_f;
            path_s = pathg.path_s;
        }

        if (pathc != null)
        {
            //Check if the mouse is in the proper position to start moving the train when clicked
            if (Input.GetMouseButton(0) && Vector2.Distance(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 1)
            {
                //If so, set the Held variable that will allow the mouse almost free movement while handling the train
                Held = true;

                
            } else
            {
                //Otherwise, that train is moving nowhere
                distanceTravelled += 0 * Time.deltaTime;
            }

            if (Held)
            {
                //This is where the trains actual movement is held. Currently it teleports to closest track position to mouse.
                //The goal is a to build a basic p loop that will move the train to the same designated point at a reasonable speed.
                //This looks scary because I put it on one line. It is basically: (closest_mouse_position - train_position) * smooth, clamped between (-speed, speed). Otherwise known as a p-loop



                //BIG IMPORTANT NOTE: This needs to pid to the closest point along any path, so we have to take all of the paths from the list, find the closest point to the mouse along each of them and use that one for my calculation here
                //this will let us track the speed across the various track pieces and smooth out the jumps.

                //Steps:
                //1. Collect all the of the track pieces, GameManager Paths is a legal option
                //2. Find the closest point to the mouse from all of them
                //3. Build the total distance required to get to that point from our current point
                //3a. This will require doing some major path planning schenanigans, as we have to be able to track the path along all of its connections
                //3b. It is breadth first search time
                //3c. Once we have the path, we have to come up with a total distance from it that accounts for all the lengths of track that have to be taken
                //3d. Alternatively, it might just work and I am overcomplicating the fuq out of it.
                //4. Turn that into a speed calculation here
                //5. Profit????????
                //6. Either Way this will be a big overhaul of how this calculation works, but it should ideally be somewhat isolated, since the other objects will not be PIDing at the same time
                train_speed = Mathf.Clamp((pathc.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - distanceTravelled) * smooth, -speed, speed);
                


            } else if (attached)
            {
                //Steps:
                //1. Check each side for a connection
                //2. If connected collect the speed values
                //3. Compare acceleration of one to the acceleration of another (current speed - previous speed)
                //4. Store Previous Values
                bool connect1 = (connectRef1 != null); //These will work as catch variables to avoid errors
                bool connect2 = (connectRef2 != null);

                if (connect1)
                {
                    if (connectRef1.CompareTag("Train"))
                    {
                        connectRef1Speed = connectRef1.GetComponent<Pather>().train_speed;
                    }
                    else if (connectRef1.CompareTag("Coal")) 
                    {
                        connectRef1Speed = connectRef1.GetComponent<CarScript>().car_speed;
                    }
                }

                if (connect2)
                {
                    if (connectRef2.CompareTag("Train"))
                    {
                        connectRef2Speed = connectRef2.GetComponent<Pather>().train_speed;
                    }
                    else if (connectRef2.CompareTag("Coal"))
                    {
                        connectRef2Speed = connectRef2.GetComponent<CarScript>().car_speed;
                    }
                }

                //Compare the acceleration values here
            
                if (Mathf.Abs(connectRef1Speed - connectRef1SpeedPrev) > Mathf.Abs(connectRef2Speed - connectRef2SpeedPrev))
                {
                    //Follow ref1
                    train_speed = connectRef1Speed;
                }
                else if (Mathf.Abs(connectRef1Speed - connectRef1SpeedPrev) < Mathf.Abs(connectRef2Speed - connectRef2SpeedPrev))
                {
                    //follow ref2
                    train_speed = connectRef2Speed;
                }
                else
                {
                    //Do nothing, Follow your own path
                    //-Gandhi, probably

                    //In actuality, this case is when the rates of change between all parties are equal, we want to take speed that is lowest in this case for reasons, I think...
                    train_speed = Mathf.Abs(connectRef1Speed) > Mathf.Abs(connectRef2Speed) ? connectRef1Speed : connectRef2Speed;
                }
                
               

                connectRef1SpeedPrev = connectRef1Speed;
                connectRef2SpeedPrev = connectRef2Speed;

            } else
            {
                train_speed = 0;
            }

            distanceTravelled += train_speed * Time.deltaTime;
            AdjustDistance();

            if (!Input.GetMouseButton(0))
            {
                Held = false;
            }

            //Switch Tracks
            //Steps:
            //1. Figure out if we are end the ends of the track
            //2. Figure out if there is a new track attached at that end
            //3. Switch Tracks
            //4. DO NOT UNDER ANY CIRCUMSTANCES TELEPORT, I SWEAR TO ALL THINGS HOLY I WILL REND THIS TRAIN FROM ITS TRACKS IF IT IS SO MUCH AS ONE PIXEL TOO FAR
           if(switched)
            {
                if (Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0)) > 0.5 && Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0.99f)) > 0.5)
                {
                    switched = false;
                }
            } else
            {
                float dist_s = Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0));
                float dist_f = Vector3.Distance(transform.position, pathc.path.GetPointAtTime(.99f));
                if (dist_s < 0.12 && path_s != null)
                {
                    currentPath = path_s;
                    pathc = currentPath.GetComponent<PathCreator>();
                    pathg = currentPath.GetComponent<PathGenerator>();
                    distanceTravelled = pathc.path.GetClosestDistanceAlongPath(transform.position);
                    setPathEnds();
                    //Do Something here that resets distance travelled in order to make it work.
                    if (distanceTravelled < 1)
                    {
                        trainModel.transform.Rotate(trainModel.transform.rotation.x, trainModel.transform.rotation.y, trainModel.transform.rotation.z - 180f, Space.Self);
                    }
                    switched = true;
                }
                else if (dist_f < 0.12 && path_f != null)
                {
                    currentPath = path_f;
                    pathc = currentPath.GetComponent<PathCreator>();
                    pathg = currentPath.GetComponent<PathGenerator>();
                    distanceTravelled = pathc.path.GetClosestDistanceAlongPath(transform.position);
                    setPathEnds();
                    //Do Something here that resets distance travelled in order to make it work.
                    if(distanceTravelled > 1)
                    {
                        trainModel.transform.Rotate(trainModel.transform.rotation.x, trainModel.transform.rotation.y, trainModel.transform.rotation.z + 180f, Space.Self);
                    }
                    switched = true;
                }
                
            }
            if (Input.GetMouseButton(1) && (Vector2.Distance(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 0.5))
            {
                attached = false;
                connectRef1 = null;
                connectRef2 = null;
                connectRef1Speed = 0;
                connectRef2Speed = 0;
                distanceTravelled += 0 * Time.deltaTime;
            }
            transform.position = pathc.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            transform.rotation = pathc.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            Debug.Log("Train: " + Time.deltaTime + "\n");   
            
        }
    }

    void OnPathChanged()
    {
        distanceTravelled = pathc.path.GetClosestDistanceAlongPath(transform.position);
    }

    void AdjustDistance()
    {
        //check to see if the new position of the train has changed from before
        newDistanceTravelled = pathc.path.GetClosestDistanceAlongPath(transform.position);
        if (Mathf.Abs(oldDistanceTravelled - newDistanceTravelled) >= 1)
        {
            //update the total distance travelled
            totalDistanceTravelled += Mathf.Round(Mathf.Abs(oldDistanceTravelled - newDistanceTravelled));
            //make sure old distance travelled is updated
            oldDistanceTravelled = newDistanceTravelled;
        }
        //display new total distance travelled
        gm.ChangeText(totalDistanceTravelled);
    }

    void setPathEnds()
    {
        path_s = pathg.path_s;
        path_f = pathg.path_f;
    }

    void OnTriggerEnter(Collider other)
    {
        //if the other object is a car, connect with it by giving a reference, the other object will get the speed
        if (other.gameObject.CompareTag("Coal"))
        {
            other.gameObject.GetComponent<CarScript>().attached = true;
            if (other.gameObject.GetComponent<CarScript>().connectRef1 == null)
            {
                other.gameObject.GetComponent<CarScript>().connectRef1 = this.gameObject;
            }
            else if (other.gameObject.GetComponent<CarScript>().connectRef1 != this.gameObject)
            {
                other.gameObject.GetComponent<CarScript>().connectRef2 = this.gameObject;
            }
        }
        //if the other object is a train, connect with it by giving a reference, the other object will get the speed
        else if (other.gameObject.CompareTag("Train") && other != this.gameObject && !Held)
        {
            if (other.gameObject.GetComponent<Pather>().connectRef1 == null)
            {
                other.gameObject.GetComponent<Pather>().connectRef1 = this.gameObject;
            }
            else if (other.gameObject.GetComponent<Pather>().connectRef1 != this.gameObject)
            {
                other.gameObject.GetComponent<Pather>().connectRef2 = this.gameObject;
            }
        }
    }
}
