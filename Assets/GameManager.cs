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
    }

    public void ChangeText(float addDistance)
    {
        distanceText.text = "Distance Traveled: " + addDistance;
        distance = addDistance;
    }

    //allows the vistory screen to display tyhe last recorded distance
    public void Victory(int i){
        correctRails = correctRails + i;
        Debug.Log(correctRails + " Ding");
        if(correctRails == numStations){
            Time.timeScale = 0;
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


