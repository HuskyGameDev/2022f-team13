using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerSwitch : MonoBehaviour
{
    public AudioSource switchNoise;
    public AudioClip switchClip;
    // Start is called before the first frame update
    void Start()
    {
        switchNoise = gameObject.AddComponent<AudioSource>();
        switchNoise.clip = switchClip;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Switching()
    {
        switchNoise.Play();
    }
}
