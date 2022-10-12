using UnityEngine;
using PathCreation;

namespace PathCreation.Examples
{
    // Example of creating a path at runtime from a set of points.

    [RequireComponent(typeof(PathCreator))]
    public class PathGenerator : MonoBehaviour
    {

        public bool closedLoop = false;
        public Transform[] waypoints;
        int previouslength;
        private GameManager gm;
        public GameObject[] allPaths;
        public GameObject path_s;
        public GameObject path_f;

        void Start()
        {
            if (waypoints.Length > 0)
            {
                // Create a new bezier path from the waypoints.
                BezierPath bezierPath = new BezierPath(waypoints, closedLoop, PathSpace.xy);
                GetComponent<PathCreator>().bezierPath = bezierPath;
                previouslength = waypoints.Length;
            }

            gm = GameObject.FindWithTag("The manager").GetComponent<GameManager>();
           
        }

        void Update()
        {
            



            if (allPaths.Length <= 0 && gm.paths.Length > 0)
            {
                //Iterate through all the waypoints in the list of all paths and determine if any of them match waypoints on this path.
                allPaths = gm.paths;
                //For all paths
                //For all their waypoints
                //For all my waypoints
                foreach (GameObject path in allPaths)
                {
                    foreach (Transform t in path.GetComponent<PathGenerator>().waypoints)
                    {
                        if (this.waypoints[0].position.Equals(t.position) && !GameObject.ReferenceEquals(path, this.gameObject))
                        {
                            //Find the "FIRST" point connection
                            path_s = path;
                        }

                        if (this.waypoints[this.waypoints.Length - 1].position.Equals(t.position) && !GameObject.ReferenceEquals(path, this.gameObject))
                        {
                            //Find the "FIRST" point connection
                            path_f = path;
                        }
                    }
                }
            }
        }
    }
}