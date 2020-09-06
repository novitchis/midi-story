using Assets.Scripts.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StyleManager : MonoBehaviour
{
    public List<TrackStyle> TracksStyles = new List<TrackStyle> {new TrackStyle() };

    public Material WhiteTileMaterial;
    public Material WhiteKeyMaterial;

    private List<Material> trackTileMaterials { get; set; }

    private List<Material> trackKeyMaterials { get; set; }

    public event EventHandler StyleChanged;

    void Start()
    {
        UpdateStyle();
    }

    private void UpdateStyle()
    {
        trackTileMaterials = new List<Material>();
        trackKeyMaterials = new List<Material>();

        TracksStyles.ForEach(trackStyle =>
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(trackStyle.color, out color);

            Material trackTileMaterial = new Material(WhiteTileMaterial);
            trackTileMaterial.SetColor("Color_613449CD", color);
            trackTileMaterials.Add(trackTileMaterial);

            Material trackKeyMaterial = new Material(WhiteKeyMaterial);
            trackKeyMaterial.SetColor("Color_613449CD", color);
            trackKeyMaterials.Add(trackKeyMaterial);
        });
    }


    public Material GetTileMaterial(int trackIndex, bool isBlackKey)
    {
        Material material = new Material(trackIndex < trackTileMaterials.Count ? trackTileMaterials[trackIndex] : trackTileMaterials.Last());
        if (isBlackKey)
            material.SetInt("Boolean_B1539AEC", 1);

        return material;
    }

    public Material GetKeyMaterial(int trackIndex, bool isBlackKey)
    {
        Material material = new Material(trackIndex < trackKeyMaterials.Count ? trackKeyMaterials[trackIndex] : trackKeyMaterials.Last());
        if (isBlackKey)
            material.SetInt("Boolean_B1539AEC", 1);

        return material;
    }

    public void LoadStyle(string styleJson)
    {
        StyleSettings style = JsonUtility.FromJson<StyleSettings>(styleJson);
        TracksStyles = style.tracks;
        UpdateStyle();
        RaiseStyleChanged();
    }

    private void RaiseStyleChanged()
    {
        if (StyleChanged != null)
            StyleChanged(this, EventArgs.Empty);
    }
}
