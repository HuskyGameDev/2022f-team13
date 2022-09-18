using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;

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
    bool Held = false;
    public GameManager gm;
    
    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        
        oldDistanceTravelled = 0;
        totalDistanceTravelled = 0;
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;
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
                //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition))

                distanceTravelled += Mathf.Clamp((pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - distanceTravelled) * smooth, -smooth, smooth) * Time.deltaTime;
                AdjustDistance();
            }

            if(!Input.GetMouseButton(0))
            {
                Held = false;
            }
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);


            
        }
    }

    void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
    
    void AdjustDistance()
    {
        //check to see if the new position of the train has changed from before
        newDistanceTravelled = Mathf.Round(pathCreator.path.GetClosestDistanceAlongPath(transform.position));
        if(!(oldDistanceTravelled == newDistanceTravelled)){
            //update the total distance travelled
            
            totalDistanceTravelled += Mathf.Abs(oldDistanceTravelled - newDistanceTravelled);
            //make sure old distance travelled is updated
            oldDistanceTravelled = newDistanceTravelled;
        }
        //display new total distance travelled
        gm.ChangeText(totalDistanceTravelled);
    }
}
