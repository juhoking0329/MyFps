using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MyFps
{
    // 직렬화가 가능한 게임 데이터 클래스
    [System.Serializable]
    public class GameData
    {
        public int savedSceneIndex;
        public int savedAmmoCount;

        public GameData(int sceneIndex, int ammoCount)
        {
            this.savedSceneIndex = sceneIndex;
            this.savedAmmoCount = ammoCount;
        }
    }

    /// <summary>
    /// 세이브/로드를 전담하는 매니저 클래스 (PlayerPrefs 및 BinaryFormatter 방식 모두 지원)
    /// </summary>
    public static class SaveLoadManager
    {
        private static string SaveFilePath => Application.persistentDataPath + "/gamesave.dat";

        public static GameData pendingLoadData = null;
        public static bool isNewGame = false;

        #region PlayerPrefs Method (과제 1)
        public static void SaveSceneWithPlayerPrefs(int sceneIndex)
        {
            PlayerPrefs.SetInt("SavedSceneIndex", sceneIndex);
            PlayerPrefs.Save();
            Debug.Log($"[PlayerPrefs] Saved Scene Index: {sceneIndex}");
        }

        public static int LoadSceneWithPlayerPrefs()
        {
            return PlayerPrefs.GetInt("SavedSceneIndex", -1);
        }

        public static bool HasPlayerPrefsSave()
        {
            return PlayerPrefs.HasKey("SavedSceneIndex");
        }
        #endregion

        #region File System - BinaryFormatter Method
        public static void SaveGameWithFile(int sceneIndex, int ammoCount)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(SaveFilePath, FileMode.Create);

            GameData data = new GameData(sceneIndex, ammoCount);
            formatter.Serialize(stream, data);
            stream.Close();

            Debug.Log($"[BinaryFormatter] Saved Game: Scene {sceneIndex}, Ammo {ammoCount} to {SaveFilePath}");
        }

        public static GameData LoadGameWithFile()
        {
            if (File.Exists(SaveFilePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(SaveFilePath, FileMode.Open);

                GameData data = formatter.Deserialize(stream) as GameData;
                stream.Close();

                return data;
            }
            else
            {
                Debug.LogWarning("Save file not found in " + SaveFilePath);
                return null;
            }
        }

        public static bool HasFileSave()
        {
            return File.Exists(SaveFilePath);
        }
        #endregion
    }
}
