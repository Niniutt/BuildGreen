using UnityEngine;
using Unity.Netcode;

public class HostController : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider boxCollider;

    [Space]

    [SerializeField] private float speed = 20.0f;
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField] private float jumpForce = 1.0f;

    private Vector3 moveInput;
    private Vector2 mouseInput;

    private Vector3 move = new();
    private Vector2 rotate = new();

    public float fallMultiplier = 2.5f; // Multiplies gravity when falling down
    public float ascendMultiplier = 2f; // Multiplies gravity for ascending to peak of jump
    private bool isGrounded = true;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.3f;
    private float playerHeight;
    private float raycastDistance;

    public LayerMask groundLayer;
    public bool firstPerson = false;

    private Vector3 thirdPersonCameraPosition = new Vector3(0f, 6f, -4f);
    private float thirdPersonRotationX = 60f;

    void Start()
    {
        Init();
    }

    public override void OnNetworkSpawn()
    {
        Init();
    }

    public void UpdateCameraPerson()
    {
        Debug.Log(cameraTransform);
        if (firstPerson)
        {
            // Reset camera transform
            cameraTransform.localPosition = new Vector3();
            cameraTransform.localRotation = new Quaternion();
        }
        else
        {
            cameraTransform.localPosition = thirdPersonCameraPosition;
            cameraTransform.localRotation = Quaternion.Euler(thirdPersonRotationX, 0f, 0f);
        }
    }

    private void Init()
    {
        // Get rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        boxCollider = GetComponentInChildren<BoxCollider>();

        // Get player camera
        if (!cameraTransform)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }

        UpdateCameraPerson();

        // Jump raycast init
        playerHeight = boxCollider.size.y * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;

        // Hide mouse
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        // if (!IsOwner)
    }

    void Update()
    {
        moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
        if (firstPerson)
        {
            mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            MovePlayerCamera();
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        JumpPhysics();
    }

    private void MovePlayer()
    {
        move = transform.TransformDirection(moveInput) * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // Stop sliding
    }

    private void MovePlayerCamera()
    {
        rotate.x = mouseInput.x * sensitivity;
        rotate.y -= Mathf.Clamp(mouseInput.y * sensitivity, -90f, 90f);
        transform.Rotate(0, rotate.x, 0);

        cameraTransform.localRotation = Quaternion.Euler(rotate.y, 0, 0);
    }

    private void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    private void JumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * ascendMultiplier * Time.fixedDeltaTime;
        }
    }
}
