using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmfLite;
using System;
using UnityEngine.Networking;
using System.Linq;
using Assets.Scripts;
using System.Runtime.Serialization;

public class Sequencer : MonoBehaviour
{
    public KeyboardAnimator keyboard;

    public GameObject blackTile;
    public GameObject whiteTile;

    public float width;
    public float pointsPerSecond = 4;

    public TextAsset sourceFile;

    private MidiFileContainer midiFile;
    private MidiSequencer sequencer = null;

    private GameObject[] playingNotes = new GameObject[109];
    private List<NoteTileInfo> allNotes = new List<NoteTileInfo>();
    private int activationNoteIndex = 0;
    private int timerIndex = 0;
    private float time = 0;

    private bool mouseDownExecuted = false;

    public PlayerState PlayerState { get; private set; }

#if !UNITY_WEBGL || UNITY_EDITOR
    private IEnumerator Start()
    {
        midiFile = MidiFileLoader.Load(sourceFile.bytes);
        yield return new WaitForSeconds(1.0f);
        ResetPlayer();

        PlaybackNotifier.SendFileLoaded(new FileInfo() { name = "", length = (int)allNotes.Last().Time });
    }
#endif

    // Update is called once per frame
    private void Update()
    {
        if (PlayerState == PlayerState.Playing)
            AdvancePlayer(Time.deltaTime);

        if (!mouseDownExecuted && Input.GetMouseButtonDown(0))
        {
            mouseDownExecuted = true;
        }
        else if (mouseDownExecuted && Input.GetMouseButtonUp(0))
        {
            mouseDownExecuted = false;
            if (PlayerState == PlayerState.Playing)
                SetPlayerState(PlayerState.Paused);
            else if (PlayerState != PlayerState.Finished)
                SetPlayerState(PlayerState.Playing);
        }
    }

    private void AdvancePlayer(float deltaTime)
    {
        if (allNotes.Count == 0 || time > allNotes.Last().Time)
        {
            SetPlayerState(PlayerState.Finished);
            return;
        }

        // Tiles become activated when are aproaching screen bounds to improve rendering
        while (allNotes.Count > activationNoteIndex && allNotes[activationNoteIndex].Time <= time + 3)
        {
            if (allNotes[activationNoteIndex].GameObject != null)
            {
                allNotes[activationNoteIndex].GameObject.SetActive(true);
                allNotes[activationNoteIndex].GameObject.hideFlags = HideFlags.None;
            }

            activationNoteIndex++;
        }

        transform.Translate(Vector3.down * pointsPerSecond * deltaTime);

        // execute events on keyboard
        time += deltaTime;
        while (allNotes.Count > timerIndex && allNotes[timerIndex].Time <= time)
        {
            MidiEvent midiEvent = allNotes[timerIndex].Event;
            if ((midiEvent.status & 0xf0) == 0x90 && midiEvent.data2 != 0)
            {
                keyboard.SetKeyPressed(midiEvent.data1, allNotes[timerIndex].TrackIndex);
            }
            else if (((midiEvent.status & 0xf0) == 0x90 && midiEvent.data2 == 0) || (midiEvent.status & 0xf0) == 0x80)
            {
                keyboard.SetKeyDepressed(midiEvent.data1);
            }

            timerIndex++;
        }
    }

    private IEnumerator LoadMidiFile(byte[] fileBytes)
    {
        midiFile = MidiFileLoader.Load(fileBytes);
        yield return new WaitForSeconds(1.0f);
        ResetPlayer();

        PlaybackNotifier.SendFileLoaded(new FileInfo() { name = "", length = (int)allNotes.Last().Time });
    }

    public void Play()
    {
        SetPlayerState(PlayerState.Playing);
    }

    public void Pause()
    {
        SetPlayerState(PlayerState.Paused);
    }

    private void SetPlayerState(PlayerState playerState)
    {
        PlayerState = playerState;

        if (playerState == PlayerState.Finished)
            PlaybackNotifier.SendFinished();
    }

    public void GoToStart()
    {
        ResetPlayer();
        SetPlayerState(PlayerState.Playing);
    }

    public void ResetPlayer()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        time = 0;
        timerIndex = 0;
        activationNoteIndex = 0;
        keyboard.Clear();
        playingNotes = new GameObject[109];

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        allNotes.Clear();

        // offset the position by the screen size to start notes falling from the top
        float totalTime = 8 / pointsPerSecond + 1;

        sequencer = new MidiSequencer(midiFile.tracks, midiFile.division, 120);
        ApplyMessages(sequencer.Start(), 0, totalTime);

        while (sequencer.Playing)
        {
            totalTime += 0.05f;
            ApplyMessages(sequencer.Advance(0.05f), 0.05f, totalTime);
        }

        // reset this flag to not duplciate the mouse clicks
        mouseDownExecuted = false;
    }

    private void ApplyMessages(List<List<IMidiEvent>> messageTracks, float deltaTime, float totalTime)
    {
        float pointsDown = pointsPerSecond * deltaTime;
        float offsetY = pointsPerSecond * totalTime;

        foreach (GameObject noteObject in playingNotes)
        {
            if (noteObject != null)
                noteObject.GetComponent<RoundedQuadMesh>().rect.height += pointsDown;
        }

        if (messageTracks != null)
        {
            for (int trackIndex = 0; trackIndex < messageTracks.Count; trackIndex++)
            {
                foreach (var message in messageTracks[trackIndex])
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

                            GameObject gameObject = null;
                            if (midiEvent.data2 != 0)
                                gameObject = HandleKeyDown(midiEvent.data1, offsetY);
                            else
                                HandleKeyUp(midiEvent.data1);

                            NoteTileInfo noteTileInfo = new NoteTileInfo { Time = totalTime, Event = midiEvent, GameObject = gameObject, TrackIndex = trackIndex };
                            if (gameObject)
                                gameObject.GetComponent<TileAnimation>().NoteTileInfo = noteTileInfo;

                            allNotes.Add(noteTileInfo);
                        }
                        else if ((midiEvent.status & 0xf0) == 0x80)
                        {
                            // the range of keys visible on keyboard is: 21 - 108
                            if (midiEvent.data1 < 21 || midiEvent.data1 > 108)
                                continue;

                            HandleKeyUp(midiEvent.data1);
                            allNotes.Add(new NoteTileInfo { Time = totalTime, Event = midiEvent, GameObject = null, TrackIndex = trackIndex });
                        }
                    }
                }
            }
            
        }
    }

    private GameObject HandleKeyDown(byte note, float offsetY)
    {
        float z = NoteUtils.IsBlackKey(note) ? 0.2f : 0.1f;
        Vector3 notePosition = new Vector3(transform.position.x + NoteUtils.GetKeyX(note, width, true), offsetY, transform.position.z - z);

        playingNotes[note] = Instantiate(
            NoteUtils.IsBlackKey(note) ? blackTile : whiteTile,
            notePosition,
            Quaternion.identity,
            transform
        );

        playingNotes[note].SetActive(false);
        playingNotes[note].hideFlags = HideFlags.HideInHierarchy;

        return playingNotes[note];
    }

    private void HandleKeyUp(byte note)
    {
        // after the tile is finished rendered disabled auto update to improve framerate
        if (playingNotes[note] != null)
        {
            playingNotes[note].GetComponent<RoundedQuadMesh>().AutoUpdate = false;
            playingNotes[note] = null;
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

    public void ReceiveFile(string dataUrl)
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

public enum PlayerState
{
    Stoped,
    Paused,
    Playing,
    Finished,
}

public class NoteTileInfo
{
    public float Time { get; set; }

    public MidiEvent Event { get; set; }

    public GameObject GameObject { get; set; }

    public int TrackIndex { get; set; }
}

[DataContract]
public class FileInfo
{
    [DataMember(Name = "name")]
    public string name = null;

    [DataMember(Name = "length")]
    public int length = 0;
}
