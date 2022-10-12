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
    public GameObject path_s; //The tracks on either side of current track
    public GameObject path_f;
    private bool switched; //One-way switch to stop constant jumping on end of track
    public bool attached;
    private PathCreator pathc;

    
    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        oldDistanceTravelled = 0;
        totalDistanceTravelled = 0;
        if (currentPath != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathc = currentPath.GetComponent<PathCreator>();
            pathc.pathUpdated += OnPathChanged;
            setPathEnds();
            
        }
        attached = false;
        Held = false;
        switched = false;
    }

    // Update is called once per frame
    void Update()
    {
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
                train_speed = Mathf.Clamp((pathc.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - distanceTravelled) * smooth, -speed, speed);
                distanceTravelled += train_speed * Time.deltaTime;
                AdjustDistance(); //Related to distance measure, getting thrown out a fuqing window


            }

            if(!Input.GetMouseButton(0))
            {
                Held = false;
            }

            


            // Debug.Log(transform.position + " " + pathCreator.path.GetPointAtTime(0) + " " + pathCreator.path.GetPointAtTime(0.9999999f));
            //Check if time to switch
            if(switched)
            {
                //Something needs to happen here that moves the train out of the range that let's it teleport
                transform.position = pathc.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                transform.rotation = pathc.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                if (Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0)) > 0.1 && Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0.9999999f)) > 0.1)
                {
                    switched = false;
                }
                
            } else
            {
                if (Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0)) < 0.1 && path_s != null)
                {
                    //Switch to First Path
                    currentPath = path_s.gameObject;
                    pathc = currentPath.GetComponent<PathCreator>();

                    transform.position = pathc.path.GetPointAtDistance(pathc.path.GetClosestDistanceAlongPath(pathc.path.GetPointAtTime(0)), endOfPathInstruction);
                    transform.rotation = pathc.path.GetRotationAtDistance(pathc.path.GetClosestDistanceAlongPath(pathc.path.GetPointAtTime(0)), endOfPathInstruction);
                

                    setPathEnds();
                    switched = true;

                }
                else if (Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0.9999999f)) < 0.1 && path_f != null)
                {
                    //Switch to Second Path
                    currentPath = path_f.gameObject;
                    pathc = currentPath.GetComponent<PathCreator>();

                    transform.position = pathc.path.GetPointAtDistance(pathc.path.GetClosestDistanceAlongPath(pathc.path.GetPointAtTime(0.9999999f)), endOfPathInstruction);
                    transform.rotation = pathc.path.GetRotationAtDistance(pathc.path.GetClosestDistanceAlongPath(pathc.path.GetPointAtTime(0.9999999f)), endOfPathInstruction);
                    setPathEnds();
                    switched = true;
                }
                else
                {
                    transform.position = pathc.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                    transform.rotation = pathc.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                }
            }
            

            
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
        path_s = currentPath.GetComponent<PathGenerator>().path_s;
        path_f = currentPath.GetComponent<PathGenerator>().path_f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coal"))
        {
            other.gameObject.GetComponent<CarScript>().attached = true;
            other.gameObject.GetComponent<CarScript>().connectRef = this.gameObject;
        }
        else if(other.gameObject.CompareTag("Train") && other != this.gameObject)
        {
            other.gameObject.GetComponent<Pather>().attached = true;
        }
    }
}
