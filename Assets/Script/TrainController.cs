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

        // Update is called once per frame
        protected override void Update()
        {
            if (rb == null)
            {
                rb = gameObject.GetComponent<Rigidbody>();
            }

            if (hover && Input.GetMouseButtonDown(0)) {
                held = true;
            }

            if (!Input.GetMouseButton(0))
            {
                held = false;
            }

            if (held)
            {
                mouse = transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                Speed = Mathf.Clamp(Mathf.Abs(mouse.y) * 100, -MaxSpeed, MaxSpeed);
                MovementDirection = MovementDirectionMethods.FromInt((int)Mathf.Sign(-mouse.y));
                base.Update();
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
