using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplitSegment))]
public class NewBehaviourScript : Editor
{
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private int selectedIndex = -1;
    SplitSegment split;
    private Transform handleTransform;
    private Quaternion handleRotation;
    // Use this for initialization
    void Start () {

    }
    private void OnSceneGUI()
    {
        split = target as SplitSegment;
        handleTransform = split.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        for (int i = 0; i < split.GetTotalPoints(); ++i)
        {
            ShowPoint(i);
        }
    }
    public override void OnInspectorGUI()
    {
        split = target as SplitSegment;

        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(split, "Add Curve");
            split.AddPoint();
            EditorUtility.SetDirty(split);
        }

        if (selectedIndex >= 0 && selectedIndex < split.GetTotalPoints())
        {
            DrawSelectedPointInspector();
        }
    }
    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        GUILayout.Label("Index: " + selectedIndex);
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", split.GetPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(split, "Move Point");
            EditorUtility.SetDirty(split);
            split.SetPoint(selectedIndex, point);
        }
        EditorGUI.BeginChangeCheck();
        split.splines[selectedIndex] = (BezierSpline)EditorGUILayout.ObjectField("Spline", split.GetSpline(selectedIndex), typeof(BezierSpline), true);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(split, "Move Spline");
            EditorUtility.SetDirty(split);
            if (split.splines[selectedIndex])
                split.SetSpline(selectedIndex, split.splines[selectedIndex]);
        }
        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Attach To Back of Spline", split.GetAttachToBack(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(split, "Toggle Attack To Back");
            EditorUtility.SetDirty(split);
            split.SetAttachToBack(selectedIndex, loop);
        }

    }
    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(split.GetPoint(index));
        float size = HandleUtility.GetHandleSize(point);

        Handles.color = Color.cyan;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index)
        {
            ShowPositionHandle(selectedIndex);
        }
        return point;
    }
    private void ShowPositionHandle(int index)
    {
        Vector3 point = handleTransform.TransformPoint(split.GetPoint(index));
        EditorGUI.BeginChangeCheck();
        point = Handles.PositionHandle(point, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(split, "Move Point");
            EditorUtility.SetDirty(split);
            split.SetPoint(index, handleTransform.InverseTransformPoint(point));
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
