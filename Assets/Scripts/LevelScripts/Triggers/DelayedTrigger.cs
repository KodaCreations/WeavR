using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger that activates with a delay after being intersected
/// </summary>
public class DelayedTrigger : Trigger {

    public float timer;
    private bool startTimer = false;
    private bool stop = false;

    void OnTriggerEnter()
    {
        startTimer = true;
    }

    public override void Update()
    {

        base.Update();

        if (startTimer == true && stop == false)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                base.activate = true;
                stop = true;
            }
        }
    }
}
