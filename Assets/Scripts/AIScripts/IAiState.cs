using UnityEngine;
using System.Collections;

public interface IAiState
{

    void UpdateState();

    void ToLookForTrigger();

    void ToLookForWeapon();

    void ToAttackState();

}
