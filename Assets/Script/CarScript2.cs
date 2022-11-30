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
    MeshCollider m;

    bool hasJoint;
    public float start;
    public float rate = 10;

    public float x, y, z;
    public float zoffset;
    public bool frontCon;
    public bool rearCon;

    public float distanceTravelled;
    AudioManagerTrainAndCarScript audioScript;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        pathGen = path_Ben.GetComponent<PathGenerator>();
        pathCreator = path_Ben.GetComponent<PathCreator>();
        m = gameObject.GetComponent<MeshCollider>();
        m.enabled = false;
        float startdist = pathCreator.path.GetClosestDistanceAlongPath(pathCreator.path.GetPointAtTime(start));
        Vector3 temp = pathCreator.path.GetPointAtDistance(startdist);
        rb.position = new Vector3(temp.x, temp.y, zoffset);
        rb.rotation = pathCreator.path.GetRotationAtDistance(startdist) * Quaternion.Euler(x, y, z);
        m.enabled = true;
        audioScript = gameObject.GetComponent<AudioManagerTrainAndCarScript>();
    }

    // Update is called once per frame
    void Update()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(rb.position);

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
        rb.AddForce((new Vector3(pos.x, pos.y, zoffset) - rb.position) * 100, ForceMode.VelocityChange); //Force Keeping Train on Track
        //rb.MovePosition(new Vector3(pos.x, pos.y, zoffset));
        Quaternion rot = pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position)) * Quaternion.Euler(x, y, z);
        if (Quaternion.Angle(rb.rotation, rot) < Quaternion.Angle(rb.rotation, rot * Quaternion.Euler(0, 0, 180)))
        {
            Quaternion tempq = rot * Quaternion.Inverse(rb.rotation);
            rb.AddTorque(tempq.x, tempq.y, tempq.z);
        }
        else
        {

            Quaternion tempq = (rot * Quaternion.Euler(0, 0, 180)) * Quaternion.Inverse(rb.rotation);
            rb.AddTorque(tempq.x, tempq.y, tempq.z);
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        //This is what I get for making two Scripts
        if (collision.gameObject.GetComponent<Rigidbody>() != null && !frontCon && transform.InverseTransformPoint(collision.contacts[0].point).y > 0)
        {
            HingeJoint j;
            j = gameObject.AddComponent<HingeJoint>();
            j.axis = -transform.forward;
            j.anchor = transform.InverseTransformPoint(collision.contacts[0].point);
            j.connectedBody = collision.rigidbody;
            //j.connectedAnchor = transform.InverseTransformPoint(collision.contacts[1].point);
            j.enableCollision = false;
            j.enablePreprocessing = false;

            frontCon = true;
            audioScript.Connection();
        }
        else if (collision.gameObject.GetComponent<Rigidbody>() != null && !rearCon && transform.InverseTransformPoint(collision.contacts[0].point).y < 0)
        {
            HingeJoint j;
            j = gameObject.AddComponent<HingeJoint>();
            j.axis = transform.forward;
            j.anchor = transform.InverseTransformPoint(collision.contacts[0].point);
            j.connectedBody = collision.rigidbody;
            //j.connectedAnchor = transform.InverseTransformPoint(collision.contacts[1].point);
            j.enableCollision = false;
            j.enablePreprocessing = false;

            rearCon = true;
            audioScript.Connection();
        }

    }

}
