using UnityEngine;
using System.Collections;

/// <summary>
/// This class represents a Debuff recieved from being hit by a weapon
/// </summary>
public class Debuff {
    public readonly float speedReduction;
    public readonly bool energyDrain;
    public readonly bool reduceVisability;
    public readonly bool shutDown;
    private float timer;

    /// <summary>
    /// Constructor for Debuff
    /// </summary>
    /// <param name="speedReduction">How much the speed should be reduced with</param>
    /// <param name="energyDrain">Whether the ship should be drained of energy or not</param>
    /// <param name="reduceVisability">Whether the visability of the player should be reduced or not</param>
    /// <param name="shutDown">Whether the ship should shut down or not</param>
    /// <param name="timer">How long the Debuff should last</param>
    public Debuff(float speedReduction, bool energyDrain, bool reduceVisability, bool shutDown, float timer)
    {
        this.timer = timer;
        this.speedReduction = speedReduction;
        this.energyDrain = energyDrain;
        this.reduceVisability = reduceVisability;
        this.shutDown = shutDown;
    }

    /// <summary>
    /// Updates the timer of the Debuff
    /// </summary>
    /// <returns>Whether the Debuff timer has run out or not</returns>
    public bool DebuffFinished()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
            return true;
        return false;
    }
}
