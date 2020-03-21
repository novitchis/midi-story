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

    public void SetKeyPressed(byte note, bool pressed)
    {
        // first 21 notes are not visible on keyboard
        int childIndex = note - 20;

        this.transform.GetChild(childIndex).GetComponent<Renderer>().material = pressed ? GetPressedMaterial(note) : GetIddleMaterial(note);
    }

    private Material GetPressedMaterial(byte note)
    {
        return NoteUtils.IsBlackKey(note) ? pressedBlackKey : pressedWhiteKey;
    }

    private Material GetIddleMaterial(byte note)
    {
        return NoteUtils.IsBlackKey(note) ? blackKey : whiteKey;
    }

    public void Clear()
    {
        foreach (byte note in Enumerable.Range(21, 88))
            SetKeyPressed(note, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetKeyPressed(60, true);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            SetKeyPressed(60, false);
        }
    }
}
