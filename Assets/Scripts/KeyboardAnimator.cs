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

    private GameObject[] notesParticles = new GameObject[109];

    private StyleManager styleManager = null;

    void Start()
    {
        styleManager = GameObject.Find("Sheet").GetComponent<StyleManager>();
    }

    public void SetKeyPressed(byte note, int trackIndex)
    {
        // first 21 notes are not visible on keyboard
        int childIndex = note - 20;

        this.transform.GetChild(childIndex).GetComponent<Renderer>().material = styleManager.GetKeyMaterial(trackIndex, NoteUtils.IsBlackKey(note));
    }

    public void SetKeyDepressed(byte note)
    {
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
