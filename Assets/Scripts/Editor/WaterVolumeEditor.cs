using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaterVolume))]
public class WaterVolumeEditor : Editor
{
    private SerializedProperty _surfaceHeightProp;

    private void OnEnable()
    {
        _surfaceHeightProp = serializedObject.FindProperty("surfaceHeight");
    }

    private void OnSceneGUI()
    {
        WaterVolume water = (WaterVolume)target;
        Vector3 handlePosition = new Vector3(water.transform.position.x, water.SurfaceY, water.transform.position.z);
        float handleSize = HandleUtility.GetHandleSize(handlePosition) * 0.5f;

        EditorGUI.BeginChangeCheck();

        Handles.color = Color.cyan;
        Vector3 newPosition = Handles.Slider(handlePosition, Vector3.up, handleSize, Handles.ArrowHandleCap, 0f);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.Update();
            _surfaceHeightProp.floatValue += newPosition.y - handlePosition.y;
            serializedObject.ApplyModifiedProperties();
        }

        Handles.Label(handlePosition + Vector3.up * handleSize, $"Water Level: {water.SurfaceY:F2}");
    }
}
