using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        if (Directory.Exists("working"))
            Directory.Delete("working", true);

        Directory.CreateDirectory("working");

        isCapturing = true;
    }

    public void StopCapturing()
    {
        Time.captureFramerate = 0;
        sequencer.Pause();

        isCapturing = false;
    }

    private void Update()
    {
        if (isCapturing)
        {
            if (sequencer.PlayerState == PlayerState.Playing)
            {
                string name = string.Format("./working/img{0:D05}.png", frame++);
                ScreenCapture.CaptureScreenshot(name);

                PlaybackNotifier.SendImageCaptured(name);
            }
            else if (sequencer.PlayerState == PlayerState.Finished)
            {
                // sending null tells the client there is nothing more to export
                PlaybackNotifier.SendImageCaptured(null);
                StopCapturing();
            }
        }
    }

#if !UNITY_WEBGL || UNITY_EDITOR
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

