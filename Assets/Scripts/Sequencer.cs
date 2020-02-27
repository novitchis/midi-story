using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmfLite;
using System;
using UnityEngine.Networking;
using System.Linq;

public class Sequencer : MonoBehaviour
{
    public GameObject blackTile;
    public GameObject whiteTile;

    public float width;
    public float quarterNoteLength;
    public float speedMultiplier;

    public TextAsset sourceFile;
    MidiFileContainer midiFile;
    MidiSequencer sequencer;

    private int[] octaveBlackKeysIndexes = new int[] { 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0 };
    private int[] octaveBlackKeys = new int[] { 0, 0, 1, 1, 2, 2, 2, 3, 3, 4, 4, 5 };
    private string[] notesNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    private GameObject[] playingNotes = new GameObject[108];
    private List<GameObject> finishedNotes = new List<GameObject>();

    private int bpm = 80; 

    void ResetAndPlay()
    {
        transform.position = new Vector3(-9, 2, 0);

        finishedNotes.ForEach(Destroy);

        int index = 0;
        foreach (GameObject noteObject in playingNotes)
        {
            if (noteObject != null)
            {
                Destroy(noteObject);
                playingNotes[index++] = null;
            }
        }

        // TODO: get bpm from the midi file header
        sequencer = new MidiSequencer(midiFile.tracks, midiFile.division, bpm);
        ApplyMessages(sequencer.Start());
    }

#if DEBUG
    IEnumerator Start()
    {
        midiFile = MidiFileLoader.Load(sourceFile.bytes);
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay();
    }
#else
    void Start()
    {
        GetImage.GetImageFromUserAsync(gameObject.name, "ReceiveImage");
    }
#endif

    IEnumerator LoadMidiFile(byte[] fileBytes)
    {
        midiFile = MidiFileLoader.Load(fileBytes);
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay();
    }

    // Update is called once per frame
    void Update()
    {
        // is the amount would move per second if we the display can fit 60 seconds at once
        // in our case we want to display about 4 seconds at a time, in this case the fall
        // should be increased
        float pointsPerSecond = (bpm / 60) * quarterNoteLength;

        float pointsDown = pointsPerSecond * Time.deltaTime * speedMultiplier;

        transform.Translate(Vector3.back * pointsDown);

        foreach (GameObject noteObject in playingNotes)
        {
            if (noteObject != null)
            {
                noteObject.transform.localScale += Vector3.forward * pointsDown;
                noteObject.transform.Translate(Vector3.forward * pointsDown / 2);
            }
        }

        if (sequencer != null && sequencer.Playing)
        {
            ApplyMessages(sequencer.Advance(Time.deltaTime));
        }
    }

    void ApplyMessages(List<MidiEvent> messages)
    {
        if (messages != null)
        {
            foreach (var m in messages)
            {
                if ((m.status & 0xf0) == 0x90)
                {
                    // the range of keys visible on keyboard is: 21 - 108
                    if (m.data1 < 21 || m.data1 > 108)
                        continue;

                    int octaveOffset = m.data1 % 12;
                    Debug.Log("Note On: " + notesNames[octaveOffset]);

                    playingNotes[m.data1] = Instantiate(
                        octaveBlackKeysIndexes[octaveOffset] == 1 ? blackTile : whiteTile,
                        GetNotePosition(m.data1), 
                        transform.rotation, 
                        transform
                    );
                }
                else if ((m.status & 0xf0) == 0x80)
                {
                    finishedNotes.Add(playingNotes[m.data1]);
                    playingNotes[m.data1] = null;

                    int octaveOffset = (int)m.data1 % 12;
                    Debug.Log("Note Off: " + notesNames[octaveOffset]);
                }
            }
        }
    }

    private Vector3 GetNotePosition(byte note)
    {
        int precedingBlackKeys = (note / 12) * 5 + octaveBlackKeys[note % 12];
        
        // 12 is the white keys count not visible on the piano on bottom
        int precedingWhiteKeys = note - precedingBlackKeys - 12;
        int totalWhiteKeys = 51;

        float whiteKeyWidth = width / totalWhiteKeys;
        float offsetX = whiteKeyWidth * precedingWhiteKeys;

        //black keys need an offset
        if (octaveBlackKeysIndexes[note % 12] == 1)
            offsetX -= whiteKeyWidth / 2;

        
        return new Vector3(transform.position.x + offsetX, 1, 0);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Restart"))
        {
            ResetAndPlay();
        }
    }

    public void ReceiveImage(string dataUrl)
    {
        StartCoroutine(LoadBlob(dataUrl));
    }

    IEnumerator LoadBlob(string url)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (!webRequest.isNetworkError && !webRequest.isHttpError)
        {
            yield return LoadMidiFile(webRequest.downloadHandler.data);
        }
    }
}
