using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float dragSpeed = 2f;
    public float edgeScrollSpeed = 15f;
    public float edgeSize = 10f;

    private Vector2 _lastMousePos;
    private bool _isDragging;

    void Update()
    {
        // WASD movement
        Vector3 move = Vector3.zero;
        Keyboard kb = Keyboard.current;

        if (kb.wKey.isPressed) move += transform.forward;
        if (kb.sKey.isPressed) move -= transform.forward;
        if (kb.aKey.isPressed) move -= transform.right;
        if (kb.dKey.isPressed) move += transform.right;

        // keep movement on the horizontal plane
        move.y = 0;
        transform.position += move.normalized * moveSpeed * Time.deltaTime;

        // Middle mouse drag
        Mouse mouse = Mouse.current;

        if (mouse.middleButton.wasPressedThisFrame)
        {
            _isDragging = true;
            _lastMousePos = mouse.position.ReadValue();
        }

        if (mouse.middleButton.wasReleasedThisFrame)
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector2 currentMousePos = mouse.position.ReadValue();
            Vector2 delta = currentMousePos - _lastMousePos;

            Vector3 dragMove = (-transform.right * delta.x + -transform.forward * delta.y) * dragSpeed * Time.deltaTime;
            dragMove.y = 0;
            transform.position += dragMove;

            _lastMousePos = currentMousePos;
        }

        // add this at the end of Update()
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0)
        {
            Vector3 zoom = transform.forward * scroll * 5f * Time.deltaTime;
            transform.position += zoom;
        }
    }
}