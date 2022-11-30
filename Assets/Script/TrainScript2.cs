using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class TrainScript2 : MonoBehaviour
    {
        public GameManager gm;
        public GameObject path;
        GameObject prevPath;
        public GameObject[] paths;
        PathCreator pathCreator;
        public PathGenerator pathGen;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public float distanceTravelled;
        Vector3 moveVec;
        public Rigidbody rb;
        MeshCollider m;

        public float smooth;
        float train_speed;
        public bool lockout;
        AudioManagerTrainAndCarScript audioScript;


        //I added
        public float x;
        public float y;
        public float z;

        public float start;
        public float prevDist;
        public float diff;
        public float zoffset;

        

        bool hasJoint;
        bool held;
        bool hover;

        public bool frontCon;
        public bool rearCon;

        Vector3 test;

        void Start() {
            pathCreator = path.GetComponent<PathCreator>();
            pathGen = path.GetComponent<PathGenerator>();
            m = gameObject.GetComponent<MeshCollider>();
           m.enabled = false;
            if(gm == null)
            {
                gm = FindObjectOfType<GameManager>();
            }

            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
            //I added
            rb = gameObject.GetComponent<Rigidbody>();
            float startdist = pathCreator.path.GetClosestDistanceAlongPath(pathCreator.path.GetPointAtTime(start));
            Vector3 temp = pathCreator.path.GetPointAtDistance(startdist);
            rb.position = new Vector3(temp.x, temp.y, zoffset);
            rb.rotation = pathCreator.path.GetRotationAtDistance(startdist) * Quaternion.Euler(x, y, z);
            prevDist = startdist;
            distanceTravelled = prevDist;
            m.enabled = true;
            audioScript = gameObject.GetComponent<AudioManagerTrainAndCarScript>();
        }

        void Update()
        {
            
            //If mouse is held on train, perform all this checking
            if (hover && Input.GetMouseButtonDown(0))
            {
                held = true;
                distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(rb.position);
            }
            //If clicked, separate the cars
            //Not Finished, Holy is this super fucking sketchy
            if(hover && Input.GetMouseButtonDown(1))
            {
                HingeJoint[] hinges = gameObject.GetComponents<HingeJoint>();
                foreach (HingeJoint hinge in hinges)
                {
                    
                    HingeJoint[] others = hinge.connectedBody.gameObject.GetComponents<HingeJoint>();
                    foreach(HingeJoint other in others)
                    {
                        if (GameObject.ReferenceEquals(rb.gameObject, other.connectedBody.gameObject))
                        {
                            Destroy(other);
                        }
                    }

                    if (hinge.connectedBody.gameObject.GetComponent<TrainScript2>() != null)
                    {
                        hinge.connectedBody.gameObject.GetComponent<TrainScript2>().frontCon = false;
                        hinge.connectedBody.gameObject.GetComponent<TrainScript2>().rearCon = false;
                    } else if(hinge.connectedBody.gameObject.GetComponent<CarScript2>() != null)
                    {
                        hinge.connectedBody.gameObject.GetComponent<CarScript2>().frontCon = false;
                        hinge.connectedBody.gameObject.GetComponent<CarScript2>().rearCon = false;
                    }
                    Destroy(hinge);
                }
                frontCon = false;
                rearCon = false;
            }
            
            if(!Input.GetMouseButton(0))
            {
                held = false;
                rb.isKinematic = false;
                //rb.velocity = Vector3.zero;
            }
            
            if (held)
            {
                rb.isKinematic = true;
                //Debug.Log(pathCreator.path.GetClosestTimeOnPath(test) + " " + pathCreator.path.GetClosestTimeOnPath(rb.position));

                //NOTE TO FUTURE SELF: DO THAT THING YOU FIGURED OUT WITH THE ACCELERATOR METHOD
                /*New Steps
                1. Treat Speed like a variable and build it into an acceleration curve
                2. Use that acceleration curve to build it into a similar loop to previous iterations of the God forsaken project
                3. Get it to apply beyond the end of the path by doing a calculation where if the distance calculated goes beyond the end of the path,
                switch Tracks and add the remainder to it.
                3a. This requires figuring out the distance at the end of the path and the "predicted" path. Should work if I just do a stack of point->distance
                This creates a bit of overhead where we have to make a new calculation
                4.This calculation needs to figure out if the new path is starting at the beginning or the end
                5. It then adds or subtracts the diference accordingly
                6. Ideally, this keeps the train off of the weird middle point, unless you park it there on purpose EtHaNNN
                7. End Goal: Give Test a coordinate that is both accurate to the track and not speed limiting if the train is going beyond the track
                */

                //Step 1: Calculate the Train speed and Distance Traveled

                //Way to Math Goofball, What do we actually do here?
                //Well I'll tell you, we simply figure if the path we need is the one we are on or otherwise
                GameObject temp = CalculatePath();
                float dist_end = pathCreator.path.GetClosestDistanceAlongPath(pathCreator.path.GetPoint(pathCreator.path.localPoints.Length - 1));
                float dist_start = pathCreator.path.GetClosestDistanceAlongPath(pathCreator.path.GetPoint(0));
                Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if (GameObject.ReferenceEquals(path, temp))
                {
                    train_speed = Mathf.Clamp((pathCreator.path.GetClosestDistanceAlongPath(mouse) - distanceTravelled) * smooth, -speed, speed);
                } else if (GameObject.ReferenceEquals(pathGen.path_s, temp))
                {
                    //We need to find the total distance of this and the adjacent path in order to properly make the momentum work
                    //This means we need to figure out which end of the path we are hitting and then find the distance required
                    float tot_dist = distanceTravelled;
                    PathCreator path_sc = pathGen.path_s.GetComponent<PathCreator>();
                    if (Vector3.Distance(path_sc.path.GetPoint(0), pathCreator.path.GetPoint(0)) == 0)//The next path starts at start
                    {
                        tot_dist += path_sc.path.GetClosestDistanceAlongPath(mouse);
                    } else
                    {
                        //next path starts at end
                        tot_dist += (path_sc.path.GetClosestDistanceAlongPath(path_sc.path.GetPoint(path_sc.path.localPoints.Length - 1)) - path_sc.path.GetClosestDistanceAlongPath(mouse));
                    }
                    
                    train_speed = Mathf.Clamp(-tot_dist * smooth, -speed, speed);
                } else if (GameObject.ReferenceEquals(pathGen.path_f, temp)) 
                {

                    float tot_dist = distanceTravelled;
                    PathCreator path_sc = pathGen.path_f.GetComponent<PathCreator>();
                    if (Vector3.Distance(path_sc.path.GetPoint(0), pathCreator.path.GetPoint(0)) == 0)//The next path starts at start
                    {
                        tot_dist += path_sc.path.GetClosestDistanceAlongPath(mouse);
                    }
                    else
                    {
                        //next path starts at end
                        tot_dist += (path_sc.path.GetClosestDistanceAlongPath(path_sc.path.GetPoint(path_sc.path.localPoints.Length - 1)) - path_sc.path.GetClosestDistanceAlongPath(mouse));
                    }

                    train_speed = Mathf.Clamp(tot_dist * smooth, -speed, speed);
                } else if (GameObject.ReferenceEquals(pathGen.path_s, ClosestPathConnection(temp)))
                {
                    train_speed = -speed;
                } else if (GameObject.ReferenceEquals(pathGen.path_f, ClosestPathConnection(temp)))
                {
                    train_speed = speed;
                } else
                {
                    Debug.Log("How did we get here?");
                }
                
                distanceTravelled += train_speed * Time.deltaTime;

                //Step 2: Get it to apply the extra value to the end of the path
                
                if (distanceTravelled > dist_end)
                {
                    //Handle the situation where the train will go beyond the end of the path
                    //Step 3a: Take the remaining distance and apply it to the new distance traveled
                    if (pathGen.path_f == null)
                    {
                        distanceTravelled = dist_end;
                    } else
                    {
                        float newdist = Mathf.Abs(distanceTravelled - dist_end); //Finds the distance past the end of the path
                        path = pathGen.path_f;
                        pathCreator = path.GetComponent<PathCreator>();
                        pathGen = path.GetComponent<PathGenerator>();
                        //Depends on which end of the path we have landed on
                        if (pathCreator.path.GetClosestDistanceAlongPath(rb.position) < .1)
                        {
                            distanceTravelled = 0 + newdist;
                        }
                        else
                        {
                            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(rb.position) - newdist;
                        }
                    }
                    
                } else if (distanceTravelled < dist_start)
                {
                    if(pathGen.path_s == null)
                    {
                        distanceTravelled = dist_start;
                    } else
                    {
                        float newdist = Mathf.Abs(distanceTravelled - dist_start); //Finds the distance past the end of the path
                        path = pathGen.path_s;
                        pathCreator = path.GetComponent<PathCreator>();
                        pathGen = path.GetComponent<PathGenerator>();
                        //Depends on which end of the path we have landed on
                        if (pathCreator.path.GetClosestDistanceAlongPath(rb.position) < .1)
                        {
                            distanceTravelled = 0 + newdist;
                        }
                        else
                        {
                            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(rb.position) - newdist;
                        }
                    }
                    
                } else
                {
                    //As Gandhi Most Definitely Said, "Let it Rock"
                }
                

                test = pathCreator.path.GetPointAtDistance(distanceTravelled);
            }
            else
            {
                
                if (Vector3.Distance(pathCreator.path.GetClosestPointOnPath(rb.position), pathCreator.path.GetPointAtTime(1f)) < .01 && pathGen.path_f != null)
                {
                    path = pathGen.path_f;
                    pathGen = path.GetComponent<PathGenerator>();
                    pathCreator = path.GetComponent<PathCreator>();

                }
                else if (Vector3.Distance(pathCreator.path.GetClosestPointOnPath(rb.position), pathCreator.path.GetPointAtTime(0f)) < .01 && pathGen.path_s != null)
                {
                    path = pathGen.path_s;
                    pathGen = path.GetComponent<PathGenerator>();
                    pathCreator = path.GetComponent<PathCreator>();
                }
                
            }
            
        }

        void FixedUpdate()
        {
            
            //rb.velocity = rb.rotation.eulerAngles * speed * Time.deltaTime;

            //Use these for when held and train is kinematic
            if (rb.isKinematic)
            {
                //Switch this to rotate towards in order to create the motion that we want.
                //Maybe figure out how to average rotation across a corner to create the look that the tracks are good
                //Debug.Log(rb.gameObject.name);
                Quaternion rot = pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position)) * Quaternion.Euler(x, y, z);
                if (Quaternion.Angle(rb.rotation, rot) < Quaternion.Angle(rb.rotation, rot * Quaternion.Euler(0, 0, 180)))
                {
                    rb.MoveRotation(rot);
                } else
                {
                    rb.MoveRotation(rot * Quaternion.Euler(0, 0, 180));
                }
                
                //rb.MovePosition(pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction));
                //rb.MovePosition(test);

                rb.MovePosition(new Vector3(test.x, test.y, zoffset));

                
                //rb.AddForce((new Vector3(test.x, test.y, zoffset) - rb.position) * 10, ForceMode.VelocityChange);

                //Do a check here to see if it needs to move onto a different path


            } else
            {
                //When not kinematic, i.e. when being pulled

               Vector3 pos = pathCreator.path.GetPointAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position));
                //Debug.Log(rb.gameObject.name + " " + (new Vector3(pos.x, pos.y, zoffset) - rb.position));
                //rb.AddForce((new Vector3(pos.x, pos.y, zoffset) - rb.position) * 100, ForceMode.VelocityChange); //Force Keeping Train on Track
                rb.MovePosition(new Vector3(pos.x, pos.y, zoffset));
                /*
                Quaternion rot = pathCreator.path.GetRotationAtDistance(pathCreator.path.GetClosestDistanceAlongPath(rb.position)) * Quaternion.Euler(x, y, z);
                if (Quaternion.Angle(rb.rotation, rot) < Quaternion.Angle(rb.rotation, rot * Quaternion.Euler(0, 0, 180)))
                {
                    rb.MoveRotation(rot);
                }
                else
                {
                    rb.MoveRotation(rot * Quaternion.Euler(0, 0, 180));
                }*/

            }
            //New Plan: Create a Force PID loop for both rotation and position
            //Then make it that all that happens between held and not held is that the target changes
            //Tune the loop accordingly to follow our implied controls, and we are in business

            //Debug.Log(rb.gameObject.name + " " + held);
            AdjustDistance();
            
        }
        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
       void OnPathChanged() {
            //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
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
                j.enableCollision = false;
                j.enablePreprocessing = false;

                rearCon = true;
                audioScript.Connection();
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

        GameObject CalculatePath()
        {
            if (paths.Length == 0)
            {
                paths = gm.paths;
            }

            float shortestDist = Mathf.Infinity;
            GameObject shortest = paths[0];
            foreach (GameObject path in paths)
            {
                PathCreator temp = path.GetComponent<PathCreator>();
                float calc = Vector3.Distance(temp.path.GetPointAtDistance(temp.path.GetClosestDistanceAlongPath(Camera.main.ScreenToWorldPoint(Input.mousePosition))), Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (calc < shortestDist)
                {
                    shortest = path;
                    shortestDist = calc;
                }


            }
            return shortest;
        }

        GameObject ClosestPathConnection(GameObject desired)
        {
            float f = Mathf.Infinity;
            float s = Mathf.Infinity;

            if (GameObject.ReferenceEquals(path, desired))
            {
                return path;
            }

            if (pathGen.path_f != null)
            {
                f = pathGen.path_f.GetComponent<PathGenerator>().findClosestPath(desired, path);
            }

            if (pathGen.path_s != null)
            {
                s = pathGen.path_s.GetComponent<PathGenerator>().findClosestPath(desired, path);
            }

            if (f <= s)
            {
                return pathGen.path_f;
            }
            else if (f > s)
            {
                return pathGen.path_s;
            }
            else
            {
                return null;
            }
        }

        void AdjustDistance()
        {
            if (rb.isKinematic)
            {
                if (GameObject.ReferenceEquals(prevPath, path)) {
                    gm.totalDistanceTravelled += Mathf.Abs(pathCreator.path.GetClosestDistanceAlongPath(rb.position) - prevDist);
                    //display new total distance travelled
                    gm.totalDistanceTravelled = Mathf.Round(gm.totalDistanceTravelled * 10) / 10;
                    gm.ChangeText(gm.totalDistanceTravelled);
                }
                else
                {
                    //Do something here for when we jump to different track pieces
                }
            }
            prevDist = pathCreator.path.GetClosestDistanceAlongPath(rb.position);
            prevPath = path;
        }
    }
}