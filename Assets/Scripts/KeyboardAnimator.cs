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

    public void SetKeyPressed(byte note, bool pressed)
    {
        // first 21 notes are not visible on keyboard
        int childIndex = note - 20;

        this.transform.GetChild(childIndex).GetComponent<Renderer>().material = pressed ? GetPressedMaterial(note) : GetIddleMaterial(note);
        SetHasParticles(note, pressed);
    }

    private Material GetPressedMaterial(byte note)
    {
        return NoteUtils.IsBlackKey(note) ? pressedBlackKey : pressedWhiteKey;
    }

    private Material GetIddleMaterial(byte note)
    {
        return NoteUtils.IsBlackKey(note) ? blackKey : whiteKey;
    }

    private void SetHasParticles(byte note, bool hasParticles)
    {
        if (hasParticles)
        {
            if (notesParticles[note] == null)
            {
                float x = particlesSheet.transform.position.x + NoteUtils.GetKeyX(note, keyboardWidth);
                float y = particlesSheet.transform.position.y;
                float z = particlesSheet.transform.position.z;
                notesParticles[note] = Instantiate(particles, new Vector3(x,y,z), particlesSheet.transform.rotation, particlesSheet.transform);
            }
        }
        else
        {
            Destroy(notesParticles[note]);
            notesParticles[note] = null;
        }
    }


    public void Clear()
    {
        foreach (byte note in Enumerable.Range(21, 88))
            SetKeyPressed(note, false);
    }
}
