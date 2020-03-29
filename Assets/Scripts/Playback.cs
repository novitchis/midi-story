using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playback : MonoBehaviour
{
    public Sequencer sequencer;
    private bool show = false;

    private void Update()
    {
        if (show && ((RectTransform)transform).anchoredPosition.y > 0)
            transform.position += Vector3.down * (200 * Time.deltaTime);
        else if (!show && ((RectTransform)transform).anchoredPosition.y < 50)
            transform.position += Vector3.up * (200 * Time.deltaTime);
    }

    public void GoToStart()
    {
        Hide();
        sequencer.GoToStart();
    }

    public void ResetPlayer()
    {
        Hide();
        sequencer.ResetPlayer();
    }

    public void Show()
    {
        show = true;
    }

    public void Hide()
    {
        show = false;
    }
}
