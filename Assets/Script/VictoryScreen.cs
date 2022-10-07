/**
 * Edited by - Wade Canavan
 * HGD team 13 
 * Manage the levels of the game 
 * created 10/6/2022
 * last upadated 10/6/2022
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    public Text victoryText;
    public float levelNumber; 

//Makes Victory screen visable and changes text given in gamemanager to the last recorded distance
    public void Setup( float distance ){
        gameObject.SetActive(true);
        victoryText.text = "Level " + levelNumber.ToString() + " solved. Your Distance is " + distance.ToString() + " m.";
    }
}