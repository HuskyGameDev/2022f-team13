using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class GameManager : MonoBehaviour
{
    public Text distanceText;
    // Start is called before the first frame update
    void Start()
    {
        distanceText.text = "Distance Traveled: 0";
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ChangeText(float addDistance)
    {
        distanceText.text = "Distance Traveled: " + addDistance;
    }
}


