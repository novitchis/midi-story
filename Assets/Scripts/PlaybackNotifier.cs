using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

public class PlaybackNotifier
{
    // Import the JSLib as following. Make sure the
    // names match with the JSLib file we've just created.
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void Finished();

        [DllImport("__Internal")]
        private static extern void FileLoaded();

        [DllImport("__Internal")]
        private static extern void ImageCaptured(string name, byte[] pngBytes);
#endif

    // Then create a function that is going to trigger
    // the imported function from our JSLib.

    public static void SendFinished()
    {
#if UNITY_WEBGL
        Finished();
#else
            Debug.LogError("Not implemented in this platform");
#endif

    }

    public static void SendFileLoaded()
    {
#if UNITY_WEBGL
        FileLoaded();
#else
            Debug.LogError("Not implemented in this platform");
#endif

    }

    public static void ImageCaptured(string name, byte[] pngBytes)
    {
#if UNITY_WEBGL
        ImageCaptured(name, pngBytes);
#else
        string path = string.Format(@"C:\Users\silviun\Desktop\Screenshots\{0}", name);
        File.WriteAllBytes(path, pngBytes);
#endif
    }
}