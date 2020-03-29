﻿using System.Collections;
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
    public Material playingWhiteTile;
    public Material playingBlackTile;

    public float width;
    public float pointsPerSecond = 4;

    public TextAsset sourceFile;

    private MidiFileContainer midiFile;
    private MidiSequencer sequencer = null;

    private GameObject[] playingNotes = new GameObject[109];
    private List<KeyValuePair<float, MidiEvent>> timeToEvent = new List<KeyValuePair<float, MidiEvent>>();
    private int timerIndex = 0;
    private float time = 0;

#if DEBUG
    private IEnumerator Start()
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

    // Update is called once per frame
    private void Update()
    {
        AdvancePlayer(Time.deltaTime);
    }

    private void AdvancePlayer(float deltaTime)
    {
        transform.Translate(Vector3.down * pointsPerSecond * deltaTime);

        // execute events on keyboard
        time += deltaTime;
        while (timeToEvent.Count > timerIndex && timeToEvent[timerIndex].Key <= time)
        {
            MidiEvent midiEvent = timeToEvent[timerIndex].Value;
            if ((midiEvent.status & 0xf0) == 0x90 && midiEvent.data2 != 0)
            {
                keyboard.SetKeyPressed(midiEvent.data1, true);
            }
            else if (((midiEvent.status & 0xf0) == 0x90 && midiEvent.data2 == 0) || (midiEvent.status & 0xf0) == 0x80)
            {
                keyboard.SetKeyPressed(midiEvent.data1, false);
            }

            timerIndex++;
        }

        // decrease the size of the tiles until it becomes 0 when it is removed
        foreach (Transform child in transform)
        {
            if (child.localPosition.y < pointsPerSecond * time)
            {
                RoundedQuadMesh quad = child.GetComponent<RoundedQuadMesh>();

                if (quad.AutoUpdate == false)
                {
                    // TODO: this can be done better
                    child.gameObject.GetComponent<Renderer>().material = child.gameObject.name.Contains(blackTile.name) ? playingBlackTile : playingWhiteTile;
                    quad.AutoUpdate = true;
                }

                quad.rect.height -= pointsPerSecond * deltaTime;
                child.transform.position += Vector3.up * (pointsPerSecond * deltaTime);

                if (quad.rect.height <= 0)
                    Destroy(child.gameObject);
            }
            else
            {
                break;
            }
        }
    }

    private IEnumerator LoadMidiFile(byte[] fileBytes)
    {
        midiFile = MidiFileLoader.Load(fileBytes);
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay();
    }

    private void ResetAndPlay()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        time = 0;
        timerIndex = 0;
        timeToEvent.Clear();
        keyboard.Clear();
        playingNotes = new GameObject[109];

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // offset the position by the screen size to start notes falling from the top
        float totalTime = 8 / pointsPerSecond;

        //TODO: this should not happen on restart
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
        float offsetY = pointsPerSecond * totalTime;

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
                            HandleKeyDown(midiEvent.data1, offsetY);
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

    private void HandleKeyDown(byte note, float offsetY)
    {
        float z = NoteUtils.IsBlackKey(note) ? 0.2f : 0.1f;
        Vector3 notePosition = new Vector3(transform.position.x + NoteUtils.GetKeyX(note, width, true), offsetY, transform.position.z - z);

        playingNotes[note] = Instantiate(
            NoteUtils.IsBlackKey(note) ? blackTile : whiteTile,
            notePosition,
            Quaternion.identity,
            transform
        );
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
