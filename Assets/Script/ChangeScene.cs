using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string LevelName;

    //Function loads scene that we designate in unity
    public void LoadLevel(){ 
        SceneManager.LoadScene(LevelName);
    }
}
