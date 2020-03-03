using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardAnimator : MonoBehaviour
{
    private Animator animator;
    private bool isDown = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("isDown", true);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetBool("isDown", false);
        }
    }
}
