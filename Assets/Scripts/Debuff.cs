using UnityEngine;
using System.Collections;

public class Debuff {
    public readonly float speedReduction;
    public readonly bool energyDrain;
    public readonly bool reduceVisability;
    public readonly bool shutDown;
    private float timer;

    public Debuff(float speedReduction, bool energyDrain, bool reduceVisability, bool shutDown, float timer)
    {
        this.timer = timer;
        this.speedReduction = speedReduction;
        this.energyDrain = energyDrain;
        this.reduceVisability = reduceVisability;
        this.shutDown = shutDown;
    }

    public bool DebuffFinished()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
            return true;
        return false;
    }
}


// Implementera i skepp för att förverkliga debufferna.
