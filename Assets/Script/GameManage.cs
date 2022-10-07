/**
 * Edited by - Brendan Singer, Brendan Griffith
 * HGD team 13 
 * Manage the levels of the game 
 * created 9/30/2022
 * last upadated 9/30/2022
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManage : MonoBehaviour
{
    /*
    * WARNING - For this to work, this script needs to be attached to
    * the game object that contains all of the train stations
    * numstations is where the number of train stations
    * on the level is stored
    * numCorrectRails is the number of rails at the correct station
    */
    int numStations;
    int numCorrectRails = 0;
    // Start is called before the first frame update
    //This counts the amount of child objects in the game object
    // "All TrainStations" in which then is stored in numStations
    void Start()
    {
        numStations = transform.childCount;
        Debug.Log(numStations);
    }

    //Public function, meaning it can be used anywhere,
    // in which updates how many rails are in the correct spot
    // and if the goal is met, then fires up GotoLevelselect();
    public void updateRailCount(int i)
    {
        numCorrectRails = numCorrectRails + i;
        if (numCorrectRails == numStations)
        {
            //Gotolevelselect();
        }
    }

    // Update is called once per frame
    // check if win conditions are met 
    // if they are call the gotolevelslect method
    private void Update()
    {
        // all of the componets of the train have to reach the desired destination 
        //Gotolevelselect();
    }

    // Load the levelselctscence 
    private void Gotolevelselect()
    {
        SceneManager.LoadScene("Level Selection");
    }

}
