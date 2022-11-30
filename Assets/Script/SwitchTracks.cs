using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;

public class SwitchTracks : MonoBehaviour
{
    public GameObject upper;
    public GameObject lower;
    public GameObject entrance;
    public List<TrainScript2> trains;
    public List<CarScript2> cars;
    private PathGenerator enter;
    public float z1;
    public float z2;
    public float speed;
    float timeCount;
    public float tempz;
    public float newVal;
    float prevTime;

    public bool open = true;
    public bool prevOpen = true;
    AudioManagerSwitch audioScript;
    // Start is called before the first frame update
    void Start()
    {
        audioScript = gameObject.GetComponent<AudioManagerSwitch>();
        Component[] paths;
        paths = transform.parent.GetComponentsInChildren<PathGenerator>();

        //Assumption because something went wrong if not the case.
        upper = paths[0].gameObject;
        lower = paths[1].gameObject;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z2);
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Train");
        trains = new List<TrainScript2>();
        foreach(GameObject t in temp)
        {
            trains.Add(t.GetComponent<TrainScript2>());
        }

        temp = GameObject.FindGameObjectsWithTag("Coal");
        cars = new List<CarScript2>();
        foreach (GameObject t in temp)
        {
            cars.Add(t.GetComponent<CarScript2>());
        }

        tempz = z2;
    }

    // Update is called once per frame
    void Update()
    {
        if (entrance == null && upper.GetComponent<PathGenerator>().path_s != null)
        {
            entrance = upper.GetComponent<PathGenerator>().path_s;
            enter = entrance.GetComponent<PathGenerator>();
        } else if (enter != null)
        {
            //Put in a Lockout to prevent the tracks from switching while the trains are on or very near the entrance point
            open = true;
            int val = 0;
            float closest = Mathf.Infinity;
            foreach (CarScript2 c in cars)
            {
                //Further Subdivide these for each situation to improve the problem.
                //I forgot an edge case that still allows things to hit the fan.
                if (GameObject.ReferenceEquals(c.path_Ben, upper) && c.distanceTravelled < 1.7)
                {
                    open = false;
                    if (prevOpen)
                    {
                        prevTime = Time.time;
                    }

                    if (GameObject.ReferenceEquals(enter.path_f, lower))
                    {
                        enter.path_f = upper;
                        audioScript.Switching();
                        tempz = z1;
                    }
                    else if (GameObject.ReferenceEquals(enter.path_s, lower))
                    {
                        enter.path_s = upper;
                        audioScript.Switching();
                        tempz = z1;
                    }

                } else if (GameObject.ReferenceEquals(c.path_Ben, lower) && c.distanceTravelled < 1.7)
                {
                    open = false;
                    if (prevOpen)
                    {
                        prevTime = Time.time;
                    }

                    if (GameObject.ReferenceEquals(enter.path_f, upper))
                    {
                        enter.path_f = lower;
                        audioScript.Switching();
                        tempz = z2;
                    }
                    else if (GameObject.ReferenceEquals(enter.path_s, upper))
                    {
                        enter.path_s = lower;
                        audioScript.Switching();
                        tempz = z2;
                    }
                }
            }
            foreach (TrainScript2 t in trains)
            {
                if (GameObject.ReferenceEquals(t.path, upper) && t.distanceTravelled < 1.7)
                {
                    open = false;
                    if (prevOpen)
                    {
                        prevTime = Time.time;
                    }

                    if (GameObject.ReferenceEquals(enter.path_f, lower))
                    {
                        enter.path_f = upper;
                        audioScript.Switching();
                        tempz = z1;
                    }
                    else if (GameObject.ReferenceEquals(enter.path_s, lower))
                    {
                        enter.path_s = upper;
                        audioScript.Switching();
                        tempz = z1;
                    }

                } else if (GameObject.ReferenceEquals(t.path, lower) && t.distanceTravelled < 1.7)
                {
                    open = false;
                    if (prevOpen)
                    {
                        prevTime = Time.time;
                    }

                    if (GameObject.ReferenceEquals(enter.path_f, upper))
                    {
                        enter.path_f = lower;
                        audioScript.Switching();
                        tempz = z2;
                    }
                    else if (GameObject.ReferenceEquals(enter.path_s, upper))
                    {
                        enter.path_s = lower;
                        audioScript.Switching();
                        tempz = z2;
                    }
                }
            }

            prevOpen = open;
            if (open)
            {
                if (GameObject.ReferenceEquals(enter.path_f, upper))
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z1), timeCount * speed);
                    tempz = z1;
                }
                else if (GameObject.ReferenceEquals(enter.path_f, lower))
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z2), timeCount * speed);
                    tempz = z2;
                }
                else if (GameObject.ReferenceEquals(enter.path_s, upper))
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z1), timeCount * speed);
                    tempz = z1;
                }
                else if (GameObject.ReferenceEquals(enter.path_s, lower))
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z2), timeCount * speed);
                    tempz = z2;
                }
            } else
            {
                Debug.Log(Time.time - prevTime);
                newVal = tempz + (Mathf.Sin((Time.time - prevTime) * 10f) * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, newVal), timeCount * speed);
            }
            
        }

       

        
        timeCount += Time.deltaTime;
    }

    //I need to generalize this before problems start
    void OnMouseDown()
    {
        Debug.Log("Switching Tracks\n");
        if (open)
        {
            if (GameObject.ReferenceEquals(enter.path_f, upper))
            {
                enter.path_f = lower;
                audioScript.Switching();
            }
            else if (GameObject.ReferenceEquals(enter.path_f, lower))
            {
                enter.path_f = upper;
                audioScript.Switching();
            }
            else if (GameObject.ReferenceEquals(enter.path_s, upper))
            {
                enter.path_s = lower;
                audioScript.Switching();
            }
            else if (GameObject.ReferenceEquals(enter.path_s, lower))
            {
                enter.path_s = upper;
                audioScript.Switching();
            }
        } else
        {
            
        }
        


        
    }
}
