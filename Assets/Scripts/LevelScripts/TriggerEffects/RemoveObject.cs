using UnityEngine;
using System.Collections;

public class RemoveObject : TriggerEffect
{

    public GameObject gObject;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (activate)
            gObject.SetActive(false);

    }
}