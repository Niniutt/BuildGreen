using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class HostController : NetworkBehaviour
{
    [SerializeField] GameObject cameraPrefab;
    private BoxCollider boxCollider;
    private Grabbable grab;
    private GridManager gridManager;
    private LevelManager levelManager;
    private Rigidbody rb;
    private GameObject thirdPersonCamera;
    private Grabber grabber;
    [HideInInspector] public Vector3 grabberPosition = new();
    [HideInInspector] public Quaternion grabberRotation = new();

    [Space]

    private readonly float speed = 7.0f;
    private readonly float rotationSpeed = 0.1f;
    // private readonly float sensitivity = 1.0f;
    private readonly float jumpForce = 8.0f;

    private Vector3 moveInput;
    // private Vector2 mouseInput;
    private Vector3 move = new();
    // private Vector2 rotate = new();

    private readonly float fallMultiplier = 2.5f;
    private readonly float ascendMultiplier = 2f;
    private readonly float groundCheckDelay = 0.3f;
    private float groundCheckTimer = 0f;
    private float playerHeight;
    private float raycastDistance;
    private bool isGrounded = true;

    [HideInInspector] public LayerMask groundLayer;
    [HideInInspector] public bool firstPerson = false;

    private Vector3 thirdPersonCameraPosition = new Vector3(0f, 6f, -1.5f);// (0f, 9f, -3f);
    // private float thirdPersonRotationX = 75f;

    private float destroyDelay = 0.5f;

    private Transform DiskMark;
    private Transform ScreenMark;
    private Transform ChipMark;
    private Transform BatteryMark;

    public override void OnNetworkSpawn()
    {
        Init();
    }

    private void Init()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        boxCollider = GetComponent<BoxCollider>();
        gridManager = FindFirstObjectByType<GridManager>();
        levelManager = FindFirstObjectByType<LevelManager>();
        thirdPersonCamera = Instantiate(cameraPrefab);
        grabber = GetComponentInChildren<Grabber>();

        // Get mini-game marks that have MiniGameMark tag
        GameObject[] marks = GameObject.FindGameObjectsWithTag("MiniGameMark");
        if (marks.Length == 0 || marks.Length > 4)
        {
            Debug.LogError("Failed Init: Incorrect number of mini game marks");
            return;
        }
        foreach (GameObject mark in marks)
        {
            if (mark.name[0] == 'D') DiskMark = mark.transform;
            else if (mark.name[0] == 'S') ScreenMark = mark.transform;
            else if (mark.name[0] == 'C') ChipMark = mark.transform;
            else if (mark.name[0] == 'B') BatteryMark = mark.transform;
            mark.SetActive(false); // Hide marks initially
        }

        // Jump raycast init
        playerHeight = boxCollider.size.y * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;

        // Hide mouse
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        AudioListener cameraAL = thirdPersonCamera.GetComponent<AudioListener>();
        if (IsOwner)
        {
            thirdPersonCamera.SetActive(true);
            cameraAL.enabled = true;
        }
        else
        {
            thirdPersonCamera.SetActive(false);
            cameraAL.enabled = false;
        }

        MovePlayerCamera();
    }

    void Update()
    {
        moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

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
        if (Input.GetKeyDown(KeyCode.E) && IsOwner)
        {
            if (grabber.hasGrabbed) Ungrab();
            else Grab();
        }

        if (IsOwner && grabber.hasGrabbed)
        {
            grabberPosition = grabber.transform.position;
            grabberRotation = transform.rotation;
        }

        if (Input.GetKeyDown(KeyCode.G) && grab != null && grab.isGrabbed.Value && grab.type.Value == Type.EXTINGUISHER)
        {
            Debug.Log("G pressed with extinguisher in hand");
            TryExtinguish();
        }


    }

    private void FixedUpdate()
    {
        MovePlayer();
        JumpPhysics();

        if (grab != null && grab.isGrabbed.Value)
        {
            grab.UpdateGrabberPose(grabberPosition, grabberRotation);
        }
    }

    private void MovePlayer()
    {
        move = moveInput; // transform.TransformDirection(moveInput);
        // If there is input, rotate player in movement direction
        if (move != new Vector3() && IsOwner) 
        {
            Quaternion newRotation = Quaternion.LookRotation(move * rotationSpeed);
            transform.rotation = newRotation;

            MovePlayerCamera();
        }
        ;
        move = move * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    private void MovePlayerCamera()
    {
        thirdPersonCamera.transform.position = transform.position + thirdPersonCameraPosition;
        /*
        rotate.x = mouseInput.x * sensitivity;
        rotate.y -= Mathf.Clamp(mouseInput.y * sensitivity, -90f, 90f);
        transform.Rotate(0, rotate.x, 0);

        cameraTransform.localRotation = Quaternion.Euler(rotate.y, 0, 0);
        */
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

    // Has to be changed after https://discussions.unity.com/t/player-hierarchical-networkobjects/864173/25
    // Because now items are NetworkObjects, we have to fake the transform parenting
    private void Grab()
    {
        // Test if there is object in zone
        if (grabber.inZone && grabber.objectInZone != null)
        {
            GameObject go = grabber.objectInZone;
            grab = go.GetComponent<Grabbable>();
            grab.RequestChangeOwnershipServerRpc(OwnerClientId);
            
            grabber.hasGrabbed = true;

            var no = GetComponent<NetworkObject>();
            if (no != null && grab != null)
            {
                grab.GrabServerRpc(no.NetworkObjectId);
            }

            // Display mini-game if there is one
            if (grab.miniGameBase.Value != Type.NULL) ToggleMiniGameMark(grab.miniGameBase.Value);
        }
    }

    private void Ungrab()
    {
        GameObject go = grabber.objectInZone;
        Vector3 point = grabber.transform.position;
        Vector3 snappedPosition = gridManager.GetSnappedPosition(point);

        // Can only put down if there is no object there
        if (!gridManager.CheckPosition(go, snappedPosition) && grab)
        {
            grabber.hasGrabbed = false;

            grab.UngrabServerRpc(snappedPosition);
            grab.RequestChangeOwnershipServerRpc(NetworkManager.ServerClientId);

            // Find closest gridpoint
            go.transform.position = snappedPosition;
            grabberPosition = snappedPosition;

            // Check if ungrab is actually a delivery (two possible positions)
            Type type = grab.type.Value;
            if (Mathf.Abs(snappedPosition.x) == 0.5f && snappedPosition.z == 8.5f)
            {
                // Deliver
                gridManager.Remove(go, type);
                levelManager.DeliverOrderServerRpc(type);
                NetworkObject no = go.GetComponent<NetworkObject>();
                DestroyServerRpc(go.GetComponent<NetworkObject>().NetworkObjectId);
                grabber.ResetGrabber();
            }

            // Stop displaying mini-game
            if (grab.miniGameBase.Value != Type.NULL) ToggleMiniGameMark(grab.miniGameBase.Value);
        }
    }

    public void ToggleMiniGameMark(Type baseType)
    {
        DiskMark.gameObject.SetActive(!DiskMark.gameObject.activeSelf);
        return;
        /* // Temporary
        switch (baseType)
        {
            case Type.DISK:
                DiskMark.gameObject.SetActive(!DiskMark.gameObject.activeSelf);
                break;
            case Type.SCREEN:
                ScreenMark.gameObject.SetActive(!ScreenMark.gameObject.activeSelf);
                break;
            case Type.CHIP:
                ChipMark.gameObject.SetActive(!ChipMark.gameObject.activeSelf);
                break;
            case Type.BATTERY:
                BatteryMark.gameObject.SetActive(!BatteryMark.gameObject.activeSelf);
                break;
            default:
                Debug.LogWarning("Unknown type for mini-game mark: " + baseType);
                break;
        }*/
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc(ulong networkObjectId)
    {
        StartCoroutine(DespawnAndDestroyWithDelay(networkObjectId, destroyDelay));
    }

    private System.Collections.IEnumerator DespawnAndDestroyWithDelay(ulong networkObjectId, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var no) && no != null)
        {
            no.Despawn();
            Destroy(no.gameObject);
        }
    }

    public void UpdateGrabbable()
    {
        grab.UpdateCheckClientRpc(grab.GetComponent<NetworkObject>().NetworkObjectId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExtinguishFireServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var no))
        {
            no.Despawn();
            Destroy(no.gameObject);
        }
    }


    private List<Vector3> GetForwardGridPositions()
    {
        List<Vector3> positions = new();
        Vector3 forward = transform.forward.normalized;
        Vector3 origin = transform.position;

        for (int i = 1; i <= 3; i++)
        {
            Vector3 checkPos = origin + forward * i;
            Vector3 snapped = gridManager.GetSnappedPosition(checkPos);
            positions.Add(snapped);
        }

        return positions;
    }

    private void TryExtinguish()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float range = 1.5f;

        for (float i = 0.5f; i <= range; i += 0.5f)
        {
            Vector3 checkPos = gridManager.GetSnappedPosition(origin + direction * i);
            Debug.Log("Checking position: " + checkPos);

            Collider[] colliders = Physics.OverlapSphere(checkPos, 0.4f);
            foreach (var col in colliders)
            {
                Debug.Log("Found collider: " + col.name);
                if (col.CompareTag("Fire"))
                {
                    Debug.Log("Fire found! Requesting server to extinguish");
                    var netObj = col.GetComponent<NetworkObject>();
                    if (netObj != null)
                    {
                        ExtinguishFireServerRpc(netObj.NetworkObjectId);
                    }
                }
            }
        }
    }

}
