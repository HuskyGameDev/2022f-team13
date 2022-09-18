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
    public float currentPosition;
    public float totalDistanceTravelled;
    float newDistanceTravelled;
    float oldDistanceTravelled;
    float smooth = 5.0f;
    bool Held = false;
    public Text distanceText;
    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        distanceText.text = "Distance Traveled: 0";
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
            if (Input.GetMouseButton(0) && Vector2.Distance(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 1)
            {
                //This is where we check if held
                Held = true;

                
            } else
            {
                currentPosition += 0 * Time.deltaTime;
            }

            if (Held)
            {
                currentPosition = pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                AdjustDistance();
            }

            if(!Input.GetMouseButton(0))
            {
                Held = false;
            }
            transform.position = pathCreator.path.GetPointAtDistance(currentPosition, endOfPathInstruction);
            transform.rotation = pathCreator.path.GetRotationAtDistance(currentPosition, endOfPathInstruction);
        }
    }

    void OnPathChanged()
    {
        currentPosition = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
    void AdjustDistance()
    {
        //check to see if the new position of the train has changed from before
        newDistanceTravelled = Mathf.Round(pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        if(!(oldDistanceTravelled == newDistanceTravelled)){
            //update the total distance travelled
            
            totalDistanceTravelled += Mathf.Abs(oldDistanceTravelled - newDistanceTravelled);
            //make sure old distance travelled is updated
            oldDistanceTravelled = newDistanceTravelled;
        }
        //display new total distance travelled
        distanceText.text = "Distance Traveled: " + totalDistanceTravelled;
    }
}
