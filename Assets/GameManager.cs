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

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1; //unpauses the game if it was paused
        distanceText.text = "Distance Traveled: 0";
    }

    public void ChangeText(float addDistance)
    {
        distanceText.text = "Distance Traveled: " + addDistance;
        distance = addDistance;
    }

    //allows the vistory screen to display tyhe last recorded distance
    public void Victory(){
        if( bricks && logs){
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


