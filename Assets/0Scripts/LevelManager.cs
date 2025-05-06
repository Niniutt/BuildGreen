using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

enum Type
{
    PLASTIC = 0,
    METAL = 1,
    GLASS = 2,
    DISK = 3,
    BATTERY = 4,
    SCREEN = 5,
    CHIP = 6,
    TV = 7,
    SERVER = 8,
    PHONE = 9,
    PC = 10,
}

enum OrderStatus
{
    RUNNING = 0,
    FINISHED = 1,
    FAILED = 2,
}

struct Order
{
    public int ID;
    public int remainingTime;
    public DeviceType deviceType;
    public OrderStatus status;

    public Order(int id, int r, DeviceType d)
    {
        ID = id;
        remainingTime = r;
        deviceType = d;
        status = OrderStatus.RUNNING;
    }

    public void print()
    {
        Debug.Log("Order " + ID + " with remaining time " + remainingTime + " of device type " + deviceType + " is " + status);
    }
}

public class LevelManager : MonoBehaviour
{
    private const int MAX_ORDERS = 3;

    [SerializeField] private GridManager gridManager;
    [SerializeField] private Canvas canvas;
    public GameObject UIPrefab;
    private GameObject[] UIorders = new GameObject[MAX_ORDERS];

    private float deltaOrders = 15f; // Time between each order
    private int nbDeviceTypes = 4;
    private int timeOrder = 30; // Max time to deliver an order
    private int lastID = 0;
    private float deltaCheck = 1f;
    private Order?[] displayOrders = new Order?[MAX_ORDERS];
    private List<Order> orders = new();

    private int materialIndex0 = 0;
    private int partIndex0 = 3;
    private int deviceIndex0 = 7;
    private int lastTypeIndex = 10;

    [Space]

    // IT products
    [SerializeField] private GameObject tvPrefab;
    [SerializeField] private GameObject serverPrefab;
    [SerializeField] private GameObject phonePrefab;
    [SerializeField] private GameObject pcPrefab;

    [Space]

    // IT parts
    [SerializeField] private GameObject diskPrefab;
    [SerializeField] private GameObject batteryPrefab;
    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private GameObject chipPrefab;

    [Space]

    // Materials
    [SerializeField] private GameObject metalPrefab;
    [SerializeField] private GameObject plasticPrefab;
    [SerializeField] private GameObject glassPrefab;

    void Start()
    {
        // Find canva place and put up a first order
        InitUI();

        StartLevel();
    }

    private void InitUI()
    {
        for (int i = 0; i < MAX_ORDERS; i++)
        {
            GameObject UIorder = Instantiate(UIPrefab, canvas.transform);
            UIorder.transform.position += new Vector3(i * 120, 0, 0);
            UIorder.SetActive(false); // Deactivate all before they pop again
            UIorders[i] = UIorder;
        }
    }

    private void StartLevel()
    {
        // Temporary: Spawn first items
        SpawnItems();

        // Repeating functions
        InvokeRepeating(nameof(StartOrder), 0, deltaOrders);
        InvokeRepeating(nameof(CheckOrders), deltaCheck, deltaCheck);
    }

    private void SpawnItems()
    {
        float z = -5.5f;
        float x = (int)lastTypeIndex / 2; // I want int anyway
        for (int i = 0; i <= lastTypeIndex; i++)
        {
            SpawnItem((Type)i, new Vector3((float)(-x + i), 0f, z));
        }
    }

    private void SpawnItem(Type type, Vector3 point)
    {
        GameObject prefab;
        switch (type)
        {
            case Type.METAL: prefab = metalPrefab; break;
            case Type.PLASTIC: prefab = plasticPrefab; break;
            case Type.GLASS: prefab = glassPrefab; break;
            case Type.DISK: prefab = diskPrefab; break;
            case Type.BATTERY: prefab = batteryPrefab; break;
            case Type.SCREEN: prefab = screenPrefab; break;
            case Type.CHIP: prefab = chipPrefab; break;
            case Type.TV: prefab = tvPrefab; break;
            case Type.SERVER: prefab = serverPrefab; break;
            case Type.PHONE: prefab = phonePrefab; break;
            case Type.PC: prefab = pcPrefab; break;
            default:
                prefab = null;
                Debug.LogError("LevelManager: Type not found");
                return;
        }

        Vector3 position = gridManager.GetSnappedPosition(point);
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        gridManager.Add(go, type, position);
    }

    private void StartOrder ()
    {
        int type = Random.Range(0, nbDeviceTypes - 1);
        lastID += 1;
        Order order = new Order(lastID, timeOrder, (DeviceType)type);
        // Debug.Log("Start order " + lastID + " with " + order.deviceType);
        orders.Add(order);
        UpdateDisplayOrders();
    }

    private void UpdateDisplayOrders()
    {
        int index = 0;
        foreach (Order order in orders)
        {
            if (order.status == OrderStatus.RUNNING && index < MAX_ORDERS)
            {
                displayOrders[index] = order;
                index++;
            }
            if (index == MAX_ORDERS)
            {
                break;
            }
        }
        if (index == 0) // If  no more running orders, add one
        {
            StartOrder();
        }
        if (index < MAX_ORDERS) // If list not completed
        {
            for (int i = index; i < MAX_ORDERS; i++)
            {
                displayOrders[index] = null;
                index++;
            }
        }
    }

    private void DisplayOrders()
    {
        for (int i = 0; i < MAX_ORDERS; i++)
        {
            if (displayOrders[i].HasValue)
            {
                UIorders[i].SetActive(true);
                TMP_Text text = UIorders[i].GetComponentInChildren<TMP_Text>();

                text.text = "Order " + displayOrders[i].Value.ID + " \n Goal: " + displayOrders[i].Value.deviceType + " \n Time: " + displayOrders[i].Value.remainingTime + " \n Ingredients: ";
            }
            else
            {
                UIorders[i].SetActive(false);
            }
        }
    }

    private void CheckOrders()
    {
        // This is ran every second
        for (int i = 0; i < orders.Count; i++)
        {
            Order order = orders[i];
            if (order.status == OrderStatus.RUNNING)
            {
                if (order.remainingTime == 0)
                {
                    order.status = OrderStatus.FAILED;
                }
                else
                {
                    order.remainingTime -= 1;
                }
            }
            orders[i] = order;
            UpdateDisplayOrders();
        }
        DisplayOrders();
    }

    public void DeliverOrder(int type)
    {
        // Correspond type with DeviceType
        DeviceType deviceType = (DeviceType)type;
        // Check if there is an order for this device
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].status == OrderStatus.RUNNING) // orders[i].deviceType == deviceType && 
            {
                // Change status of order
                Order currentOrder = orders[i];
                currentOrder.status = OrderStatus.FINISHED;
                orders[i] = currentOrder;
                break;
            }
        }
        // Change display
        UpdateDisplayOrders();
    }
}
