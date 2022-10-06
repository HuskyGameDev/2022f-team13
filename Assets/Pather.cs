using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;
using PathCreation.Examples;

public class Pather : MonoBehaviour
{
    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;
    public Rigidbody2D rb;
    public float speed = 10f;
    public float distanceTravelled;
    public float totalDistanceTravelled;
    float newDistanceTravelled;
    float oldDistanceTravelled;
    public float smooth = 5.0f;
    public float train_speed = 0.0f;
    public bool Held { get; private set; } = false;
    public GameManager gm;
    public PathGenerator path_s;
    public PathGenerator path_f;

    private GameObject track;
    
    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        oldDistanceTravelled = 0;
        totalDistanceTravelled = 0;
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            track = pathCreator.gameObject;
            pathCreator.pathUpdated += OnPathChanged;
            setPathEnds();
            
        }
        Held = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (pathCreator != null)
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
                train_speed = Mathf.Clamp((pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - distanceTravelled) * smooth, -speed, speed);
                distanceTravelled += train_speed * Time.deltaTime;
                AdjustDistance();


            }

            if(!Input.GetMouseButton(0))
            {
                Held = false;
            }
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);


            Debug.Log(transform.position + " " + pathCreator.path.GetPointAtTime(0) + " " + pathCreator.path.GetPointAtTime(0.9999999f));
            //Check if time to switch
            if (Vector3.Distance(transform.position, pathCreator.path.GetPointAtTime(0)) < 0.001 && path_s != null)
            {
                //Switch to First Path
                track = path_s.gameObject;
                pathCreator = track.GetComponent<PathCreator>();

                
                setPathEnds();

            } else if (Vector3.Distance(transform.position, pathCreator.path.GetPointAtTime(0.9999999f)) < 0.001 && path_f != null)
            {
                //Switch to Second Path
                track = path_f.gameObject;
                pathCreator = track.GetComponent<PathCreator>();
                setPathEnds();
            }

            
        }
    }

    void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    void AdjustDistance()
    {
        //check to see if the new position of the train has changed from before
        newDistanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
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
        path_s = track.GetComponent<PathGenerator>().path_s;
        path_f = track.GetComponent<PathGenerator>().path_f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coal"))
        {
            other.gameObject.GetComponent<CarScript>().attached = true;
            other.gameObject.GetComponent<CarScript>().trainRef = this.gameObject;
        }
    }
}
