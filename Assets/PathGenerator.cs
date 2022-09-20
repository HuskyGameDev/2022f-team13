using UnityEngine;

namespace PathCreation.Examples
{
    // Example of creating a path at runtime from a set of points.

    [RequireComponent(typeof(PathCreator))]
    public class PathGenerator : MonoBehaviour
    {

        public bool closedLoop = false;
        public Transform[] waypoints;
        int previouslength;

        void Start()
        {
            if (waypoints.Length > 0)
            {
                // Create a new bezier path from the waypoints.
                BezierPath bezierPath = new BezierPath(waypoints, closedLoop, PathSpace.xy);
                GetComponent<PathCreator>().bezierPath = bezierPath;
                previouslength = waypoints.Length;
            }
        }

        void Update()
        {
            if(waypoints.Length != previouslength)
            {
                BezierPath bezierPath = new BezierPath(waypoints, closedLoop, PathSpace.xy);
                GetComponent<PathCreator>().bezierPath = bezierPath;
            }

            previouslength = waypoints.Length;
        }
    }
}