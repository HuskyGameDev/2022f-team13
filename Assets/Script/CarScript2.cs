using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class CarScript2 : MonoBehaviour
{
    public Rigidbody rb;
    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;

    bool hasJoint;
    public float start;

    public float x, y, z;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        float startdist = pathCreator.path.GetClosestDistanceAlongPath(pathCreator.path.GetPointAtTime(start));
        rb.position = pathCreator.path.GetPointAtDistance(startdist);
        rb.rotation = pathCreator.path.GetRotationAtDistance(startdist) * Quaternion.Euler(x, y, z);
        //prevDist = startdist;
        //distanceTravelled = startdist;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.GetComponent<SpringJoint>() != null)
        {

        }

    }

    void FixedUpdate()
    {
        rb.MoveRotation(pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position), endOfPathInstruction) * Quaternion.Euler(x, y, z));
        rb.AddForce((pathCreator.path.GetPointAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position), endOfPathInstruction) - rb.position) * 100, ForceMode.VelocityChange); //Force Keeping Train on Track
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>() != null && !hasJoint)
        {
            SpringJoint j;
            j = gameObject.AddComponent<SpringJoint>();
            j.connectedBody = collision.rigidbody;
            j.enableCollision = true;

            hasJoint = true;
        }
    }
}
