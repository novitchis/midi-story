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

    // default midi file bpm
    public float bpm = 120f; 

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
        // is the amount would move per second if the display can fit 60 seconds at once
        // in our case we want to display about 4 seconds at a time, in this case the fall
        // should be increased
        float pointsPerSecond = (bpm / 60f) * quarterNoteLength;
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

    void ApplyMessages(List<IMidiEvent> messages)
    {
        if (messages != null)
        {
            foreach (var message in messages)
            {
                //Debug.Log("Message: " + message);

                if (message is MidiMetaEvent) {
                    MidiMetaEvent midiMetaEvent = (MidiMetaEvent)message;

                    // set tempo event
                    if (midiMetaEvent.type == 0x51)
                    {
                        bpm = GetBPM(midiMetaEvent.bytes);
                        sequencer.SetBPM(bpm);
                    }
                }
                else if (message is MidiEvent)
                {
                    MidiEvent midiEvent = (MidiEvent)message;
                    if ((midiEvent.status & 0xf0) == 0x90)
                    {
                        // the range of keys visible on keyboard is: 21 - 108
                        if (midiEvent.data1 < 21 || midiEvent.data1 > 108)
                            continue;

                        int octaveOffset = midiEvent.data1 % 12;
                        //Debug.Log("Note On: " + notesNames[octaveOffset] + " [ " + m.ToString() + " ] " );

                        if (midiEvent.data2 != 0)
                        {
                            playingNotes[midiEvent.data1] = Instantiate(
                                octaveBlackKeysIndexes[octaveOffset] == 1 ? blackTile : whiteTile,
                                GetNotePosition(midiEvent.data1), 
                                transform.rotation, 
                                transform
                            );
                        }
                        else
                        {
                            finishedNotes.Add(playingNotes[midiEvent.data1]);
                            playingNotes[midiEvent.data1] = null;
                        }
                    }
                    else if ((midiEvent.status & 0xf0) == 0x80)
                    {
                        finishedNotes.Add(playingNotes[midiEvent.data1]);
                        playingNotes[midiEvent.data1] = null;

                        int octaveOffset = (int)midiEvent.data1 % 12;
                        //Debug.Log("Note Off: " + notesNames[octaveOffset]);
                    }
                }

            }
        }
    }

    private float GetBPM(byte[] bytes)
    {
        int microsecondsPerQuarterNote = 0;
        foreach (byte tempoByte in bytes)
        {
            microsecondsPerQuarterNote <<= 8;
            microsecondsPerQuarterNote |= tempoByte;
        }

        return 60 * (1000000f / microsecondsPerQuarterNote);
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
