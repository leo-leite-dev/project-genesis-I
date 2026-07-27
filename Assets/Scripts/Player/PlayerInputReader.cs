using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool DashPressed { get; private set; }
    public bool InteractPressed { get; private set; }

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            JumpPressed = true;
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
            DashPressed = true;
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
            InteractPressed = true;
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
    }

    public void ConsumeDash()
    {
        DashPressed = false;
    }

    public void ConsumeInteract()
    {
        InteractPressed = false;
    }
}
