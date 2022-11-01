using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class TrainScript2 : MonoBehaviour
    {
        public GameObject path;
        PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public float distanceTravelled;
        Vector3 moveVec;
        public Rigidbody rb;

        //I added
        public float x;
        public float y;
        public float z;

        public float start;
        public float prevDist;
        public float diff;

        bool hasJoint;
        bool held;
        bool hover;

        void Start() {
            pathCreator = path.GetComponent<PathCreator>();
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
            //I added
            rb = gameObject.GetComponent<Rigidbody>();
            float startdist = pathCreator.path.GetClosestDistanceAlongPath(pathCreator.path.GetPointAtTime(start));
            rb.position = pathCreator.path.GetPointAtDistance(startdist);
            rb.rotation = pathCreator.path.GetRotationAtDistance(startdist) * Quaternion.Euler(x, y, z);
            prevDist = startdist;
            distanceTravelled = prevDist;
        }

        void Update()
        {

            //If mouse is held on train, perform all this checking
            if (hover && Input.GetMouseButtonDown(0))
            {
                held = true;
            }
            if(!Input.GetMouseButton(0))
            {
                held = false;
                rb.isKinematic = false;
            }
            
            if (held)
            {
                rb.isKinematic = true;
                if (pathCreator != null)
                {
                    distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    diff = distanceTravelled - prevDist;
                    if (Mathf.Abs(diff) > speed * Time.deltaTime)
                    {
                        //Speed limited movement

                        Debug.Log("Too Far");
                        if (diff > 0)
                        {
                            distanceTravelled = prevDist + speed * Time.deltaTime;
                        }
                        else
                        {
                            distanceTravelled = prevDist - speed * Time.deltaTime;
                        }

                    }
                    else
                    {
                        Debug.Log("Mouse Speed");
                    }
                    prevDist = distanceTravelled;

                }

            }
            else
            {
                distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(rb.position);
            }
        }

        void FixedUpdate()
        {
            //rb.velocity = rb.rotation.eulerAngles * speed * Time.deltaTime;

            //Use these for when held and train is kinematic
            if (rb.isKinematic)
            {
                rb.MoveRotation(pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position), endOfPathInstruction) * Quaternion.Euler(x, y, z));
                rb.MovePosition(pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction));
            } else
            {
                //When not kinematic, i.e. when being pulled

                //Calculate some bull to make this rotate with add torque so it looks pretty

                //rb.AddTorque(Quaternion.FromToRotation(rb.rotation.eulerAngles, pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position)).eulerAngles).eulerAngles * 10, ForceMode.Impulse);
                rb.MoveRotation(pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position), endOfPathInstruction) * Quaternion.Euler(x, y, z));
                //rb.MoveRotation(pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position), endOfPathInstruction) * Quaternion.Euler(x, y, z));
                rb.AddForce((pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) - rb.position) * 100, ForceMode.VelocityChange); //Force Keeping Train on Track
            }
            


            

        }
        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
       void OnPathChanged() {
            //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<Rigidbody>() != null && !hasJoint)
            {
                HingeJoint j;
                j = gameObject.AddComponent<HingeJoint>();
                j.connectedBody = collision.rigidbody;
                j.axis = new Vector3(0, 0, 1);
                j.enableCollision = true;

                hasJoint = true;
            }
        }

        void OnMouseOver()
        {
            hover = true;
        }

        void OnMouseExit()
        {
            hover = false;
        }
    }
}