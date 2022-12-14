using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//WARNING - This script is for detecting 3D objects
//THIS WILL NOT WORK FOR 2D
public class Rail3DDetector : MonoBehaviour
{
    //Variable for the tag we are using for each individual car that we can change easily
    [SerializeField] string railType;
    GameObject gameManager;

    private int numberOfCol;

    //This finds the gameobject that is holding all of the stations
    //We need this to access the gamemanage script so we can
    //update winning conditions
    private void Start()
    {
        gameManager = GameObject.Find("GameManager");
        railType = this.gameObject.GetComponent<Type>().e.ToString();
        numberOfCol = 0;
    }

    //When something enters the proximity check3d, it checks for the right tag
    //It then updates the winning conditions in the gamemanage script
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collision");
        if (collision.gameObject.GetComponent<Type>().e.ToString() == railType)
        {
            Debug.Log("Passed Check");
            numberOfCol += 1;
            if (numberOfCol == 1)
            {
                gameManager.GetComponent<GameManager>().Victory(1);
                Debug.Log("3D object with tag '" + railType + "': " + collision + " detected");
            }
            
        }
        

    }


    //When the rail with the correct tag leaves, the victory condition
    //is updated, telling the game the rail is no longer at this station
    //It then updates the winning conditions in the gamemanage script
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.GetComponent<Type>().e.ToString() == railType)
        {
            numberOfCol -= 1;
            if (numberOfCol == 0)
            {
                gameManager.GetComponent<GameManager>().Victory(-1);
                Debug.Log("3D object with tag '" + railType + "': " + collision + " has left range");
            }
            

        }
    }
}
