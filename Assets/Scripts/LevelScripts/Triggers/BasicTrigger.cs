using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger that activates when intersected
/// </summary>
public class BasicTrigger : Trigger {

    public void OnTriggerEnter()
    {
        activate = true;
    }
}
