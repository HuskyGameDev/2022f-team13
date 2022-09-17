using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Pather : MonoBehaviour
{
    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;
    public Rigidbody2D rb;
    public float speed = 10f;
    public float distanceTravelled;
    float smooth = 5.0f;
    bool Held = false;
    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
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
                distanceTravelled += 0 * Time.deltaTime;
            }

            if (Held)
            {
                distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
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
}
