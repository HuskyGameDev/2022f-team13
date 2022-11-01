﻿using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        float distanceTravelled;
        void Start() {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
        }

        void Update()
        {
            distanceTravelled += 5 * Time.deltaTime;
            if (pathCreator != null)
            {
                Vector3 thing = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                transform.position = new Vector3(thing.x, thing.y, -.5f);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.Euler(180, 90, 90);
            }
        }

        
        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged() {
            //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }
    }
}