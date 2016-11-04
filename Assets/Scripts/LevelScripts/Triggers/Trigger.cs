using UnityEngine;
using System.Collections;

/// <summary>
/// Base trigger class
/// </summary>
public class Trigger : MonoBehaviour {

    // Reference to affected object in scene
    public TriggerEffect teObject;              
    protected bool activate = false;

    public virtual void Update()
    {
        // Activates the effect
        if (activate == true)
            teObject.Activate();
    }
}
