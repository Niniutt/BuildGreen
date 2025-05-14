using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;
using Unity.Netcode;


struct Order
{
    public int ID;
    public int remainingTime;
    public Type type;
    public OrderStatus status;

    public Order(int id, int r, Type t)
    {
        ID = id;
        remainingTime = r;
        type = t;
        status = OrderStatus.RUNNING;
    }

    public void print()
    {
        Debug.Log("Order " + ID + " with remaining time " + remainingTime + " of device type " + type + " is " + status);
    }
}

public class LevelManager : NetworkBehaviour
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

    [SerializeField] private Vector3 craftSpawnPoint = new(0f, 0.5f, 2.5f);

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

    #region PRIVATE METHODS

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartLevel();
        }
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

    public void StartLevel()
    {
        // Find canva place and put up a first order
        InitUI();

        // Temporary: Spawn first items
        SpawnItems();

        // Repeating functions
        InvokeRepeating(nameof(StartOrder), 0, deltaOrders);
        InvokeRepeating(nameof(CheckOrders), deltaCheck, deltaCheck);
    }

    private void SpawnItems()
    {
        float z = -5.5f;
        float x = lastTypeIndex / 2; // I want int anyway
        for (int i = 0; i <= lastTypeIndex; i++)
        {
            SpawnItem((Type)i, new Vector3((float)(-x + i), 0f, z));
        }
    }

    private void SpawnItem(Type type, Vector3 point)
    {
        GameObject prefab = GetPrefabFromType(type);
        Vector3 position = gridManager.GetSnappedPosition(point);

        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        NetworkObject no = go.GetComponent<NetworkObject>();

        if (no != null)
        {
            no.Spawn();
        }
        else
        {
            Debug.LogError($"Spawned prefab is missing NetworkObject: {prefab.name}");
            return;
        }
        Grabbable grab = go.GetComponent<Grabbable>();
        grab.type = type;
        gridManager.Add(go, type, position);
    }

    private GameObject GetPrefabFromType(Type type)
    {
        // Formatting from ChatGPT is better <3
        return type switch
        {
            Type.METAL => metalPrefab,
            Type.PLASTIC => plasticPrefab,
            Type.GLASS => glassPrefab,
            Type.DISK => diskPrefab,
            Type.BATTERY => batteryPrefab,
            Type.SCREEN => screenPrefab,
            Type.CHIP => chipPrefab,
            Type.TV => tvPrefab,
            Type.SERVER => serverPrefab,
            Type.PHONE => phonePrefab,
            Type.PC => pcPrefab,
            _ => null,
        };
    }

    private void StartOrder ()
    {
        int type = Random.Range(deviceIndex0, lastTypeIndex + 1); // Range's max is exclusive
        lastID += 1;
        Order order = new Order(lastID, timeOrder, (Type)type);
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

                text.text = "Order " + displayOrders[i].Value.ID + " \n Goal: " + displayOrders[i].Value.type + " \n Time: " + displayOrders[i].Value.remainingTime + " \n Ingredients: ";
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

    #endregion
    #region PUBLIC METHODS

    public void SpawnCraftedItem(Type type)
    {        
        SpawnItem(type, craftSpawnPoint);
    }

    public void DeliverOrder(Type type)
    {
        // Correspond type with Type
        // Check if there is an order for this device
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].type == type && orders[i].status == OrderStatus.RUNNING)
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

    #endregion
}
