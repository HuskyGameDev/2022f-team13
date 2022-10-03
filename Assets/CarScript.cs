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
    public float trainPosition;
    public float smooth = 5.0f;
    public bool attached;
    public float car_speed = 0.0f;
    public GameManager gm;
    public GameObject trainRef;

    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        attached = false;
        currentPosition = 5;
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pathCreator != null)
        {
            
            if (attached)
            {
                currentPosition += trainRef.GetComponent<Pather>().train_speed * Time.deltaTime;
            }
            //Check if mouse is clicking on the car when attached, detach from train if it is
            if ((!Input.GetMouseButton(0)) && (Vector2.Distance(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 0.5) && !trainRef.GetComponent<Pather>().Held)
            {
               attached = false;
               currentPosition += 0 * Time.deltaTime;
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
