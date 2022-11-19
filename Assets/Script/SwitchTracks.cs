using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation.Examples;

public class SwitchTracks : MonoBehaviour
{
    public GameObject upper;
    public GameObject lower;
    public GameObject entrance;
    private PathGenerator enter;
    public float z1;
    public float z2;
    public float speed;
    float timeCount;
    // Start is called before the first frame update
    void Start()
    {
        Component[] paths;
        paths = transform.parent.GetComponentsInChildren<PathGenerator>();

        //Assumption because something went wrong if not the case.
        upper = paths[0].gameObject;
        lower = paths[1].gameObject;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z2);
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
            if (GameObject.ReferenceEquals(enter.path_f, upper))
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z1), timeCount * speed);
            }
            else if (GameObject.ReferenceEquals(enter.path_f, lower))
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z2), timeCount * speed);
            }
            else if (GameObject.ReferenceEquals(enter.path_s, upper))
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z1), timeCount * speed);
            }
            else if (GameObject.ReferenceEquals(enter.path_s, lower))
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z2), timeCount * speed);
            }
        }

       

        
        timeCount += Time.deltaTime;
    }

    //I need to generalize this before problems start
    void OnMouseDown()
    {
        Debug.Log("Switching Tracks\n");
        
        if(GameObject.ReferenceEquals(enter.path_f, upper))
        {
            enter.path_f = lower;
        } else if (GameObject.ReferenceEquals(enter.path_f, lower))
        {
            enter.path_f = upper;
        } else if (GameObject.ReferenceEquals(enter.path_s, upper))
        {
            enter.path_s = lower;
        } else if (GameObject.ReferenceEquals(enter.path_s, lower))
        {
            enter.path_s = upper;
        }
        
    }
}
