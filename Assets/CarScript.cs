using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;
using PathCreation.Examples;

public class CarScript : MonoBehaviour
{
    public GameObject currentPath; //The Path the Train is on
    public Rigidbody2D rb;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 10f;
    public float currentPosition;
    public float trainPosition;
    public float smooth = 5.0f;
    public bool attached;
    public float car_speed = 0.0f;
    public GameManager gm;
    public GameObject connectRef1;
    public GameObject connectRef2;
    public float connectRef1Speed;
    public float connectRef2Speed;
    public string carTag;

    //Track Switching Stuff
    public PathCreator pathc;
    public PathGenerator pathg;
    public GameObject carModel;
    public GameObject path_s; //The tracks on either side of current track
    public GameObject path_f;
    private bool switched; //One-way switch to stop constant jumping on end of track

    Vector2 v2;
    // Start is called before the first frame update
    void Start()
    {
        carModel = this.gameObject.transform.GetChild(0).gameObject;
        attached = false;
        if (currentPath != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathc = currentPath.GetComponent<PathCreator>();
            pathg = currentPath.GetComponent<PathGenerator>();
            pathc.pathUpdated += OnPathChanged;
            setPathEnds();
        }
        attached = false;
        switched = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Update our Paths if they are not known, should only happen once realistically
        if (!GameObject.ReferenceEquals(path_f, pathg.path_f) || !GameObject.ReferenceEquals(path_s, pathg.path_s))
        {
            path_f = pathg.path_f;
            path_s = pathg.path_s;
        }

        if (pathc != null)
        {
            //if the car is attached to something
            if (attached)
            {
                //get the speed of the connected object
                if (connectRef1.CompareTag("Train"))
                {
                    connectRef1Speed = connectRef1.GetComponent<Pather>().train_speed;
                }
                else if (connectRef1.CompareTag("Coal"))
                {
                    connectRef1Speed = connectRef1.GetComponent<CarScript>().car_speed;
                }
                //if there isn't a second thing connected, just go with the train
                if (connectRef2 == null)
                {
                    car_speed = connectRef1Speed;
                    currentPosition += car_speed * Time.deltaTime;
                }


                //if there is another object connected, compare their speeds and determine where to go
                else
                {
                    if (connectRef2.CompareTag("Train"))
                    {
                        connectRef2Speed = connectRef2.GetComponent<Pather>().train_speed;
                    }
                    else if (connectRef2.CompareTag("Coal"))
                    {
                        connectRef2Speed = connectRef2.GetComponent<CarScript>().car_speed;
                    }
                    //if the first connected object is faster go that way
                    if (Mathf.Abs(connectRef1Speed) > Mathf.Abs(connectRef2Speed))
                    {
                        car_speed = connectRef1Speed;
                        currentPosition += car_speed * Time.deltaTime;
                    }
                    //if the second connected object is faster, go that way
                    else if (Mathf.Abs(connectRef1Speed) <= Mathf.Abs(connectRef2Speed))
                    {
                        car_speed = connectRef2Speed;
                        currentPosition += car_speed * Time.deltaTime;
                    }
                }
            }
            //Debug.Log(car_speed + " " + connectRef1Speed + " " + connectRef2Speed + "\n");
            //Check if right mouse button is clicked on the car when attached, detach from train if it is
            if (Input.GetMouseButton(1) && (Vector2.Distance(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 0.5))
            {
                attached = false;
                connectRef1 = null;
                connectRef2 = null;
                connectRef1Speed = 0;
                connectRef2Speed = 0;
                currentPosition += 0 * Time.deltaTime;
            }

            //Switch Tracks
            //Steps:
            //1. Figure out if we are end the ends of the track
            //2. Figure out if there is a new track attached at that end
            //3. Switch Tracks
            //4. DO NOT UNDER ANY CIRCUMSTANCES TELEPORT, I SWEAR TO ALL THINGS HOLY I WILL REND THIS TRAIN FROM ITS TRACKS IF IT IS SO MUCH AS ONE PIXEL TOO FAR
            if (switched)
            {
                if (Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0)) > 0.1 && Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0.99f)) > 0.1)
                {
                    switched = false;
                }
            }
            else
            {
                float dist_s = Vector3.Distance(transform.position, pathc.path.GetPointAtTime(0));
                float dist_f = Vector3.Distance(transform.position, pathc.path.GetPointAtTime(.99f));
                if (dist_s < 0.12 && path_s != null)
                {
                    currentPath = path_s;
                    pathc = currentPath.GetComponent<PathCreator>();
                    pathg = currentPath.GetComponent<PathGenerator>();
                    currentPosition = pathc.path.GetClosestDistanceAlongPath(transform.position);
                    setPathEnds();
                    //Do Something here that resets distance travelled in order to make it work.
                    if (currentPosition < 1)
                    {
                        carModel.transform.Rotate(carModel.transform.rotation.x, carModel.transform.rotation.y, carModel.transform.rotation.z - 180f, Space.Self);
                    }
                }
                else if (dist_f < 0.12 && path_f != null)
                {
                    currentPath = path_f;
                    pathc = currentPath.GetComponent<PathCreator>();
                    pathg = currentPath.GetComponent<PathGenerator>();
                    currentPosition = pathc.path.GetClosestDistanceAlongPath(transform.position);
                    setPathEnds();
                    //Do Something here that resets distance travelled in order to make it work.
                    if (currentPosition > 1)
                    {
                        carModel.transform.Rotate(carModel.transform.rotation.x, carModel.transform.rotation.y, carModel.transform.rotation.z + 180f, Space.Self);
                    }
                }
                switched = true;
            }
            transform.position = pathc.path.GetPointAtDistance(currentPosition, endOfPathInstruction);
            transform.rotation = pathc.path.GetRotationAtDistance(currentPosition, endOfPathInstruction);

            Debug.Log("Car: " + Time.deltaTime + "\n");

        }
    }

    void setPathEnds()
    {
        path_s = pathg.path_s;
        path_f = pathg.path_f;
    }

    void OnPathChanged()
    {
        currentPosition = pathc.path.GetClosestDistanceAlongPath(transform.position);
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
        else if (other.gameObject.CompareTag("Train") && other != this.gameObject)
        {
            other.gameObject.GetComponent<Pather>().attached = true;
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
