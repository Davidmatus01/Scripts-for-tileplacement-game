using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private GameControls controls;
    private Vector2 moveInput;

    private void Awake()
    {
        controls = new GameControls();

        // Connects the Move action to read input values
        controls.TilePlacement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.TilePlacement.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.TilePlacement.Enable();
    }

    private void OnDisable()
    {
        controls.TilePlacement.Disable();
    }

    private void Update()
    {
        // Move the camera based on input (WASD -> Vector2)
        Vector3 move = new Vector3(-moveInput.x, 0, -moveInput.y);
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
