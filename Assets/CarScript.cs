using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;

public class CarScript : MonoBehaviour
{
    public PathCreator pathCreator;
    public Rigidbody2D rb;
    public float speed = 10f;
    public float currentPosition;
    public float smooth = 5.0f;
    bool Held = false;
    public GameManager gm;

    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        currentPosition = 5;
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


            }
            else
            {
                //Otherwise, that train is moving nowhere
                currentPosition += 0 * Time.deltaTime;
            }

            if (Held)
            {
                //This is where the trains actual movement is held. Currently it teleports to closest track position to mouse.
                //The goal is a to build a basic p loop that will move the train to the same designated point at a reasonable speed.
                //currentPosition = pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition))

                currentPosition += Mathf.Clamp((pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - currentPosition) * smooth, -smooth, smooth) * Time.deltaTime;
            }

            if (!Input.GetMouseButton(0))
            {
                Held = false;
            }
            transform.position = pathCreator.path.GetPointAtDistance(currentPosition);
            transform.rotation = pathCreator.path.GetRotationAtDistance(currentPosition);



        }
    }

    void OnPathChanged()
    {
        currentPosition = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
}
