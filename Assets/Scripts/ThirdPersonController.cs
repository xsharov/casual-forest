using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement settings")]
    public float moveSpeed = 5f;         
    public float rotationSpeed = 720f;  

    [Header("Camera controls")]
    public float cameraSensitivity = 2f;
    public Transform cameraTransform; 

    private CharacterController _characterController;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        var inputDirection = new Vector3(horizontal, 0f, vertical);
        
        var moveDirection = cameraTransform.TransformDirection(inputDirection);
        moveDirection.y = 0f;

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        _characterController.SimpleMove(moveDirection * moveSpeed);

        if (moveDirection.sqrMagnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        var mouseX = Input.GetAxis("Mouse X") * cameraSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * cameraSensitivity;

        cameraTransform.RotateAround(transform.position, Vector3.up, mouseX);
    }
}
