using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Type : MonoBehaviour
{
    public GameObject Coal;
    public GameObject Nuke;
    public GameObject Brick;
    public GameObject Logs;
    public GameObject Bullets;
    public GameObject Pigs;
    public GameObject Cows;
    public GameObject Iron;
    public GameObject Car;
    public GameObject Flower;
    public GameObject Cash;
    public GameObject Gold;

    GameObject temp;
    GameObject temp2 = null;
    Vector3 position;
    Quaternion rotation;
    Vector3 scale;

    public enum element//only add types to the bottom or all the types on the game will get messed up
    {
        Train,
        Coal,
        Nuke,
        Brick,
        Logs,
        Bullets,
        Pigs,
        Cows,
        Iron,
        Car,
        Oranges,
        Blue,
        Maroon,
        Flower,
        Cash,
        Green,
        Gold,
    };
    public element e;
    element preve;
    // Start is called before the first frame update
    void Start()
    {
        //Set the location for each object 
        if (gameObject.CompareTag("Train"))
        {
            temp = gameObject;
            position = new Vector3(-0.0007f, -0.01234f, 0.032f);
            rotation = Quaternion.Euler(0, 0, 270);
            scale = new Vector3(.5f, .5f, 1f);
        }
        else if (gameObject.CompareTag("Coal"))
        {
            temp = gameObject;
            position = new Vector3(0.0004f, 0.0035f, 0.0324f);
            rotation = Quaternion.Euler(0, 0, -90);
            scale = new Vector3(.3f, .3f, .3f);
        }
        else if (gameObject.CompareTag("Bucket"))
        {
            temp = transform.parent.GetChild(1).gameObject;
            if (temp.CompareTag("Bucket")) {
                position = new Vector3(0f, 0f, 0.03f);
                rotation = Quaternion.Euler(0, 0, 0);
                scale = new Vector3(.75f, .75f, 1f);

            } else
            {
                //Error State: I have no clue what needs to happen here
                //Actually, nothing should happen, literally things should explode 
                return;
            }
        } else
        {
            //Error State: Something really bad happened if we ended up here
            return;
        }

        //Spawn the train icon in the center of each object
        //Declare a model object that will be adjusted here

        changeValue();



    }

    // Update is called once per frame
    void Update()
    {
        //Check if the value has changed
        if (e != preve)
        {
            Destroy(temp2);
            changeValue();
            
        }

        preve = e;
    }

    void changeValue()
    {
        switch (e)
        {
            case element.Train:
                break;
            case element.Coal:
                temp2 = Instantiate(Coal, temp.transform, false);
                break;
            case element.Nuke:
                temp2 = Instantiate(Nuke, temp.transform, false);
                break;
            case element.Brick:
                temp2 = Instantiate(Brick, temp.transform, false);
                break;
            case element.Logs:
                temp2 = Instantiate(Logs, temp.transform, false);
                break;
            case element.Cows:
                temp2 = Instantiate(Cows, temp.transform, false);
                break;
            case element.Bullets:
                temp2 = Instantiate(Bullets, temp.transform, false);
                break;
            case element.Pigs:
                temp2 = Instantiate(Pigs, temp.transform, false);
                break;
            case element.Iron:
                temp2 = Instantiate(Iron, temp.transform, false);
                break;
            case element.Car:
                temp2 = Instantiate(Car, temp.transform, false);
                break;
            case element.Oranges:
                break;
            case element.Blue:
                break;
            case element.Maroon:
                break;
            case element.Flower:
                temp2 = Instantiate(Flower, temp.transform, false);
                break;
            case element.Cash:
                temp2 = Instantiate(Cash, temp.transform, false);
                break;
            case element.Green:
                break;
            case element.Gold:
                temp2 = Instantiate(Gold, temp.transform, false);
                break;
        }

        if (temp2 != null)
        {
            temp2.transform.localRotation = rotation;
            temp2.transform.localPosition = position;
            temp2.transform.localScale = scale;
        }
    }
}
