using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramePerSecond : MonoBehaviour
{
    private Rect fpsRect;
    private GUIStyle style;
    private float fps;

    // Start is called before the first frame update
    void Start()
    {
        fpsRect = new Rect(10, 10, 100, 100);
        style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        StartCoroutine(RecalculateFPS());
    }

    private IEnumerator RecalculateFPS()
    {
        while(true)
        {
            fps = 1 / Time.deltaTime;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnGUI()
    {
        GUI.Label(fpsRect, string.Format("FPS: {0:0.0}", fps), style);
    }
}
