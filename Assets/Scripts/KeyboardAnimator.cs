using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyboardAnimator : MonoBehaviour
{
    public Material pressedBlackKey;
    public Material pressedWhiteKey;

    public Material blackKey;
    public Material whiteKey;

    public float keyboardWidth;

    public GameObject particlesSheet;
    public GameObject particles;

    private StyleManager styleManager = null;

    private Dictionary<byte, int> pressedKeysTracks = new Dictionary<byte, int>(160);

    void Start()
    {
        styleManager = GameObject.Find("Sheet").GetComponent<StyleManager>();
        styleManager.StyleChanged += OnStyleChanged;
    }

    private void OnStyleChanged(object sender, System.EventArgs e)
    {
        foreach (var pressedKeyInfo in pressedKeysTracks)
            SetKeyPressed(pressedKeyInfo.Key, pressedKeyInfo.Value);
    }

    public void SetKeyPressed(byte note, int trackIndex)
    {
        pressedKeysTracks[note] = trackIndex;

        // first 21 notes are not visible on keyboard
        int childIndex = note - 20;
        this.transform.GetChild(childIndex).GetComponent<Renderer>().material = styleManager.GetKeyMaterial(trackIndex, NoteUtils.IsBlackKey(note));
    }

    public void SetKeyDepressed(byte note)
    {
        pressedKeysTracks.Remove(note);

        // first 21 notes are not visible on keyboard
        int childIndex = note - 20;

        this.transform.GetChild(childIndex).GetComponent<Renderer>().material = GetIddleMaterial(note);
    }

    private Material GetIddleMaterial(byte note)
    {
        return NoteUtils.IsBlackKey(note) ? blackKey : whiteKey;
    }

    public void Clear()
    {
        foreach (byte note in Enumerable.Range(21, 88))
            SetKeyDepressed(note);
    }
}
