using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerTrainAndCarScript : MonoBehaviour
{
    public AudioSource trainConnect;
    public AudioSource trainDisconnect;
    public AudioClip trainConnectClip;
    public AudioClip trainDisconnectClip;
    // Start is called before the first frame update
    void Start()
    {
        trainConnect = gameObject.AddComponent<AudioSource>();
        trainDisconnect = gameObject.AddComponent<AudioSource>();
        trainConnect.clip = trainConnectClip;
        trainDisconnect.clip = trainDisconnectClip;
        trainConnect.playOnAwake = false;
        trainDisconnect.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Connection()
    {
        trainConnect.Play();
    }
    public void Disconnection()
    {
        trainDisconnect.Play();
    }
}
