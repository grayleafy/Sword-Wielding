using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] public Vector2 mousePos = Vector2.zero;
    [SerializeField] public Vector2 lastMousePos = Vector2.zero;
    [SerializeField] public bool lockMouse = true;
    [SerializeField] public Vector2 mouseDelta = Vector2.zero;
    public void OnMousePos(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            mousePos = callback.ReadValue<Vector2>();
        }
    }

    public void OnLockMouse(InputAction.CallbackContext callback)
    {
        if (callback.performed)
        {
            lockMouse = !lockMouse;
        }
    }

    private void Update()
    {
        mouseDelta = mousePos - lastMousePos;
        lastMousePos = mousePos;
    }
}
