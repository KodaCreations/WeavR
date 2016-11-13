using UnityEngine;
using System.Collections;

public class DropObject : TriggerEffect {

    public Rigidbody rb;

    void Update()
    {
        if(activate == true)
        {
            rb.useGravity = true;
        }
    }

    

}
