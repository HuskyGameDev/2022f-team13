using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//WARNING - This script is for detecting 2D objects
//THIS WILL NOT WORK FOR 3D
public class RailDetector : MonoBehaviour
{
    //Variable for the tag we are using for each individual car that we can change easily
    [SerializeField] string railType = "Gold";

    GameObject gameManager;

    //This finds the gameobject that is holding all of the stations
    //We need this to access the gamemanage script so we can
    //update winning conditions
    private void Start()
    {
        gameManager = GameObject.Find("GameManager");
    }


    //When something enters the proximity check, it checks for the right tag
    //It then updates the winning conditions in the gamemanage script
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == railType)
        {
            gameManager.GetComponent<GameManager>().Victory(1);
            Debug.Log("2D object with tag '" + railType + "': " + collision + " detected");
        }


    }


    //When the rail with the correct tag leaves, the victory condition
    //is updated, telling the game the rail is no longer at this station
    //It then updates the winning conditions in the gamemanage script
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == railType)
        {
            gameManager.GetComponent<GameManager>().Victory(-1);
            Debug.Log("2D object with tag '" + railType + "': " + collision + " has left range");

        }
    }
}
