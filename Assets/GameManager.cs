using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text distanceText;
    public VictoryScreen VictoryScreen;
    public float distance = 1;
    public bool bricks;
    public bool logs;
    public GameObject[] paths;

    private GameObject allStations;
    private int numStations;
    private int correctRails;
    public float totalDistanceTravelled = 0;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1; //unpauses the game if it was paused
        distanceText.text = "Distance Traveled: 0";
        paths = GameObject.FindGameObjectsWithTag("Path");
        allStations = GameObject.Find("All TrainStations");
        numStations = allStations.GetComponent<TrainStationCount>().numOfTrainStations();
        correctRails = 0;
        Debug.Log(numStations);

        //Time to do some order of operations bull in here in order to make the trains chill out
        //I have no clue how I intend to do it, but it seriously needs to be considered

    }

    public void ChangeText(float addDistance)
    {
        distanceText.text = "Distance Traveled: " + addDistance;
        distance = addDistance;
    }

    //allows the vistory screen to display tyhe last recorded distance
    public void Victory(int i){
        correctRails = correctRails + i;
        if(correctRails == numStations){
            Time.timeScale = 0;
            Debug.Log(distance);
            VictoryScreen.Setup(distance);
        }
    }

    public void Bricks(){
        bricks = true;
    }

        public void Logs(){
        logs = true;
    }
}


