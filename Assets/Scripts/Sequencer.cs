using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequencer : MonoBehaviour
{
    public GameObject blackTile;
    public GameObject whiteTile;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(blackTile, transform.position, transform.rotation, this.transform);
        Instantiate(whiteTile, transform.position + Vector3.left, transform.rotation, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        bool spacePressed = Input.GetKey(KeyCode.Space);
        if (spacePressed)
            transform.position = new Vector3(0, 0, 0);

        transform.Translate(Vector3.forward * Time.deltaTime * -1f);
    }
}
