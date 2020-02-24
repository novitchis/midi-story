using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmfLite;
using System;

public class Sequencer : MonoBehaviour
{
    public GameObject blackTile;
    public GameObject whiteTile;

    public float width;

    public TextAsset sourceFile;
    MidiFileContainer song;
    MidiTrackSequencer sequencer;

    private int[] octaveBlackKeys = new int[] { 0, 0, 1, 1, 2, 2, 2, 3, 3, 4, 4, 5 };
    private string[] notes = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    void ResetAndPlay()
    {
        transform.position = new Vector3(-9, 2, 0);
        //TODO: get bpm from the midi file header
        sequencer = new MidiTrackSequencer(song.tracks[0], song.division, 80.0f);
        ApplyMessages(sequencer.Start());
    }

    IEnumerator Start()
    {
        song = MidiFileLoader.Load(sourceFile.bytes);
        yield return new WaitForSeconds(1.0f);
        ResetAndPlay();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * -1f);

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

                    int offset = (int)m.data1 % 12;
                    //if (m.data2 != 0)
                    //    Debug.Log((m.data2 == 0x0 ? "Note Off " : "Note On: ") + notes[offset]);// + " " + m.ToString());

                    Instantiate(
                        notes[offset].EndsWith("#") ? blackTile : whiteTile,
                        GetNotePosition(m.data1), 
                        transform.rotation, 
                        transform
                    );
                }
                else if ((m.status & 0xf0) == 0x80)
                {
                    int offset = (int)m.data1 % 12;
                    Debug.Log("Note Off: " + notes[offset]);// + " " + m.ToString());
                }
                //else
                //    Debug.Log("Unknown " + m.ToString());
            }
        }
    }

    private Vector3 GetNotePosition(byte note)
    {
        // TODO: our range is: 21 - 108 - discard notes above and under
        int precedingBlackKeys = (note / 12) * 5 + octaveBlackKeys[note % 12];
        // 12 is the white keys count not visible on the piano on bottom
        int precedingWhiteKeys = note - precedingBlackKeys - 12;
        int totalWhiteKeys = 51;

        //TODO: black keys need an offset
        return new Vector3(transform.position.x + (width / totalWhiteKeys) * precedingWhiteKeys, 1, 0);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Play/Stop"))
        {
            ResetAndPlay();
        }
    }
}
