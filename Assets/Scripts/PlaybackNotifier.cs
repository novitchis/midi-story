using UnityEngine;
using System.Runtime.InteropServices;

public class PlaybackNotifier
{
    // Import the JSLib as following. Make sure the
    // names match with the JSLib file we've just created.
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void Finished();
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
}