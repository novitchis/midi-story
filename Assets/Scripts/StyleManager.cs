using Assets.Scripts.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StyleManager : MonoBehaviour
{
    public List<string> TrackColors = new List<string> { "#00A8E8" };

    public Material WhiteTileMaterial;

    private List<Material> TrackMaterials { get; set; }

    void Start()
    {
        TrackMaterials = new List<Material>();

        TrackColors.ForEach(colorValue =>
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(colorValue, out color);
            Material trackMaterial = new Material(WhiteTileMaterial);
            trackMaterial.SetColor("Color_613449CD", color);

            TrackMaterials.Add(trackMaterial);
        });
    }

    public Material GetTileMaterial(int trackIndex, bool isBlackKey)
    {
        Material material = new Material(trackIndex < TrackMaterials.Count ? TrackMaterials[trackIndex] : TrackMaterials.Last());
        if (isBlackKey)
            material.SetInt("Boolean_B1539AEC", 1);

        return material;
    }
}
