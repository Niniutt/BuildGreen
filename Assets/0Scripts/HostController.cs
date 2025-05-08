using UnityEngine;
using Unity.Netcode;
using System.Drawing;
using System.Collections.Generic;
using Unity.VisualScripting;

public class HostController : NetworkBehaviour
{
    private Transform cameraTransform;
    private BoxCollider boxCollider;
    private Grabbable grab;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject playerMesh;
    [SerializeField] private Grabber grabber;
    [SerializeField] private Transform grabberTransform;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private LevelManager levelManager;

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

    private Vector3 thirdPersonCameraPosition = new Vector3(0f, 6f, -1.5f);// (0f, 9f, -3f);
    private float thirdPersonRotationX = 75f;

    private float destroyDelay = 0.5f;

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
        // Debug.Log(cameraTransform);
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

        // Grab
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabber.hasGrabbed)
            {
                Ungrab();
            }
            else
            {
                Grab();
            }
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        JumpPhysics();
    }

    private void MovePlayer()
    {
        move = transform.TransformDirection(moveInput);
        // If there is input, rotate player in movement direction
        if (move != new Vector3()) { playerMesh.transform.rotation = Quaternion.LookRotation(move); };
        move = move * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
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

    private void Grab()
    {
        // Test if there is object in zone
        if (grabber.inZone && grabber.objectInZone != null)
        {
            GameObject go = grabber.objectInZone;
            // If yes, put object in child "Grabbed"
            grabber.hasGrabbed = true;
            go.transform.parent = grabberTransform;
            // Reset transform
            go.transform.localPosition = new Vector3();
            go.transform.localRotation = new Quaternion();

            grab = go.GetComponent<Grabbable>();
            if (grab == null) Debug.LogError("Not valid grabbable object (missing 'Grabbable' script)");
            grab.UpdateGrabbable();
        }
    }

    private void Ungrab()
    {
        GameObject go = grabber.objectInZone;
        Vector3 point = go.transform.parent.position;
        Vector3 snappedPosition = gridManager.GetSnappedPosition(point);

        // Can only put down if there is no object there
        if (!gridManager.CheckPosition(go, snappedPosition))
        {
            grabber.hasGrabbed = false;
            go.transform.parent = null;

            grab.UpdateGrabbable();

            // Find closest gridpoint
            go.transform.position = snappedPosition;

            // Check if ungrab is actually a delivery (two possible positions)
            if (Mathf.Abs(snappedPosition.x) == 0.5f && snappedPosition.z == 8.5f)
            {
                // Get object data
                Type type = grab.type;

                // Deliver
                gridManager.Remove(go, type);
                levelManager.DeliverOrder(type);
                Debug.Log("deliver");
                Destroy(go, destroyDelay);
                grabber.ResetGrabber();
            }
        }

        
    }
}
