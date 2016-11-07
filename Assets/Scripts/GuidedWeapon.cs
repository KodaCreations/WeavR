using UnityEngine;
using System.Collections;

public class GuidedWeapon : Weapon {
    public ShipController target;
    private float speed;

	// Use this for initialization
	public override void Start () {
        Initialize(5);
        base.Start();
	}

    private void Initialize(float speed)
    {
        this.speed = speed;
    }
	
	// Update is called once per frame
	public override void Update () {
        if (target)
        {
            Vector3 dir = target.transform.position - transform.position;

            transform.position += dir * Time.deltaTime * speed;

            base.Update();
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit");
        ParticleSystem explosion = GetComponent<ParticleSystem>();
        explosion.Play();
        Destroy(gameObject, explosion.duration);
        GetComponent<Collider>().enabled = false;

        GetComponentInChildren<MeshRenderer>().enabled = false;
    }
}
