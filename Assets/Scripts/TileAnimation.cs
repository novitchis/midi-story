using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
    private Material material = null;

    // Start is called before the first frame update
    void Start()
    {
        material = this.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position.y < 0) {
            material.SetFloat("Vector1_8C03E553", 0.3f);
        }
    }
}
