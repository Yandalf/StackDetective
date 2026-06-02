using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour
{
    public string horizontalAxisName, verticalAxisName, lastHorizontalAxisName, lastVerticalAxisName;
    public float movementSpeed = 1f;
    public Rigidbody2D rb2D;
    public Animator animator;

    private Vector2 _movementInput;


    public void OnMove(CallbackContext context) 
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        rb2D.linearVelocity = _movementInput.normalized * movementSpeed;
        animator.SetFloat(horizontalAxisName, _movementInput.x);
        animator.SetFloat(verticalAxisName, _movementInput.y);
        if(_movementInput != Vector2.zero)
        {
            animator.SetFloat(lastHorizontalAxisName, _movementInput.x);
            animator.SetFloat(lastVerticalAxisName, _movementInput.y);
        }
    }
}
