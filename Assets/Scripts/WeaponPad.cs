using UnityEngine;
using System.Collections;

public class WeaponPad : MonoBehaviour {
    private float timer;
    private bool active;

	// Use this for initialization
	void Start () {
        active = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (!active)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                GetComponentInChildren<MeshRenderer>().enabled = true;
                active = true;
            }
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (active)
        {
            active = false;
            timer = 10;
            GetComponentInChildren<MeshRenderer>().enabled = false;
            GetWeapon(); //ge till skepp
        }
    }

    private Weapon GetWeapon()
    {
        int rnd = Random.Range(0, 99);
        if (rnd < 5)
            return (Weapon)Instantiate(Resources.Load("Prefabs/EMP") as GameObject, new Vector3(0, 0, 0), Quaternion.identity); ///Ge bara enum och spawna i skeppet istället
        if (rnd < 35)
            return (Weapon)Instantiate(Resources.Load("Prefabs/Missile") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
        if (rnd < 57)
            return (Weapon)Instantiate(Resources.Load("Prefabs/Mine") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
        if (rnd < 80)
            return (Weapon)Instantiate(Resources.Load("Prefabs/EnergyDrain") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
        return (Weapon)Instantiate(Resources.Load("Prefabs/DecreaseVision") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
    }
}
