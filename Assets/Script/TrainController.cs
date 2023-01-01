using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{
    public class TrainController : SplineController
    {
        //This is an extension of the spline Controller, Update the controllers in here
        [Section("Motor")]
        public float MaxSpeed = 5;

        [Section("Controls")]
        public Vector3 mouse;
        public Rigidbody rb = null;
        public bool hover = false;
        public bool held = false;

        [Section("Connections")]
        public TrainController front = null;
        public TrainController back = null;
        public float horizontalBound = .1f;
        public float verticalBound = .02f;
        public bool seen = false;

        // Update is called once per frame
        protected override void Update()
        {
            seen = false;
            //rb.isKinematic = false;
            if (rb == null)
            {
                rb = gameObject.GetComponent<Rigidbody>();
            }

            //Set the different behaviors for trains and everything else
            if (gameObject.tag == "Train")
            {
                if (hover && Input.GetMouseButtonDown(0))
                {
                    held = true;
                    rb.isKinematic = true;
                }

                if (!Input.GetMouseButton(0))
                {
                    held = false;
                    Speed = 0;
                    UpdateSpeeds();
                    if (front == null && back == null)
                    {
                        rb.isKinematic = false;
                    }
                }

                if (held)
                {
                    mouse = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                    Speed = Mathf.Clamp(Mathf.Abs(mouse.y) * 100, -MaxSpeed, MaxSpeed);
                    MovementDirection = MovementDirectionMethods.FromInt((int)Mathf.Sign(-mouse.y));

                    //Perform the update all speed actions in here, and maybe in the release state if those do not comply (Make it a function)
                    UpdateSpeeds();

                }
            } else
            {
                if (front == null && back == null)
                {
                    rb.isKinematic = false;
                }
            }

            //Handle The Possbility of disconnection
            

            base.Update();

        }

        void OnMouseOver()
        {
            hover = true;
        }

        void OnMouseExit()
        {
            hover = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            //Find the Train Controller of our collision
            TrainController temp = collision.gameObject.GetComponent<TrainController>();
            if(temp != null)
            {
                //Check the local coorcdinates to see if they are in the desired range for where we need them.
                Vector3 localTemp = transform.InverseTransformPoint(collision.GetContact(0).point);
                if(localTemp.x > -horizontalBound && localTemp.x < horizontalBound && (localTemp.y > verticalBound || localTemp.y < -verticalBound))
                {
                    if (localTemp.y > 0)
                    {
                        //Front
                        if (front == null)
                        {
                            front = temp;
                        }
                    }
                    else if (localTemp.y < 0)
                    {
                        //Back
                        if (back == null)
                        {
                            back = temp;
                        }
                    }
                }
                
            }
        }

        void UpdateSpeeds()
        {
            /*Steps:
            1. Iterate through each connection and change the speed
            2. Append each visited object to a list
            3. If the object is not on a list do not perform the action
            */
            seen = true;
            if (front != null && front.seen == false)
            {
                front.Speed = Speed;
                front.MovementDirection = MovementDirection;
                front.UpdateSpeeds();
            }

            if (back != null && back.seen == false)
            {
                back.Speed = Speed;
                back.MovementDirection = MovementDirection;
                back.UpdateSpeeds();
            }
        }


    }

}