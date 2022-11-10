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
        upper = this.transform.parent.Find("Upper").gameObject;
        lower = this.transform.parent.Find("Lower").gameObject;
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
            if (enter.path_f.name == "Upper")
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z1), timeCount * speed);
            }
            else if (enter.path_f.name == "Lower")
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, z2), timeCount * speed);
            }
        }

       

        
        timeCount += Time.deltaTime;
    }

    void OnMouseDown()
    {
        Debug.Log("Switching Tracks\n");
        
        if(enter.path_f.name == "Upper")
        {
            enter.path_f = lower;
        } else if (enter.path_f.name == "Lower")
        {
            enter.path_f = upper;
        }
        
    }
}
