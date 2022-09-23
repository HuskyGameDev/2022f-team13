using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//WARNING - This script is for detecting 2D objects
//THIS WILL NOT WORK FOR 3D
public class RailDetector : MonoBehaviour
{
    //Variable for the tag we are using for each individual car that we can change easily
    [SerializeField] string railType = "Gold";

    //Variable to use for the victory condition. When the right rail
    //is in this station, the variable should update to true
    //If the correct rail is not at this station, it will be false
    bool correctRail = false;


    //When something enters the proximity check, it checks for the right tag
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == railType)
        {
            correctRail = true;
            Debug.Log("2D object with tag '" + railType + "': " + collision + " detected");
        }


    }


    //When the rail with the correct tag leaves, the victory condition
    //is updated, telling the game the rail is no longer at this station
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == railType)
        {
            correctRail = false;
            Debug.Log("2D object with tag '" + railType + "': " + collision + " has left range");

        }
    }
}
