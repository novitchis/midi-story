using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmfLite;
using System;
using UnityEngine.Networking;
using System.Linq;
using Assets.Scripts;

public class Sequencer : MonoBehaviour
{
    public KeyboardAnimator keyboard;

    public GameObject blackTile;
    public GameObject whiteTile;
    public GameObject gridLine;

    public float width;
    public float pointsPerSecond = 4;

    public TextAsset sourceFile;

    private MidiFileContainer midiFile;
    private MidiSequencer sequencer = null;

    private int[] octaveBlackKeys = new int[] { 0, 0, 1, 1, 2, 2, 2, 3, 3, 4, 4, 5 };
    private string[] notesNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    private GameObject[] playingNotes = new GameObject[108];
    private List<GameObject> finishedNotes = new List<GameObject>();

    void ResetAndPlay()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

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

        // offset the position by the screen size to start notes falling from the top
        float totalTime = 8 / pointsPerSecond;
        sequencer = new MidiSequencer(midiFile.tracks, midiFile.division, 120);
        ApplyMessages(sequencer.Start(), 0, totalTime);

        while (sequencer.Playing) {
            totalTime += 0.05f;
            ApplyMessages(sequencer.Advance(0.05f), 0.05f, totalTime);
        }
    }

#if DEBUG
    IEnumerator Start()
    {
        SpawnGridLines();

        midiFile = MidiFileLoader.Load(sourceFile.bytes);
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay();
    }
#else
    void Start()
    {
        SpawnGridLines();
        GetImage.GetImageFromUserAsync(gameObject.name, "ReceiveImage");
    }
#endif

    private void SpawnGridLines()
    {
        float whiteKeyWidth = width / 52;
        float offset = whiteKeyWidth * 2;

        for (int index = 0; index < 7; index++)
        {
            // octave line
            Instantiate(gridLine, new Vector3(-9 + offset, 1, -2), gridLine.transform.rotation);
            // second line
            Instantiate(gridLine, new Vector3(-9 + offset + whiteKeyWidth * 3, 1, -2), gridLine.transform.rotation);
            offset += whiteKeyWidth * 7; 
        }
        // one more line at the end
        Instantiate(gridLine, new Vector3(-9 + offset, 1, -2), gridLine.transform.rotation);
    }

    private IEnumerator LoadMidiFile(byte[] fileBytes)
    {
        midiFile = MidiFileLoader.Load(fileBytes);
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.back * pointsPerSecond * Time.deltaTime);
    }

    void ApplyMessages(List<IMidiEvent> messages, float deltaTime, float totalTime)
    {
        float pointsDown = pointsPerSecond * deltaTime;
        float offsetZ = pointsPerSecond * totalTime;

        foreach (GameObject noteObject in playingNotes)
        {
            if (noteObject != null)
            {
                noteObject.transform.localScale += Vector3.forward * pointsDown;
                noteObject.transform.Translate(Vector3.forward * pointsDown / 2);
            }
        }

        if (messages != null)
        {
            foreach (var message in messages)
            {
                if (message is MidiMetaEvent)
                {
                    MidiMetaEvent midiMetaEvent = (MidiMetaEvent)message;

                    // set tempo event
                    if (midiMetaEvent.type == 0x51)
                        sequencer.SetBPM(GetBPM(midiMetaEvent.bytes));
                }
                else if (message is MidiEvent)
                {
                    MidiEvent midiEvent = (MidiEvent)message;
                    if ((midiEvent.status & 0xf0) == 0x90)
                    {
                        // the range of keys visible on keyboard is: 21 - 108
                        if (midiEvent.data1 < 21 || midiEvent.data1 > 108)
                            continue;

                        if (midiEvent.data2 != 0)
                            HandleKeyDown(midiEvent.data1, offsetZ);
                        else
                            HandleKeyUp(midiEvent.data1);
                    }
                    else if ((midiEvent.status & 0xf0) == 0x80)
                    {
                        HandleKeyUp(midiEvent.data1);
                    }
                }
            }
        }        
    }

    private void HandleKeyDown(byte note, float offsetZ)
    {
        playingNotes[note] = Instantiate(
            NoteUtils.IsBlackKey(note) ? blackTile : whiteTile,
            GetNotePosition(note, offsetZ),
            transform.rotation,
            transform
        );

        // TODO: keyboard pressing when the note is received does not make sense anymore
        //keyboard.SetKeyPressed(note, true);
    }

    private void HandleKeyUp(byte note)
    {
        finishedNotes.Add(playingNotes[note]);
        playingNotes[note] = null;

        //keyboard.SetKeyPressed(note, false);
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

    private Vector3 GetNotePosition(byte note, float offsetZ)
    {
        int y = 1;
        int precedingBlackKeys = (note / 12) * 5 + octaveBlackKeys[note % 12];
        
        // 12 is the white keys count not visible on the piano on bottom
        int precedingWhiteKeys = note - precedingBlackKeys - 12;
        int totalWhiteKeys = 52;

        float whiteKeyWidth = width / totalWhiteKeys;
        float offsetX = whiteKeyWidth * precedingWhiteKeys + whiteKeyWidth / 2;

        //black keys need an offset
        if (NoteUtils.IsBlackKey(note))
        {
            // there are the types of black keys left. middle, right

            // middle between two white notes
            offsetX -= whiteKeyWidth / 2;

            // note index in octave
            int noteIndex = note % 12;

            // left black key c#, f#
            if (noteIndex == 1 || noteIndex == 6)
            {
                offsetX -= whiteKeyWidth / 10;
            }
            else if (noteIndex == 3 || noteIndex == 10)
            {
                // right black key d#, a#
                offsetX += whiteKeyWidth / 10;
            }

            y = 2;
        }

        return new Vector3(transform.position.x + offsetX, y, offsetZ);
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
