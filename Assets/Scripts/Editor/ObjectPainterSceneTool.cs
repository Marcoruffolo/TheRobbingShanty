using UnityEditor;
using UnityEngine;

public static class ObjectPainterSceneTool
{
    private static bool isDragging;
    private static double nextDragStampTime;
    private static int activeUndoGroup = -1;
    private static int placedThisStroke;

    public static void HandleSceneGUI(SceneView sceneView, ObjectPainterWindow window)
    {
        if (window == null || !window.ToolEnabled || window.Brush == null)
            return;

        Event current = Event.current;
        if (current == null)
            return;

        if (!current.alt && current.type == EventType.Layout)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);
        }

        ObjectPainterBrush brush = window.Brush;
        Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);

        bool hasHit = Physics.Raycast(
            ray,
            out RaycastHit hit,
            Mathf.Infinity,
            brush.surfaceMask,
            QueryTriggerInteraction.Ignore
        );

        if (hasHit)
            DrawBrushPreview(hit, brush, window.Mode);

        if (current.alt || current.button != 0)
            return;

        switch (current.type)
        {
            case EventType.MouseDown:
                if (!hasHit)
                    return;

                BeginStroke(window.Mode);
                ApplyStamp(window, hit);
                current.Use();
                break;

            case EventType.MouseDrag:
                if (!isDragging || !window.PaintOnDrag || !hasHit)
                    return;

                if (EditorApplication.timeSinceStartup < nextDragStampTime)
                {
                    current.Use();
                    return;
                }

                ApplyStamp(window, hit);
                nextDragStampTime = EditorApplication.timeSinceStartup + brush.dragStampInterval;
                current.Use();
                break;

            case EventType.MouseUp:
                if (!isDragging)
                    return;

                EndStroke(window);
                current.Use();
                break;
        }
    }

    private static void DrawBrushPreview(RaycastHit hit, ObjectPainterBrush brush, ObjectPainterPaintMode mode)
    {
        Color color = mode == ObjectPainterPaintMode.Paint
            ? new Color(0.15f, 0.85f, 0.35f, 0.85f)
            : new Color(1f, 0.25f, 0.2f, 0.85f);

        Handles.color = color;
        Handles.DrawWireDisc(hit.point, hit.normal, brush.brushRadius);

        Handles.color = new Color(color.r, color.g, color.b, 0.08f);
        Handles.DrawSolidDisc(hit.point, hit.normal, brush.brushRadius);
    }

    private static void BeginStroke(ObjectPainterPaintMode mode)
    {
        isDragging = true;
        placedThisStroke = 0;
        nextDragStampTime = EditorApplication.timeSinceStartup;
        Undo.IncrementCurrentGroup();
        activeUndoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(mode == ObjectPainterPaintMode.Paint ? "Paint Object Instances" : "Erase Object Instances");
    }

    private static void EndStroke(ObjectPainterWindow window)
    {
        isDragging = false;
        placedThisStroke = 0;
        window.RebuildGeneratedContent();

        if (activeUndoGroup >= 0)
        {
            Undo.CollapseUndoOperations(activeUndoGroup);
            activeUndoGroup = -1;
        }
    }

    private static void ApplyStamp(ObjectPainterWindow window, RaycastHit hit)
    {
        if (window.Mode == ObjectPainterPaintMode.Paint)
            PaintStamp(window, hit);
        else
            EraseStamp(window, hit);
    }

    private static void PaintStamp(ObjectPainterWindow window, RaycastHit hit)
    {
        ObjectPainterBrush brush = window.Brush;
        if (!brush.HasValidVariants || placedThisStroke >= brush.maxInstancesPerStroke)
            return;

        PaintedObjectField field = window.EnsureField();
        if (field == null)
            return;

        Undo.RecordObject(field, "Paint Object Instances");

        int remainingForStroke = brush.maxInstancesPerStroke - placedThisStroke;
        int attempts = Mathf.Min(brush.instancesPerStamp, remainingForStroke);
        int placedThisStamp = 0;

        for (int i = 0; i < attempts; i++)
        {
            if (!TryFindPlacement(hit, brush, out RaycastHit placementHit))
                continue;

            Vector3 position = placementHit.point + placementHit.normal * brush.surfaceOffset;
            if (brush.minSpacing > 0f && field.HasInstanceWithin(position, brush.minSpacing))
                continue;

            int variantIndex = brush.PickVariantIndex(Random.value);
            if (variantIndex < 0)
                continue;

            Quaternion rotation = BuildRotation(placementHit.normal, brush);
            float scaleValue = Random.Range(brush.minScale, brush.maxScale);
            Vector3 scale = Vector3.one * scaleValue;

            field.AddInstance(new PaintedObjectInstance(position, rotation, scale, placementHit.normal, variantIndex));
            placedThisStroke++;
            placedThisStamp++;

            if (placedThisStroke >= brush.maxInstancesPerStroke)
                break;
        }

        if (placedThisStamp > 0)
        {
            EditorUtility.SetDirty(field);
            SceneView.RepaintAll();
        }
    }

    private static void EraseStamp(ObjectPainterWindow window, RaycastHit hit)
    {
        PaintedObjectField field = window.Field;
        ObjectPainterBrush brush = window.Brush;

        if (field == null || brush == null)
            return;

        Undo.RecordObject(field, "Erase Object Instances");
        int removed = field.RemoveInstancesInRadius(hit.point, brush.brushRadius);

        if (removed > 0)
        {
            EditorUtility.SetDirty(field);
            SceneView.RepaintAll();
        }
    }

    private static bool TryFindPlacement(RaycastHit brushHit, ObjectPainterBrush brush, out RaycastHit placementHit)
    {
        Vector2 offset = Random.insideUnitCircle * brush.brushRadius;
        Vector3 tangent = Vector3.Cross(brushHit.normal, Vector3.up);

        if (tangent.sqrMagnitude < 0.001f)
            tangent = Vector3.Cross(brushHit.normal, Vector3.right);

        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(brushHit.normal, tangent).normalized;

        Vector3 samplePoint = brushHit.point + tangent * offset.x + bitangent * offset.y;
        Vector3 rayOrigin = samplePoint + brushHit.normal * (brush.projectionDistance * 0.5f);

        return Physics.Raycast(
            rayOrigin,
            -brushHit.normal,
            out placementHit,
            brush.projectionDistance,
            brush.surfaceMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private static Quaternion BuildRotation(Vector3 normal, ObjectPainterBrush brush)
    {
        Vector3 up = brush.alignToSurface ? normal : Vector3.up;
        Quaternion alignRotation = Quaternion.FromToRotation(Vector3.up, up);

        if (!brush.randomYaw)
            return alignRotation;

        return Quaternion.AngleAxis(Random.Range(0f, 360f), up) * alignRotation;
    }
}
