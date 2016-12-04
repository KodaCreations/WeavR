using UnityEngine;
using System.Collections;

/// <summary>
/// Base trigger effect class
/// </summary>
public class TriggerEffect : MonoBehaviour
{

    protected bool activate = false;

    // Called from a trigger to activate the effect
    public void Activate()
    {
        activate = true;
    }
}
