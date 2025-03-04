using UnityEngine;
using UnityEditor;
using System.IO;

public class ScreenshotTaker : EditorWindow
{
    private int width = 1920;
    private int height = 1080;
    private string folderPath = "Assets/Screenshots";
    private bool customFolderSelected = false;

    [MenuItem("Tools/Saqib Ali/Take Screenshot")]
    public static void ShowWindow()
    {
        GetWindow<ScreenshotTaker>("Screenshot Taker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Screenshot Settings", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        if (!customFolderSelected)
        {
            folderPath = "Assets/Screenshots";
        }

        EditorGUILayout.LabelField("Folder Path", folderPath);

        if (GUILayout.Button("Choose Folder"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Screenshot Folder", folderPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                folderPath = selectedPath;
                customFolderSelected = true;
            }
        }

        GUILayout.Label("If no folder is selected, screenshots will be saved in 'Assets/Screenshots'.", EditorStyles.wordWrappedLabel);

        if (GUILayout.Button("Take Screenshot"))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = GetUniqueFileName();
        string fullPath = Path.Combine(folderPath, fileName);

        ScreenCapture.CaptureScreenshot(fullPath);
        Debug.Log($"Screenshot saved to {fullPath}");
        AssetDatabase.Refresh(); // Refresh Unity asset database to make the file visible in the editor
    }

    private string GetUniqueFileName()
    {
        int index = 0;
        string fileName;

        do
        {
            fileName = index == 0 ? "screenshot.png" : $"screenshot{index}.png";
            index++;
        } while (File.Exists(Path.Combine(folderPath, fileName)));

        return fileName;
    }
}
