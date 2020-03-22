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
    private List<KeyValuePair<float, MidiEvent>> timeToEvent = new List<KeyValuePair<float, MidiEvent>>();
    private int timerIndex = 0;
    private float time = 0;

//#if DEBUG
    private IEnumerator Start()
    {
        SpawnGridLines();

        midiFile = MidiFileLoader.Load(sourceFile.bytes);
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay();
    }
//#else
//    void Start()
//    {
//        SpawnGridLines();
//        GetImage.GetImageFromUserAsync(gameObject.name, "ReceiveImage");
//    }
//#endif

    // Update is called once per frame
    private void Update()
    {
        AdvancePlayer(Time.deltaTime);
    }

    private void AdvancePlayer(float deltaTime)
    {
        transform.Translate(Vector3.back * pointsPerSecond * deltaTime);

        // execute events on keyboard
        time += deltaTime;
        while (timeToEvent.Count > timerIndex && timeToEvent[timerIndex].Key <= time)
        {
            MidiEvent midiEvent = timeToEvent[timerIndex].Value;
            if ((midiEvent.status & 0xf0) == 0x90 && midiEvent.data2 != 0)
                keyboard.SetKeyPressed(midiEvent.data1, true);
            else if (((midiEvent.status & 0xf0) == 0x90 && midiEvent.data2 == 0) || (midiEvent.status & 0xf0) == 0x80)
                keyboard.SetKeyPressed(midiEvent.data1, false);

            timerIndex++;
        }
    }

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

    private void ResetAndPlay()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        time = 0;
        timerIndex = 0;
        timeToEvent.Clear();
        keyboard.Clear();

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

        while (sequencer.Playing)
        {
            totalTime += 0.05f;
            ApplyMessages(sequencer.Advance(0.05f), 0.05f, totalTime);
        }
    }

    private void ApplyMessages(List<IMidiEvent> messages, float deltaTime, float totalTime)
    {
        float pointsDown = pointsPerSecond * deltaTime;
        float offsetZ = pointsPerSecond * totalTime;

        foreach (GameObject noteObject in playingNotes)
        {
            if (noteObject != null)
                noteObject.GetComponent<RoundedQuadMesh>().rect.height += pointsDown;
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

                        timeToEvent.Add(new KeyValuePair<float, MidiEvent>(totalTime, midiEvent));
                    }
                    else if ((midiEvent.status & 0xf0) == 0x80)
                    {
                        // the range of keys visible on keyboard is: 21 - 108
                        if (midiEvent.data1 < 21 || midiEvent.data1 > 108)
                            continue;

                        HandleKeyUp(midiEvent.data1);
                        timeToEvent.Add(new KeyValuePair<float, MidiEvent>(totalTime, midiEvent));
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
            whiteTile.transform.rotation,
            transform
        );
    }

    private void HandleKeyUp(byte note)
    {
        // after the tile is finished rendered disabled auto update to improve framerate
        playingNotes[note].GetComponent<RoundedQuadMesh>().AutoUpdate = false;

        finishedNotes.Add(playingNotes[note]);
        playingNotes[note] = null;
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
        float offsetX = whiteKeyWidth * precedingWhiteKeys;

        //black keys need an offset
        if (NoteUtils.IsBlackKey(note))
        {
            // there are the types of black keys left. middle, right
            // note index in octave
            int noteIndex = note % 12;

            // left black key c#, f#
            if (noteIndex == 1 || noteIndex == 6)
            {
                offsetX -= whiteKeyWidth / 10 * 3.5f;
            }
            else if (noteIndex == 3 || noteIndex == 10)
            {
                // right black key d#, a#
                offsetX -= whiteKeyWidth / 10;
            }
            else
            {
                // middle between two white notes
                offsetX -= whiteKeyWidth / 10 * 2f;
            }

            y = 2;
        }

        return new Vector3(transform.position.x + offsetX, y, offsetZ);
    }

    private void OnGUI()
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

    private IEnumerator LoadBlob(string url)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (!webRequest.isNetworkError && !webRequest.isHttpError)
        {
            yield return LoadMidiFile(webRequest.downloadHandler.data);
        }
    }
}
