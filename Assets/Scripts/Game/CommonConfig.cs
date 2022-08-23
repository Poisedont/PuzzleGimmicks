using UnityEngine;

public class CommonConfig : MonoBehaviour {
    public static CommonConfig main;

    void Awake() {
        main = this;
    }

    public bool square_combination = true;
    public float chip_acceleration = 20f;
    public float chip_max_velocity = 17f;
    public float swap_duration = 0.1f;
    public int refilling_time = 30;
    public int dailyreward_hour = 10;
    public int lifes_limit = 5;
    public float slot_offset = 0.7f;
    public float music_volume_max = 0.4f;
}