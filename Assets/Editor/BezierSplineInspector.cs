using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor {

    private const int stepsPerCurve = 10;
    private const float directionScale = 0.5f;

    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private int selectedIndex = -1;

    private BezierSpline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private Quaternion newRotation;

    private static Color[] modeColors = { Color.white, Color.yellow, Color.cyan };

    private void OnSceneGUI()
    {
        spline = target as BezierSpline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p3 = ShowPoint(i + 2);
            Vector3 p2 = ShowPoint(i + 1);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }

        ShowDirections();
    }
    public override void OnInspectorGUI()
    {
        spline = target as BezierSpline;
        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Toggle Loop");
            EditorUtility.SetDirty(spline);
            spline.Loop = loop;
        }
        EditorGUI.BeginChangeCheck();
        spline.parent = (BezierSpline)EditorGUILayout.ObjectField("Parent: ", spline.parent, typeof(BezierSpline), true);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Set Parent");
            EditorUtility.SetDirty(spline);
            spline.EnforceParent();
            //spline.parent = parent.GetComponent<BezierSpline>();
        }
        EditorGUI.BeginChangeCheck();
        spline.wallSegment = (GameObject)EditorGUILayout.ObjectField("Wall: ", spline.wallSegment, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Set Parent");
            EditorUtility.SetDirty(spline);
        }
        EditorGUI.BeginChangeCheck();
        spline.floorSegment = (GameObject)EditorGUILayout.ObjectField("Floor: ", spline.floorSegment, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Set Parent");
            EditorUtility.SetDirty(spline);
        }
        if (GUILayout.Button("Generate Mesh"))
        {
            Undo.RecordObject(spline, "Generated Mesh");
            spline.BuildMesh();
            EditorUtility.SetDirty(spline);
        }
        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
        {
            switch (Tools.current)
            {
                case Tool.Rotate:
                    DrawSelectedRotationInspector();
                    break;
                case Tool.Move:
                default:
                    DrawSelectedPointInspector();
                    break;
            }
            if (selectedIndex > 0 && selectedIndex < spline.ControlPointCount - 3 && selectedIndex % 3 == 0)
            {
                if (GUILayout.Button("Split Spline"))
                {
                    Undo.RecordObject(spline, "Split Spline");
                    GameObject newSpline = new GameObject();
                    newSpline.name = spline.name + ".1";
                    spline.SplitSplineAt(selectedIndex, newSpline);
                    EditorUtility.SetDirty(spline);
                }
            }
        }
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
        if(spline.parent)
        {
            if (GUILayout.Button("Merge With Parent"))
            {
                Undo.RecordObject(spline, "Add Curve");
                GameObject go = spline.parent.gameObject;
                spline.MergeWithParent();
                DestroyImmediate(go);
                EditorUtility.SetDirty(spline);
            }
        }
    }
    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        GUILayout.Label("Index: " + selectedIndex);
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }
        EditorGUI.BeginChangeCheck();
        BezierSpline.BezierControlPointMode mode = (BezierSpline.BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }
    private void DrawSelectedRotationInspector()
    {
        GUILayout.Label("Selected Rotation");
        GUILayout.Label("Index: " + selectedIndex);
        EditorGUI.BeginChangeCheck();
        float angle = spline.GetControlRotation(selectedIndex);
        angle = EditorGUILayout.FloatField("Rotation", angle);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Rotate Point");
            EditorUtility.SetDirty(spline);;
            spline.SetControlRotation(selectedIndex, angle);
        }
    }

    private void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point = spline.GetPoint(0f);
        Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
        int steps = stepsPerCurve * spline.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            point = spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
        }
    }
    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0)
            size *= 2f;

        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if(index % 3 == 0)
        {
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
            {
                selectedIndex = index;
                newRotation = Quaternion.LookRotation(spline.GetDirection(selectedIndex / 3));
                Repaint();
            }
        }
        else
        {
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.CircleCap))
            {
                selectedIndex = index;
                newRotation = Quaternion.LookRotation(spline.GetDirection(selectedIndex / 3));
                Repaint();
            }
        }

        if (selectedIndex == index)
        {
            switch(Tools.current)
            {
                case Tool.Rotate:
                    ShowRotationHandle(selectedIndex);
                    break;
                case Tool.Move:
                default:
                    ShowPositionHandle(selectedIndex);
                    break;
            }
        }
        return point;
    }
    private void ShowPositionHandle(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        EditorGUI.BeginChangeCheck();
        point = Handles.PositionHandle(point, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
        }
    }
    private void ShowRotationHandle(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        EditorGUI.BeginChangeCheck();
        Quaternion rotation = Handles.RotationHandle(newRotation, point);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Rotate Point");
            EditorUtility.SetDirty(spline);
            newRotation = rotation;
            float angle = Quaternion.Angle(Quaternion.LookRotation(spline.GetDirection(index / 3)), newRotation);
            spline.SetControlRotation(index, angle);
        }
    }
}
