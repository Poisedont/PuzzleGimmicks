using System.Collections;
using UnityEngine;

public class CameraControl : Singleton<CameraControl>
{
    public static Camera cam;
    public bool m_playing = false;
    ////////////////////////////////////////////////////////////////////////////////
    public override void Awake()
    {
        base.Awake();

        float deviceRatio = (Screen.height * 1.0f) / Screen.width;
        float baseRatio = (16 * 1.0f) / 9;

        cam = GetComponent<Camera>();
        if (!cam)
        {
            cam = Camera.main;
        }

        cam.orthographicSize *= (deviceRatio/baseRatio);

        //if (deviceRatio - baseRatio > 0.1f)
        //{
        //    cam.orthographicSize = 7;
        //}
        //else
        //{
        //    cam.orthographicSize = 6;
        //}
    }
    public IEnumerator HideFieldRoutine()
    {
        if (!m_playing)
            yield break;

        m_playing = false;

        float t = 0;

        Vector3 position = transform.position;

        while (t < 1)
        {
            t += (-Mathf.Abs(0.5f - t) + 0.5f + 0.05f) * Time.unscaledDeltaTime * 6;
            transform.position = Vector3.Lerp(position, new Vector3(0, 10, -10), t);
            yield return 0;
        }

        yield break;
    }

    public void HideField() {
        
    }


}