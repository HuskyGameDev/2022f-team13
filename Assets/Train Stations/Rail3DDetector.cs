using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//WARNING - This script is for detecting 3D objects
//THIS WILL NOT WORK FOR 2D
public class Rail3DDetector : MonoBehaviour
{
    //Variable for the tag we are using for each individual car that we can change easily
    [SerializeField] string railType = "Gold";

    //Variable to use for the victory condition. When the right rail
    //is in this station, the variable should update to true
    //If the correct rail is not at this station, it will be false
    bool correctRail = false;


    //When something enters the proximity check3d, it checks for the right tag
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == railType)
        {
            correctRail = true;
            Debug.Log("3D object with tag '" + railType +"': " + collision + " detected");
        }
        

    }


    //When the rail with the correct tag leaves, the victory condition
    //is updated, telling the game the rail is no longer at this station
    private void OnTriggerExit(Collider collision)
    {
        if (collision.tag == railType)
        {
            correctRail = false;
            Debug.Log("3D object with tag '" + railType + "': " + collision + " has left range");

        }
    }
}
