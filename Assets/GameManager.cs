using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text distanceText;
    public VictoryScreen VictoryScreen;
    public float distance = 1;
    //public bool bricks;
    //public bool logs;
    public GameObject[] paths;

    private GameObject allStations;
    private int numStations;
    private int correctRails;
    public float totalDistanceTravelled = 0;
    [SerializeField] public int levelNumber; //set this to level number in GM
    public int overallScore;
    public int pointsRecieved = 0; 


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

    //allows the victory screen to display the last recorded distance
    public void Victory(int i){
        correctRails = correctRails + i;
        if(correctRails == numStations){
            Time.timeScale = 0;
            Debug.Log(distance);

                if ( (distance < PlayerPrefs.GetInt("Highscore" + levelNumber)) || (PlayerPrefs.GetInt("Highscore" + levelNumber) == 0)){ //if your score is higher than last time or there is no current score 

                PlayerPrefs.SetInt("Highscore" + levelNumber, (int) distance); //assigns distance to your score

                pointsRecieved = PlayerPrefs.GetInt("Highscore" + levelNumber);  //sets points recieved this playthrough to your distance as an int
                }

                overallScore = (PlayerPrefs.GetInt("Highscore" + 1) + PlayerPrefs.GetInt("Highscore" + 2) + PlayerPrefs.GetInt("Highscore" + 3)
                + PlayerPrefs.GetInt("Highscore" + 4) + PlayerPrefs.GetInt("Highscore" + 5) + PlayerPrefs.GetInt("Highscore" + 6)
                + PlayerPrefs.GetInt("Highscore" + 7) + PlayerPrefs.GetInt("Highscore" + 8) + PlayerPrefs.GetInt("Highscore" + 9) 
                + PlayerPrefs.GetInt("Highscore" + 10) + PlayerPrefs.GetInt("Highscore" + 11) + PlayerPrefs.GetInt("Highscore" + 12)
                + PlayerPrefs.GetInt("Highscore" + 13) + PlayerPrefs.GetInt("Highscore" + 14) + PlayerPrefs.GetInt("Highscore" + 15)
                + PlayerPrefs.GetInt("Highscore" + 16) + PlayerPrefs.GetInt("Highscore" + 17) + PlayerPrefs.GetInt("Highscore" + 18)
                + PlayerPrefs.GetInt("Highscore" + 19) +  PlayerPrefs.GetInt("Highscore" + 20) + PlayerPrefs.GetInt("Highscore" + 21)); //adds up all the high score to come up with current points

                
            VictoryScreen.Setup(distance, overallScore, pointsRecieved, levelNumber);
        }
    }

   /* public void Bricks(){
        bricks = true;
    }

        public void Logs(){
        logs = true;
    }  */
}


