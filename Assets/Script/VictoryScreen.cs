/**
 * Edited by - Wade Canavan
 * HGD team 13 
 * Manage the levels of the game 
 * created 10/6/2022
 * last updated 11/27/2022
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    public Text victoryText;    

//Makes Victory screen visible and changes text given in gamemanager to the last recorded distance
    public void Setup( float distance , int overallScore, int pointsRecieved, int levelNumber){

        gameObject.SetActive(true);

        victoryText.text = "Level " + levelNumber.ToString() + " solved. Your Distance is " + distance.ToString() + " m. \n You recieved " + pointsRecieved + " points. Your total points are " + overallScore + " points." ;;
    }

}
