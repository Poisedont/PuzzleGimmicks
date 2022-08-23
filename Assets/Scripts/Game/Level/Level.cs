using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
    public static Dictionary<int, LevelProfile> all = new Dictionary<int, LevelProfile>();
    public LevelProfile profile;

    public static void LoadLevel(int key) {
        LevelProfile.main = all[key];
    }
    #if UNITY_EDITOR
    public static void TestLevel(int l) {
        LevelProfile.main = all[l];
        FieldManager.Instance.StartLevel();
        PlayerPrefs.DeleteKey("TestLevel");
    }
    #endif
}