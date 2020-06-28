using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
    private Material material = null;
    private float distance = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        material = this.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position.y < 0) {
            distance = Mathf.Min(distance + Time.deltaTime * 2, 0.2f);

            material.SetFloat("Vector1_8C03E553", distance);
        }
    }
}
