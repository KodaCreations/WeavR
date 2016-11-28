using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorPath : MonoBehaviour {

    public Color Rcolor = Color.red;
    public List<Transform> pathObjs = new List<Transform>();
    Transform[] pathArray;

    void OnDrawGizmos()
    {
        Gizmos.color = Rcolor;
        pathArray = GetComponentsInChildren<Transform>();
        pathObjs.Clear();

        foreach (Transform pathObj in pathArray)
        {
            if (pathObj != this.transform)
            {
                pathObjs.Add(pathObj);
            }
        }

        for (int i = 0; i < pathObjs.Count; i++)
        {
            Vector3 position = pathObjs[i].position;
            if (i > 0)
            {
                Vector3 previousPos = pathObjs[i - 1].position;
                Gizmos.DrawLine(previousPos, position);
                Gizmos.DrawWireSphere(position, 0.3f);
            }
        }
    }
}
