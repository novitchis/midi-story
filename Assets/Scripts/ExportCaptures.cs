using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ExportCaptures: MonoBehaviour
{
    public Sequencer sequencer;
    private int frame = 0;

    private bool isCapturing = false;

    public void StartCapturing()
    {
        frame = 0;
        sequencer.GoToStart();
        Time.captureFramerate = 60;

        isCapturing = true;
    }

    public void StopCapturing()
    {
        Time.captureFramerate = 0;
        isCapturing = false;
    }

    private void Update()
    {
        if (isCapturing && sequencer.PlayerState == PlayerState.Playing)
        {
            string name = string.Format("img{0:D05}.png", frame++);

            // Capture the screenshot to the specified file.
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] pngBytes = texture.EncodeToPNG();

            PlaybackNotifier.ImageCaptured(name, pngBytes);
        }
    }

#if !UNITY_WEBGL
    void OnGUI()
    {
        if (sequencer.PlayerState == PlayerState.Playing)
            return;

        if (GUI.Button(new Rect(10, 50, 100, 30), isCapturing ? "Stop Capture" : "Start Capture"))
        {
            if (isCapturing)
                StopCapturing();
            else
                StartCapturing();
        }
    }
#endif
}

