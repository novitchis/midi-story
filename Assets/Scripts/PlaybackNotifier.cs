using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

public class PlaybackNotifier
{
    // Import the JSLib as following. Make sure the
    // names match with the JSLib file we've just created.
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void Finished();

        [DllImport("__Internal")]
        private static extern void FileLoaded(string fileInfo);

        [DllImport("__Internal")]
        private static extern void ImageCaptured(string name);
#endif

    // Then create a function that is going to trigger
    // the imported function from our JSLib.

    public static void SendFinished()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Finished();
#else
        Debug.LogWarning("Not implemented in this platform");
#endif

    }

    public static void SendFileLoaded(FileInfo fileInfo)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FileLoaded(JsonUtility.ToJson(fileInfo));
#else
        Debug.LogWarning("Not implemented in this platform: SendFileLoaded " + JsonUtility.ToJson(fileInfo));
#endif

    }

    public static void SendImageCaptured(string name)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ImageCaptured(name);
#else
        Debug.LogWarning("Not implemented in this platform");
#endif
    }
}