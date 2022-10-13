using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainStationCount : MonoBehaviour
{
    public int numOfTrainStations()
    {
        return transform.childCount;
    }
}
