using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;

public class CarScript : MonoBehaviour
{
    public PathCreator pathCreator;
    public Rigidbody2D rb;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 10f;
    public float currentPosition;
    public float trainPosition;
    public float smooth = 5.0f;
    public bool attached;
    public float car_speed = 0.0f;
    public GameManager gm;
    public GameObject connectRef;
    public GameObject newconnectRef;

    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        attached = false;
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
                if (connectRef.GetComponent<Pather>().Held)
                 {
                     currentPosition += connectRef.GetComponent<Pather>().train_speed * Time.deltaTime;
                 }
                
                else
                {
                    currentPosition += 0 * Time.deltaTime;
                }
            }
            //Check if mouse is clicking on the car when attached, detach from train if it is
            if (Input.GetMouseButton(1) && (Vector2.Distance(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 0.5))
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
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coal"))
        {
            other.gameObject.GetComponent<CarScript>().attached = true;
            other.gameObject.GetComponent<CarScript>().connectRef = this.connectRef;
        }
        else if (other.gameObject.CompareTag("Train") && other != this.gameObject)
        {
            other.gameObject.GetComponent<Pather>().attached = true;
        }
    }
}
