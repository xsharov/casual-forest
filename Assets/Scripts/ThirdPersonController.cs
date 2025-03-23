using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class ThirdPersonController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraHolder;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private float animationSmoothness = 10f;

    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private float cameraDistance = 3f;
    [SerializeField] private float cameraHeightOffset = 1.5f;
    [SerializeField] private float minCameraHeight = 0.3f;

    [SerializeField] private float strafeSpeedMultiplier = 0.8f;
    [SerializeField] private float backwardSpeedMultiplier = 0.6f;

    private Rigidbody _rigidbody;

    private float _horizontalInput;
    private float _verticalInput;

    private float _currentHorizontal;
    private float _currentVertical;

    private float _yaw;
    private float _pitch;

    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        _yaw = transform.eulerAngles.y;
        _pitch = cameraHolder.localEulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        _currentHorizontal = Mathf.Lerp(_currentHorizontal, _horizontalInput, Time.deltaTime * animationSmoothness);
        _currentVertical = Mathf.Lerp(_currentVertical, _verticalInput, Time.deltaTime * animationSmoothness);

        animator.SetFloat(Horizontal, _currentHorizontal);
        animator.SetFloat(Vertical, _currentVertical);

        HandleMouseRotation();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        var direction = transform.forward * _verticalInput + transform.right * _horizontalInput;
        direction.y = 0;

        var adjustedSpeed = moveSpeed;
        if (_verticalInput < 0)
            adjustedSpeed *= backwardSpeedMultiplier;
        else if (_horizontalInput != 0 && _verticalInput == 0)
            adjustedSpeed *= strafeSpeedMultiplier;

        var velocity = direction.normalized * adjustedSpeed;
        velocity.y = _rigidbody.velocity.y;

        if (direction.magnitude == 0)
        {
            velocity.x = 0;
            velocity.z = 0;
        }

        _rigidbody.velocity = velocity;
    }

    private void HandleMouseRotation()
    {
        _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

        var cameraPosition = Quaternion.Euler(_pitch, _yaw, 0) * new Vector3(0, 0, -cameraDistance);
        cameraPosition += transform.position + Vector3.up * cameraHeightOffset;

        if (cameraPosition.y < transform.position.y + minCameraHeight)
        {
            cameraPosition.y = transform.position.y + minCameraHeight;
        }

        cameraHolder.position = cameraPosition;
        cameraHolder.LookAt(transform.position + Vector3.up * cameraHeightOffset);
    }
}