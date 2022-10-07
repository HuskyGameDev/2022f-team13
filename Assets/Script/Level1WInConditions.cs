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

public class Level1WInConditions : MonoBehaviour
{
    //when train comes in contact with the station pause game and set up victory screeen
     void OnCollisionEnter(Collision collision){
        if (collision.gameObject.tag == "Gold"){
            Time.timeScale = 0;
            FindObjectOfType<GameManager>().Victory();
        }
     }
}
