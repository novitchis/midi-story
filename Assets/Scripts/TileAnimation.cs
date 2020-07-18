using Assets.Scripts;
using Assets.Scripts.Styles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
    private Material material = null;

    public NoteTileInfo NoteTileInfo { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        if (NoteTileInfo == null)
            throw new System.Exception("NoteTileInfo property is mandatory.");

        StyleManager styleManager = GameObject.Find("Sheet").GetComponent<StyleManager>();

        material = styleManager.GetTileMaterial(NoteTileInfo.TrackIndex, NoteUtils.IsBlackKey(NoteTileInfo.Event.data1));
        this.GetComponent<Renderer>().material = material;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position.y < 0) {
            material.SetFloat("Vector1_8C03E553", 0.3f);
        }
    }
}
