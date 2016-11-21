using UnityEngine;
using System.Collections;

public class GuidedWeapon : Weapon {
    public Transform target;
    public int range;

	// Use this for initialization
	public override void Start () {
        range = 1000;
        base.Start();
	}
	
	// Update is called once per frame
	public override void Update () {
        if (target)
        {
            Vector3 dir = target.position - transform.position;

            transform.position += dir * Time.deltaTime * speed;

            base.Update();
        }
	}
}
