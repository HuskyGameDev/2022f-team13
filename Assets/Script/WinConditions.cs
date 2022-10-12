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

public class WinConditions : MonoBehaviour
{
    public string carTag; //put the tag of the train or car the station will destect
    public string gmFuction;


    //when train comes in contact with the station and set up call function in GameManager.cs
     void OnCollisionEnter(Collision collision){
        if ((collision.gameObject.tag == carTag) && ( carTag == "Locomotive" )){
            
            FindObjectOfType<GameManager>().Victory();
        }

        if ((collision.gameObject.tag == carTag) && ( carTag == "Bricks") ){
            
            FindObjectOfType<GameManager>().Bricks();
        }

        if ((collision.gameObject.tag == carTag) && (carTag == "Logs") ){
            
            FindObjectOfType<GameManager>().Logs();
        }
     }
}
