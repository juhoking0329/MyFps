#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateMixerHackEditor
{
    public static void Execute()
    {
        string dir = "Assets/MyFps/Audio";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string path = "Assets/MyFps/Audio/MainMixer.mixer";
        if (AssetDatabase.LoadAssetAtPath<Object>(path) == null)
        {
            var type = System.Reflection.Assembly.Load("UnityEditor").GetType("UnityEditor.Audio.AudioMixerController");
            if (type != null)
            {
                var mixer = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(mixer, path);
                Debug.Log("Mixer created successfully via Reflection hack.");
            }
        }
        else
        {
            Debug.Log("Mixer already exists.");
        }
        FileUtil.DeleteFileOrDirectory("Assets/CreateMixerHackEditor.cs");
    }
}
#endif
