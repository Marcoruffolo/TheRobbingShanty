using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPainterWindow : EditorWindow
{
    private const string DefaultBrushFolder = "Assets/Scripts/ScriptableObjects/ObjectPainter";

    [SerializeField] private ObjectPainterBrush brush;
    [SerializeField] private PaintedObjectField field;
    [SerializeField] private bool toolEnabled = true;
    [SerializeField] private ObjectPainterPaintMode mode = ObjectPainterPaintMode.Paint;
    [SerializeField] private bool paintOnDrag = true;
    [SerializeField] private bool showAdvancedSettings = true;

    private Editor brushEditor;
    private Vector2 scrollPosition;

    internal ObjectPainterBrush Brush => brush;
    internal PaintedObjectField Field => field;
    internal bool ToolEnabled => toolEnabled;
    internal ObjectPainterPaintMode Mode => mode;
    internal bool PaintOnDrag => paintOnDrag;

    [MenuItem("Tools/The Robbing Shanty/Object Painter")]
    public static void Open()
    {
        GetWindow<ObjectPainterWindow>("Object Painter");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGui;
        Selection.selectionChanged += OnSelectionChanged;
        Undo.undoRedoPerformed += OnUndoRedo;
        OnSelectionChanged();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGui;
        Selection.selectionChanged -= OnSelectionChanged;
        Undo.undoRedoPerformed -= OnUndoRedo;

        if (brushEditor != null)
            DestroyImmediate(brushEditor);
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("The Robbing Shanty Object Painter", EditorStyles.boldLabel);

        DrawToolControls();
        DrawPresetSection();
        DrawPaintedFieldSection();

        if (brush != null)
            DrawBrushInspector();

        EditorGUILayout.EndScrollView();
    }

    internal PaintedObjectField EnsureField()
    {
        if (field == null)
            CreateField(false);

        if (field != null && field.Brush != brush)
            AssignBrushToField();

        return field;
    }

    internal void RebuildGeneratedContent()
    {
        if (field == null)
            return;

        field.NotifyDataChanged(true);
        EditorUtility.SetDirty(field);
        SceneView.RepaintAll();
    }

    private void DrawToolControls()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Pintado", EditorStyles.boldLabel);
            toolEnabled = EditorGUILayout.Toggle("Herramienta activa", toolEnabled);
            mode = (ObjectPainterPaintMode)EditorGUILayout.EnumPopup("Modo", mode);
            paintOnDrag = EditorGUILayout.Toggle("Pintar al arrastrar", paintOnDrag);
        }
    }

    private void DrawPresetSection()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUI.BeginChangeCheck();
            brush = (ObjectPainterBrush)EditorGUILayout.ObjectField("Preset", brush, typeof(ObjectPainterBrush), false);

            if (EditorGUI.EndChangeCheck())
            {
                if (field != null && brush == null)
                    brush = field.Brush;
            }

            if (brush == null)
            {
                EditorGUILayout.HelpBox("Selecciona prefabs en Project y crea un preset. El contenedor de escena se crea solo cuando empezas a pintar.", MessageType.Info);
            }
            else if (!brush.HasValidVariants)
            {
                EditorGUILayout.HelpBox("Este preset todavia no tiene prefabs. Agrega prefabs seleccionados para poder pintar.", MessageType.Warning);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Nuevo preset"))
                    CreateBrushAsset(false);

                using (new EditorGUI.DisabledScope(GetSelectedPrefabSources().Count == 0))
                {
                    if (GUILayout.Button("Preset desde seleccion"))
                        CreateBrushAsset(true);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(brush == null || GetSelectedPrefabSources().Count == 0))
                {
                    if (GUILayout.Button("Agregar seleccion"))
                        AddSelectedPrefabsToBrush();
                }

                using (new EditorGUI.DisabledScope(brush == null))
                {
                    if (GUILayout.Button("Seleccionar preset"))
                        Selection.activeObject = brush;
                }
            }
        }
    }

    private void DrawPaintedFieldSection()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Guardado en escena", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            field = (PaintedObjectField)EditorGUILayout.ObjectField("Contenedor", field, typeof(PaintedObjectField), true);
            if (EditorGUI.EndChangeCheck() && field != null && field.Brush != null)
                brush = field.Brush;

            if (field != null)
                EditorGUILayout.LabelField("Instancias pintadas", field.InstanceCount.ToString());
            else
                EditorGUILayout.HelpBox("No hace falta crear esto a mano: si hay preset, se crea automaticamente al pintar.", MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(brush == null))
                {
                    if (GUILayout.Button("Crear contenedor ahora"))
                        CreateField(true);
                }

                using (new EditorGUI.DisabledScope(Selection.activeGameObject == null || Selection.activeGameObject.GetComponent<PaintedObjectField>() == null))
                {
                    if (GUILayout.Button("Usar seleccionado"))
                        UseSelectedField();
                }
            }

            using (new EditorGUI.DisabledScope(field == null || brush == null))
            {
                if (GUILayout.Button("Asignar preset al contenedor"))
                    AssignBrushToField();
            }
        }
    }

    private void DrawBrushInspector()
    {
        EditorGUILayout.Space(6);
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Configuracion del preset", true);
        if (!showAdvancedSettings)
            return;

        Editor.CreateCachedEditor(brush, null, ref brushEditor);
        if (brushEditor == null)
            return;

        EditorGUI.BeginChangeCheck();
        brushEditor.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            if (field != null)
                field.NotifyDataChanged(true);

            SceneView.RepaintAll();
        }
    }

    private void DuringSceneGui(SceneView sceneView)
    {
        ObjectPainterSceneTool.HandleSceneGUI(sceneView, this);
    }

    private void OnSelectionChanged()
    {
        Object activeObject = Selection.activeObject;
        if (activeObject is ObjectPainterBrush selectedBrush)
        {
            brush = selectedBrush;
            Repaint();
            return;
        }

        GameObject activeGameObject = Selection.activeGameObject;
        if (activeGameObject == null)
            return;

        PaintedObjectField selectedField = activeGameObject.GetComponent<PaintedObjectField>();
        if (selectedField == null)
            return;

        field = selectedField;
        if (field.Brush != null)
            brush = field.Brush;

        Repaint();
    }

    private void OnUndoRedo()
    {
        if (field != null)
            field.NotifyDataChanged(true);

        SceneView.RepaintAll();
        Repaint();
    }

    private void CreateBrushAsset(bool addSelectedPrefabs)
    {
        EnsureAssetFolder(DefaultBrushFolder);
        string path = AssetDatabase.GenerateUniqueAssetPath($"{DefaultBrushFolder}/ObjectPainterBrush.asset");
        var newBrush = CreateInstance<ObjectPainterBrush>();
        AssetDatabase.CreateAsset(newBrush, path);

        brush = newBrush;

        if (addSelectedPrefabs)
            AddSelectedPrefabsToBrush();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = brush;
    }

    private void CreateField(bool selectCreated)
    {
        if (brush == null)
            return;

        var go = new GameObject($"Painted Field - {brush.name}");
        Undo.RegisterCreatedObjectUndo(go, "Create Painted Object Field");

        field = go.AddComponent<PaintedObjectField>();
        field.SetBrush(brush);

        if (selectCreated)
            Selection.activeGameObject = go;

        EditorUtility.SetDirty(field);
        SceneView.RepaintAll();
    }

    private void UseSelectedField()
    {
        GameObject activeGameObject = Selection.activeGameObject;
        if (activeGameObject == null)
            return;

        field = activeGameObject.GetComponent<PaintedObjectField>();
        if (field != null && field.Brush != null)
            brush = field.Brush;

        Repaint();
    }

    private void AssignBrushToField()
    {
        if (field == null || brush == null)
            return;

        Undo.RecordObject(field, "Assign Object Painter Brush");
        field.SetBrush(brush);
        EditorUtility.SetDirty(field);
    }

    private void AddSelectedPrefabsToBrush()
    {
        if (brush == null)
            return;

        List<GameObject> selectedPrefabs = GetSelectedPrefabSources();
        if (selectedPrefabs.Count == 0)
            return;

        Undo.RecordObject(brush, "Add Prefabs To Object Painter Preset");
        brush.variants ??= new List<ObjectPainterPrefabVariant>();

        for (int i = 0; i < selectedPrefabs.Count; i++)
        {
            GameObject prefab = selectedPrefabs[i];
            bool alreadyAdded = false;

            for (int variantIndex = 0; variantIndex < brush.variants.Count; variantIndex++)
            {
                if (brush.variants[variantIndex] != null && brush.variants[variantIndex].prefab == prefab)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded)
                brush.variants.Add(new ObjectPainterPrefabVariant { prefab = prefab, weight = 1f });
        }

        EditorUtility.SetDirty(brush);
        AssetDatabase.SaveAssets();
    }

    private static List<GameObject> GetSelectedPrefabSources()
    {
        var selectedPrefabs = new List<GameObject>();

        Object[] selectedObjects = Selection.objects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] is not GameObject selectedGameObject)
                continue;

            GameObject prefab = selectedGameObject;
            if (!AssetDatabase.Contains(prefab))
                prefab = PrefabUtility.GetCorrespondingObjectFromSource(selectedGameObject);

            if (prefab == null || !AssetDatabase.Contains(prefab) || selectedPrefabs.Contains(prefab))
                continue;

            selectedPrefabs.Add(prefab);
        }

        return selectedPrefabs;
    }

    private static void EnsureAssetFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}

public enum ObjectPainterPaintMode
{
    Paint,
    Erase
}
