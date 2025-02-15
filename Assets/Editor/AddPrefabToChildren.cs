using UnityEditor;
using UnityEngine;

public class AddPrefabToChildren : EditorWindow
{
    private GameObject prefab;
    private int childIndex = 0;

    private bool resetPosition = true;
    private bool resetRotation = true;
    private bool resetScale = false;
    private bool matchParentHeight = false;

    private bool selectParents = false;
    private bool selectNewChildren = true;

    private Vector3 positionOffset = Vector3.zero;
    private Vector3 randomRotationMin = Vector3.zero;
    private Vector3 randomRotationMax = Vector3.zero;
    private string customName = "Child_{parentName}_{index}";

    [MenuItem("Tools/Saqib Ali/Add Prefab to Children")]
    private static void Init()
    {
        var window = GetWindow<AddPrefabToChildren>();
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Prefab Settings:", EditorStyles.boldLabel);
        prefab = EditorGUILayout.ObjectField(new GUIContent("Prefab", "The prefab that will be added as a child"), prefab, typeof(GameObject), false) as GameObject;
        childIndex = EditorGUILayout.IntField(new GUIContent("Child Index", "0 for first, -1 for last"), childIndex);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Reset Transform Options:", EditorStyles.boldLabel);
        resetPosition = EditorGUILayout.Toggle(new GUIContent("Reset Position", "Resets the child's position to (0,0,0) under the parent"), resetPosition);
        resetRotation = EditorGUILayout.Toggle(new GUIContent("Reset Rotation", "Resets the child's rotation to match the parent"), resetRotation);
        resetScale = EditorGUILayout.Toggle(new GUIContent("Reset Scale", "Resets the child's scale to (1,1,1)"), resetScale);
        matchParentHeight = EditorGUILayout.Toggle(new GUIContent("Match Parent Height", "Scales the child to match the height of the parent"), matchParentHeight);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Offset & Randomization:", EditorStyles.boldLabel);
        positionOffset = EditorGUILayout.Vector3Field(new GUIContent("Position Offset", "Offsets the child's position from (0,0,0)"), positionOffset);
        randomRotationMin = EditorGUILayout.Vector3Field(new GUIContent("Random Rotation Min", "Minimum random rotation applied to the child"), randomRotationMin);
        randomRotationMax = EditorGUILayout.Vector3Field(new GUIContent("Random Rotation Max", "Maximum random rotation applied to the child"), randomRotationMax);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Naming & Selection:", EditorStyles.boldLabel);
        customName = EditorGUILayout.TextField(new GUIContent("Custom Naming Pattern", "Use {parentName} and {index} to customize names"), customName);
        selectParents = EditorGUILayout.Toggle(new GUIContent("Select Parents After", "After adding children, selects the parent objects"), selectParents);
        selectNewChildren = EditorGUILayout.Toggle(new GUIContent("Select New Children After", "After adding children, selects the new child objects"), selectNewChildren);

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Prefab to Children"))
        {
            AddPrefabToSelectedChildren();
        }
    }

    private void AddPrefabToSelectedChildren()
    {
        if (prefab == null)
        {
            Debug.LogError("Please assign a prefab.");
            return;
        }

        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        var addedObjects = new System.Collections.Generic.List<GameObject>();

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            var selectedObject = selectedObjects[i];
            var instantiatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            if (instantiatedPrefab != null)
            {
                Transform parentTransform = selectedObject.transform;

                // Store the original world scale before parenting
                Vector3 originalWorldScale = instantiatedPrefab.transform.lossyScale;

                // Parent the object
                instantiatedPrefab.transform.SetParent(parentTransform, false);

                // Apply transform resets
                if (resetPosition) instantiatedPrefab.transform.localPosition = Vector3.zero;
                if (resetRotation) instantiatedPrefab.transform.localRotation = Quaternion.identity;

                // Correct the scale so that it matches the original scale before parenting
                if (!matchParentHeight)
                {
                    instantiatedPrefab.transform.localScale = new Vector3(
                        originalWorldScale.x / parentTransform.lossyScale.x,
                        originalWorldScale.y / parentTransform.lossyScale.y,
                        originalWorldScale.z / parentTransform.lossyScale.z
                    );
                }

                // Apply position offset
                instantiatedPrefab.transform.localPosition += positionOffset;

                // Apply random rotation
                Vector3 randomRotation = new Vector3(
                    Random.Range(randomRotationMin.x, randomRotationMax.x),
                    Random.Range(randomRotationMin.y, randomRotationMax.y),
                    Random.Range(randomRotationMin.z, randomRotationMax.z)
                );
                instantiatedPrefab.transform.localEulerAngles += randomRotation;

                // Match parent height (scaling Y while keeping proportions)
                if (matchParentHeight)
                {
                    float parentHeight = GetObjectHeight(selectedObject);
                    float prefabHeight = GetObjectHeight(instantiatedPrefab);

                    if (prefabHeight > 0)
                    {
                        float scaleMultiplier = parentHeight / prefabHeight;
                        instantiatedPrefab.transform.localScale = new Vector3(
                            instantiatedPrefab.transform.localScale.x * scaleMultiplier,
                            instantiatedPrefab.transform.localScale.y * scaleMultiplier,
                            instantiatedPrefab.transform.localScale.z * scaleMultiplier
                        );
                    }
                }

                // Handle child index (-1 for last position)
                int validChildIndex = childIndex == -1 ? parentTransform.childCount - 1 : Mathf.Clamp(childIndex, 0, parentTransform.childCount - 1);
                instantiatedPrefab.transform.SetSiblingIndex(validChildIndex);

                // Apply custom naming
                instantiatedPrefab.name = customName
                    .Replace("{parentName}", selectedObject.name)
                    .Replace("{index}", i.ToString());

                Undo.RegisterCreatedObjectUndo(instantiatedPrefab, "Add Prefab to Children");
                addedObjects.Add(instantiatedPrefab);
            }
        }

        // Handle selection based on user preference
        if (selectNewChildren)
        {
            Selection.objects = addedObjects.ToArray();
        }
        else if (selectParents)
        {
            Selection.objects = selectedObjects;
        }

        Debug.Log("Prefab added with advanced options.");
    }

    private float GetObjectHeight(GameObject obj)
    {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }
        return obj.transform.lossyScale.y; // Fallback if no renderer
    }
}
