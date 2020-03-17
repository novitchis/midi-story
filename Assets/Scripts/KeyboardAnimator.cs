using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardAnimator : MonoBehaviour
{
    public Material blackKeyMaterial;
    public Material whiteKeyMaterial;
    private Material originalMaterial = null;

    // Start is called before the first frame update
    void Start()
    {
        originalMaterial = GameObject.Find("Keys").GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        GameObject.Find("Keys").GetComponent<Renderer>().material = whiteKeyMaterial;

    //        //animator.SetBool("isDown", true);
    //    }
    //    else if (Input.GetKeyUp(KeyCode.Space))
    //    {
    //        GameObject.Find("Keys").GetComponent<Renderer>().material = originalMaterial;
    //        //animator.SetBool("isDown", false);
    //    }
    //}
}
