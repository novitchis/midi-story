using Assets.Scripts;
using Assets.Scripts.Styles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
    private Material material = null;
    private StyleManager styleManager = null;
    public NoteTileInfo NoteTileInfo { get; set; }

    private bool currentlyPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        if (NoteTileInfo == null)
            throw new System.Exception("NoteTileInfo property is mandatory.");

        styleManager = GameObject.Find("Sheet").GetComponent<StyleManager>();
        styleManager.StyleChanged += OnStyleChanged;

        UpdateMaterial();
    }

    private void OnStyleChanged(object sender, System.EventArgs e)
    {
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        material = styleManager.GetTileMaterial(NoteTileInfo.TrackIndex, NoteUtils.IsBlackKey(NoteTileInfo.Event.data1));
        this.GetComponent<Renderer>().material = material;
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentlyPressed && this.transform.position.y < 0) 
        {
            currentlyPressed = true;
            material.SetFloat("Vector1_8C03E553", 0.3f);
        }
    }

    private void OnDestroy()
    {
        if (styleManager != null)
            styleManager.StyleChanged -= OnStyleChanged;
    }
}
