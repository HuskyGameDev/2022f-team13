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

        //Track Generator Variables
        public GameObject prefab;
        public GameObject holder;
        public float spacing = 3;
        const float minSpacing = .1f;

        private PathCreator pathc;

        void Start()
        {
            if (waypoints.Length > 0)
            {
                // Create a new bezier path from the waypoints.
                BezierPath bezierPath = new BezierPath(waypoints, closedLoop, PathSpace.xy);
                pathc = GetComponent<PathCreator>();
                pathc.bezierPath = bezierPath;
                previouslength = waypoints.Length;
            }

            gm = GameObject.FindWithTag("The manager").GetComponent<GameManager>();
            Generate();
           
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

        void Generate()
        {
            if (pathc != null && prefab != null && holder != null)
            {
                DestroyObjects();

                VertexPath path = pathc.path;

                spacing = Mathf.Max(minSpacing, spacing);
                float dst = 0;

                while (dst < path.length)
                {
                    Vector3 point = path.GetPointAtDistance(dst);
                    Quaternion rot = path.GetRotationAtDistance(dst);
                    Instantiate(prefab, point, rot, holder.transform);
                    dst += spacing;
                }
            }
        }

        void DestroyObjects()
        {
            int numChildren = holder.transform.childCount;
            for (int i = numChildren - 1; i >= 0; i--)
            {
                DestroyImmediate(holder.transform.GetChild(i).gameObject, false);
            }
        }

        protected void PathUpdated()
        {
            if (pathc != null)
            {
                Generate();
            }
        }
    }
}