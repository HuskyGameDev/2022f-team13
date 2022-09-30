/**
 * Edited by - Brendan Singer
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    // check if win conditions are met 
    // if they are call the gotolevelslect method
    private void Update()
    {
        // all of the componets of the train have to reach the desired destination 
        // Gotolevelselect();
    }

    // Load the levelselctscence 
    private void Gotolevelselect()
    {
        SceneManager.LoadScene("Level Selection");
    }

}
