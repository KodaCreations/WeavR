using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour {


    [HideInInspector]
    public IAiState currentState;
    [HideInInspector]
    public ChaseState chaseState;
    [HideInInspector]
    public AttackState attackState;
    [HideInInspector]
    public LookForTriggerState lookForTriggerState;
    [HideInInspector]
    public LookForWeaponState lookForWeaponState;

    public TheRabbit rabbit;
    public ShipController ship;
    public bool activateAI;
    public Vector3 offset;

    // Use this for initialization
    void Awake()
    {
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        lookForTriggerState = new LookForTriggerState(this);
        lookForWeaponState = new LookForWeaponState(this);
    }
    void Start()
    {
        
        ship = gameObject.GetComponent<ShipController>();
        currentState = chaseState;
        //offset = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
    }

    // Update is called once per frame
    void Update ()
    {
        if(activateAI)
            currentState.UpdateState();
        //ChangeOffset();
	}

    void ChangeOffset()
    {
        offset.x += Random.Range(-1.0f, 1.0f) * Time.deltaTime;
        offset.z += Random.Range(-1.0f, 1.0f) * Time.deltaTime;
        offset.y += Random.Range(-1.0f, 1.0f) * Time.deltaTime;
        offset.x = Mathf.Clamp(offset.x, -2, 2);
        offset.z = Mathf.Clamp(offset.z, -2, 2);
        offset.y = Mathf.Clamp(offset.y, -2, 2);

    }


    public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.026)
        {
            return 1.0f;
        }
        else if (dir < -0.026)
        {
            return -1.0f;
        }
        else {
            return 0.0f;
        }
    }
}
