using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;


public class CarScript2 : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject path_Ben;
    PathCreator pathCreator;
    PathGenerator pathGen;
    public EndOfPathInstruction endOfPathInstruction;

    bool hasJoint;
    public float start;
    public float rate = 10;

    public float x, y, z;
    // Start is called before the first frame update
    void Start()
    {

        rb = gameObject.GetComponent<Rigidbody>();
        pathGen = path_Ben.GetComponent<PathGenerator>();
        pathCreator = path_Ben.GetComponent<PathCreator>();
        float startdist = pathCreator.path.GetClosestDistanceAlongPath(pathCreator.path.GetPointAtTime(start));
        rb.position = pathCreator.path.GetPointAtDistance(startdist);
        rb.rotation = pathCreator.path.GetRotationAtDistance(startdist) * Quaternion.Euler(x, y, z);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(pathCreator.path.GetClosestPointOnPath(rb.position), pathCreator.path.GetPointAtTime(1f)) < .01 && pathGen.path_f != null)
        {
            path_Ben = pathGen.path_f;
            pathGen = path_Ben.GetComponent<PathGenerator>();
            pathCreator = path_Ben.GetComponent<PathCreator>();

        } else if (Vector3.Distance(pathCreator.path.GetClosestPointOnPath(rb.position), pathCreator.path.GetPointAtTime(0f)) < .01 && pathGen.path_s != null)
        {
            path_Ben = pathGen.path_s;
            pathGen = path_Ben.GetComponent<PathGenerator>();
            pathCreator = path_Ben.GetComponent<PathCreator>();
        }
    }

    void FixedUpdate()
    {
        Vector3 pos = pathCreator.path.GetClosestPointOnPath(rb.position);
        rb.AddForce((new Vector3(pos.x, pos.y, -1) - rb.position) * 100, ForceMode.VelocityChange); //Force Keeping Train on Track
    }

    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.GetComponent<Rigidbody>() != null && !hasJoint)
        {
            HingeJoint j;
            j = gameObject.AddComponent<HingeJoint>();
            j.axis = transform.forward;
            j.anchor = transform.InverseTransformPoint(collision.contacts[0].point);
            j.connectedAnchor = transform.InverseTransformPoint(collision.contacts[1].point);
            j.connectedBody = collision.rigidbody;
            j.enableCollision = true;
            j.enablePreprocessing = false;

            hasJoint = true;
        }
        
    }
}
