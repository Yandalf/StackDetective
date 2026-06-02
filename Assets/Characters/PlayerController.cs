using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string horizontalAxisName, verticalAxisName, lastHorizontalAxisName, lastVerticalAxisName;
    public float movementSpeed = 1f;
    public Rigidbody2D rb2D;
    public Animator animator;


    void Update()
    {
        var input = new Vector3(Input.GetAxisRaw(horizontalAxisName), Input.GetAxisRaw(verticalAxisName));
        var movement = input.normalized * movementSpeed;
        rb2D.linearVelocity = movement;
        animator.SetFloat(horizontalAxisName, input.x);
        animator.SetFloat(verticalAxisName, input.y);
        if(input != Vector3.zero)
        {
            animator.SetFloat(lastHorizontalAxisName, input.x);
            animator.SetFloat(lastVerticalAxisName, input.y);
        }
    }
}
