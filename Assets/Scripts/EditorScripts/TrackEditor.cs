using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TrackEditor : MonoBehaviour {

    public string whatTag;
    public string whatLayer;
    public string whatName;
    public GameObject roadSegment;
    public GameObject waypoint;
    public BezierSpline spline;
    float currentTime = 0;
    float centerZ;
    Mesh defaultMesh;
    Mesh mesh;
    GameObject road;
    Material[] mat;

    public List<List<Vector3>> segmentVertices;
    public List<List<Vector3>> defaultSegmentVertices;  // Road segment vertices divided into seperate lists, by column.
    public List<List<int>> segmentVerticesID;           // Road segment Indices divided into seperate lists mirroing segment Verttices;
    public List<bool> segmentVerticesUsed;
    public float roadDetail = 1;                        // How detailed (distance between forward vertices) the road should be. 
    int counter = 0;
    float roadTime = 0;
    void Initialize(string whatTag, string whatLayer, string whatName, GameObject roadSegment, BezierSpline spline)
    {

    }
    void SortVertices()
    {
        defaultMesh = Instantiate(road.GetComponent<MeshFilter>().sharedMesh);
        List<Vector3> meshVertices = new List<Vector3>(defaultMesh.vertices);
        List<Vector3> meshVerticesSorted = new List<Vector3>(defaultMesh.vertices);
        List<int> meshVerticesID = new List<int>();
        meshVerticesSorted.Sort(delegate(Vector3 i1, Vector3 i2) { return i1.x.CompareTo(i2.x); });

        for (int i = 0; i < meshVerticesSorted.Count; ++i)
        {
            for(int j = 0; i < meshVertices.Count; ++j)
            {
                if (meshVerticesSorted[i] == meshVertices[j])
                {
                    meshVerticesID.Add(j);
                }
            }
        }
    }

    int FindVertexPosInMesh(Vector3 vertex, List<Vector3> meshVertices)
    {
        for(int i = 0; i < meshVertices.Count; ++i)
        {
            if (segmentVerticesUsed[i])
                continue;

            if (vertex == meshVertices[i])
            {
                segmentVerticesUsed[i] = true;
                return i;
            }
        }
        return 0;
    }
    // Goes through roadSegment's  mesh and assigns it's vertices to segmentVertices by column.
    void AssignSegmentVerts()
    {
        defaultMesh = Instantiate(road.GetComponent<MeshFilter>().sharedMesh);
        mesh = new Mesh();
        road.GetComponent<MeshFilter>().sharedMesh = mesh;
        defaultSegmentVertices = new List<List<Vector3>>();
        List<Vector3> meshVertices = new List<Vector3>(defaultMesh.vertices);
        List<Vector3> meshVerticesSorted = new List<Vector3>(defaultMesh.vertices);
        List<int> meshVerticesID = new List<int>();
        meshVerticesSorted.Sort(delegate(Vector3 i1, Vector3 i2) { return i1.x.CompareTo(i2.x); });

        segmentVerticesUsed = new List<bool>();
        for (int i = 0; i < meshVertices.Count; ++i)
            segmentVerticesUsed.Add(false);

        for (int i = 0; i < meshVerticesSorted.Count; ++i)
        {
            for (int j = 0; j < meshVertices.Count; ++j)
            {
                if (meshVerticesSorted[i] == meshVertices[j])
                {
                    meshVerticesID.Add(j);
                }
            }
        }
        // Insert vertices to seperate lists, by column
        int columnIndex = 0;
        float mostLeft = 0;
        float mostRight = 0;
        float currentX = meshVerticesSorted[0].x;
        segmentVertices = new List<List<Vector3>>();
        segmentVertices.Add(new List<Vector3>());
        defaultSegmentVertices = new List<List<Vector3>>();
        defaultSegmentVertices.Add(new List<Vector3>());

        segmentVerticesID = new List<List<int>>();
        segmentVerticesID.Add(new List<int>());
        

        for (int i = 0; i < meshVerticesSorted.Count; i++)
        {
            if (mostLeft > meshVerticesSorted[i].z)
                mostLeft = meshVerticesSorted[i].z;
            if (mostRight < meshVerticesSorted[i].z)
                mostRight = meshVerticesSorted[i].z;
            if (meshVerticesSorted[i].x != currentX)
            {
                columnIndex++;
                segmentVertices.Add(new List<Vector3>());
                defaultSegmentVertices.Add(new List<Vector3>());
                segmentVerticesID.Add(new List<int>());
            }

            segmentVertices[columnIndex].Add(meshVerticesSorted[i]);
            defaultSegmentVertices[columnIndex].Add(new Vector3(meshVerticesSorted[i].x, meshVerticesSorted[i].y, meshVerticesSorted[i].z));
            segmentVerticesID[columnIndex].Add(FindVertexPosInMesh(meshVerticesSorted[i], meshVertices));
            currentX = meshVerticesSorted[i].x;

            //if (columnIndex == 0)
            //    StartCoroutine(DrawVertex(0, meshVerticesSorted[i]));
        }
        centerZ = (mostRight + mostLeft) * 0.5f;
    }

    //private int XCompare(Vector3 value1, Vector3 value2)
    //{
    //    if (value1.x > value2.x)
    //    {
    //        return 1;
    //    }
    //    return -1;
    //}

    // Debug function to draw mesh vertices one by one.
    IEnumerator DrawVertex(float time, Vector3 vertex)
    {
        yield return new WaitForSeconds(time);
        Debug.DrawLine(road.transform.position + vertex, road.transform.position + vertex + Vector3.up, Color.red, 25);
    }
	void Start () 
    {
        //road = Instantiate(roadSegment, Vector3.zero, Quaternion.identity, null) as GameObject;

        //AssignSegmentVerts();
        //currentTime = 0;
        //int splineCurves = spline.CurveCountAll;
        //while (currentTime < splineCurves - 0.002f)
        //{
        //    PlaceSegmentsOnSpline(currentTime);
        //}
        //SaveMesh();
        ////for (int i = 0; i < 13; ++i)// (currentTime < splineCurves)
        ////{
        ////    PlaceSegmentsOnSpline(currentTime);
        ////}
    }
    //Make the track great again
    public void BuildTheWall(GameObject trackSegment, BezierSpline thisSpline, string whatTag, string whatLayer, string whatName, float detail)
    {
        this.whatTag = whatTag;
        this.whatLayer = whatLayer;
        this.whatName = whatName;
        this.roadDetail = detail;
        spline = thisSpline;
        counter = 0;

        if (GameObject.Find(thisSpline.name + trackSegment.name + "(Clone)"))
            DestroyImmediate(GameObject.Find(thisSpline.name + trackSegment.name + "(Clone)"));

        road = Instantiate(trackSegment, Vector3.zero, Quaternion.identity, null) as GameObject;
        road.name = thisSpline.name + road.name;
        AssignSegmentVerts();
        currentTime = 0;
        int splineCurves = spline.CurveCountAll;
        while (currentTime < splineCurves - 0.002f)
        {
            ++counter;
            PlaceSegmentsOnSpline(currentTime);
        }
        SaveMesh();
    }
    void PlaceSegmentsOnSpline(float startTime)
    {
        Vector3 oldPos = defaultSegmentVertices[0][0];
        Vector3 point = spline.GetPointConstantSpeed(ref startTime, 0, 1);
        PlaceSegmentAt(point, startTime, 0);
        for (int i = 1; i < defaultSegmentVertices.Count; ++i)
        {
            float vertexDistance = Mathf.Abs(defaultSegmentVertices[i][0].x - oldPos.x);
            float detail = vertexDistance / roadDetail;
            if (detail < 1)
                detail = 1;
            point = spline.GetPointConstantSpeed(ref currentTime, vertexDistance, roadDetail);
            oldPos = defaultSegmentVertices[i][0];
            PlaceSegmentAt(point, currentTime, i);
        }
        for (int i = 0; i < segmentVertices.Count; ++i)
        {
            for(int j = 0; j < segmentVertices[i].Count; ++j)
            {
                StartCoroutine(DrawVertex(1, segmentVertices[i][j]));
            }
        }
        UpdateMeshToSpline();
    }
    void PlaceSegmentAt(Vector3 point, float time, int segmentIndex)
    {
        //Vector3 pos = spline.GetPoint(time); //WRONG FUNCTION
        Quaternion rot = spline.GetRotationAt(time) * Quaternion.Euler(0.0f, 0.0f, 90.0f);
        Vector3 directionSideways = rot * Vector3.up;
        Vector3 directionUp = rot * Quaternion.Euler(0.0f, 0.0f, 90.0f) * Vector3.up;
        List<Vector3> segment = segmentVertices[segmentIndex];
        List<Vector3> defaultSegment = defaultSegmentVertices[segmentIndex];
        for (int i = 0; i < segment.Count; ++i)
        {
            float offset = defaultSegment[i].z - centerZ;
            float height = -defaultSegment[i].y;
            segment[i] = point + directionSideways * offset + directionUp * height;
            //StartCoroutine(DrawVertex(0, point + directionSideways * offset + directionUp * height));
        }
    }
    void UpdateMeshToSpline()
    {
        // Create a new Mesh of a copy of DefaultMesh to be combined later
        Mesh newMesh = Instantiate(defaultMesh);
        MeshFilter RoadMeshFilter = road.GetComponent<MeshFilter>();

        // Assign Vertices to new Mesh on the spline
        List<Vector3> meshVertices = new List<Vector3>(newMesh.vertices);
        for(int i = 0; i < segmentVerticesID.Count; ++i)
        {
            for(int j = 0; j < segmentVerticesID[i].Count; ++j)
            {
                meshVertices[segmentVerticesID[i][j]] = segmentVertices[i][j];
            }
        }
        newMesh.SetVertices(meshVertices);

        // Combine old mesh and the new mesh together
        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = RoadMeshFilter.sharedMesh;
        combine[0].transform = Matrix4x4.identity;// transform.localToWorldMatrix;
        combine[1].mesh = newMesh;
        combine[1].transform = Matrix4x4.identity;// transform.localToWorldMatrix;

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        RoadMeshFilter.mesh = combinedMesh;
        RoadMeshFilter.sharedMesh.name = "RoadMesh";
    }
    void SaveMesh()
    {
        //Save the mesh so that the prefab can get it somehow
        road.GetComponent<MeshFilter>().sharedMesh.subMeshCount = defaultMesh.subMeshCount;
        for (int i = 1; i < defaultMesh.subMeshCount; ++i)
        {
            int[] triangles = defaultMesh.GetTriangles(i);
            Array.Resize(ref triangles, defaultMesh.GetTriangles(i).Length * counter);
            for(int j = 0; j < counter; ++j)
            {
                int totalTriangles = defaultMesh.GetTriangles(i).Length;
                for (int k = 0; k < totalTriangles; ++k)
                {
                    triangles[j * totalTriangles + k] = defaultMesh.GetTriangles(i)[k] + defaultMesh.vertices.Length * j;
                }
            }
            road.GetComponent<MeshFilter>().sharedMesh.SetTriangles(triangles, i);
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(road.GetComponent<MeshFilter>().sharedMesh, "Assets/Prefabs/Tracks/Generated Track Meshes/" + name + whatName + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + ".asset");
        UnityEditor.AssetDatabase.SaveAssets();

        //Set imortant variables before saving that will be used
        road.tag = whatTag;
        road.layer = LayerMask.NameToLayer(whatLayer);
        road.transform.localScale = new Vector3(1, 1, 1);

        //Set the MeshCollider to the new mesh
        road.AddComponent<MeshCollider>();
        road.GetComponent<MeshCollider>().sharedMesh = road.GetComponent<MeshFilter>().sharedMesh;
        //road.GetComponent<MeshCollider>().sharedMesh = road.GetComponent<MeshFilter>().sharedMesh;

        //Create the Prefab of the Tack
        UnityEditor.PrefabUtility.CreatePrefab("Assets/Prefabs/Tracks/Generated Track Prefabs/" + name + whatName + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + ".prefab", road);
#endif
    }
    public void DeleteWaypoints()
    {
        GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
        foreach (GameObject w in waypoints)
        {
            DestroyImmediate(w);
        }
    }
    public void CreateWaypointsOnSpline(BezierSpline thisSpline, GameObject waypoint, float distanceBetweenWaypoints)
    {
        spline = thisSpline;
        
        currentTime = 0;
        int splineCurves = spline.CurveCountAll;
        PlaceWaypointOnSpline(waypoint, 0, 1);
        while (currentTime < splineCurves - 0.002f)
        {
            PlaceWaypointOnSpline(waypoint, currentTime, distanceBetweenWaypoints);
        }
    }
    void PlaceWaypointOnSpline(GameObject waypoint, float time, float distance)
    {
        GameObject newWaypoint = (GameObject)Instantiate(waypoint, Vector3.zero, Quaternion.identity, null);
        newWaypoint.transform.position = spline.GetPointConstantSpeed(ref currentTime, distance, roadDetail);
        newWaypoint.transform.rotation = spline.GetRotationAt(currentTime);
        GameObject waypoints = GameObject.Find("Waypoints");
        if (!waypoints)
        {
            waypoints = new GameObject();
            waypoints.name = "Waypoints";
        }
        newWaypoint.transform.parent = waypoints.transform;
        newWaypoint.name = waypoint.name + GameObject.FindGameObjectsWithTag("Waypoint").Length;
    }

    //void Update () 
    //{
    //    //road.transform.position += road.transform.forward;
    //    //CalculateConstantSpeedOnSpline();
    //}

    //void CalculateConstantSpeedOnSpline()
    //{
    //    Vector3 newPosition = spline.GetPointConstantSpeed(ref currentTime, Time.deltaTime, 1);
    //    Quaternion rotation = spline.GetRotationAt(currentTime);
    //}


    //void PlaceSegment()
    //{
    //    // Create a copy of the mesh vertices
    //    List<List<Vector3>> segmentVertsCopy = segmentVertices;

    //    // Move the copied vertices into place along the track

    //    // For each column (several per mesh)
    //    for (int i = 0; i < segmentVertices.Count; i++)
    //    {
    //        Vector3 middlePos = spline.GetPoint(roadTime);// Get point! constantspeedonspline?
    //        Quaternion rotation = spline.GetRotationAt(roadTime);// Get point! constantspeedonspline?

    //        // For each vertex in the column
    //        for (int j = 0; j < segmentVertices.Count; j++)
    //        {
    //            // Get spline position at each column (+ - pos at road time).
    //            // Move in x(?) to match mesh distance from center.
    //            // Rotate around spline position (column).

    //            //RotatePointAroundPivot(segmentVertices[i][j], middlePos, rotation.eulerAngles);
    //        }
    //        roadTime += roadDetail;
    //    }
    //}



    //Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    //{
    //    Vector3 dir = point - pivot; 
    //    dir = Quaternion.Euler(angles) * dir;
    //    point = dir + pivot; 
    //    return point; 
    //}
}