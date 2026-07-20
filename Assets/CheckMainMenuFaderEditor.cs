#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MyFps;

public class CheckMainMenuFaderEditor
{
    public static void Execute()
    {
        string scenePath = "Assets/MyFps/Scenes/MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        SceneFader fader = Object.FindFirstObjectByType<SceneFader>();
        if (fader != null)
        {
            Debug.Log($"SceneFader found in MainMenu. isFadeIn: {new SerializedObject(fader).FindProperty("isFadeIn").boolValue}");
        }
        else
        {
            Debug.Log("SceneFader NOT FOUND in MainMenu.");
        }

        FileUtil.DeleteFileOrDirectory("Assets/CheckMainMenuFaderEditor.cs");
    }
}
#endif
