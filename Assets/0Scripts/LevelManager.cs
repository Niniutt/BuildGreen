using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;
using Unity.Netcode;
using Unity.VisualScripting;

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
    [SerializeField] private RecipesSO recipesSO;
    public GameObject UIPrefab;
    private GameObject[] UIorders = new GameObject[MAX_ORDERS];

    private readonly float deltaOrders = 15f; // Time between each order
    private int timeOrder = 60; // Max time to deliver an order
    private int lastID = 0;
    private readonly float deltaCheck = 1f;
    private Order?[] displayOrders = new Order?[MAX_ORDERS];
    private List<Order> orders = new();
    private readonly float deltaMaterial = 0.66f;
    private readonly float deltaPart = 3f;

    // private int materialIndex0 = 0;
    // private int partIndex0 = 3;
    private int deviceIndex0 = 7;
    private int lastTypeIndex = 10;

    [SerializeField] private Vector3 craftSpawnPoint = new(-2f, 0.5f, 2.5f);

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

    private List<Type> toSpawn = new();

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
        InvokeRepeating(nameof(SpawnMaterials), 0, deltaMaterial);
        InvokeRepeating(nameof(SpawnParts), 0, deltaPart);

        // Repeating functions
        InvokeRepeating(nameof(StartOrder), 0, deltaOrders);
        InvokeRepeating(nameof(CheckOrders), deltaCheck, deltaCheck);
    }

    // Repeating spawning of metal, plastic (x2), glass for just one second (scene animation)
    private void SpawnMaterials()
    {
        int type = Random.Range(0, 4); // Choosing material type
        switch(type)
        {
            case 0:
                SpawnMaterial(Type.METAL, new Vector3(-3.5f, 0.5f, -10f));
                break;
            case 1:
                SpawnMaterial(Type.PLASTIC, new Vector3(-2.5f, 0.5f, -10f));
                break;
            case 2:
                SpawnMaterial(Type.PLASTIC, new Vector3(2.5f, 0.5f, -10f));
                break;
            case 3:
                SpawnMaterial(Type.GLASS, new Vector3(3.5f, 0.5f, -10f));
                break;
        }
    }

    private void SpawnMaterial(Type type, Vector3 point)
    {
        GameObject prefab = GetPrefabFromType(type);
        GameObject go = Instantiate(prefab, point, Quaternion.identity);
        NetworkObject no = go.GetComponent<NetworkObject>();
        if (no != null) no.Spawn();
    }

    private void SpawnParts()
    {
        // Either spawn a part according to an order recipe (1), either randomly (2).
        Type type = Type.NULL;
        int index = 0;
        if (toSpawn.Count > 0)
        {
            type = toSpawn[0];
            toSpawn.RemoveAt(0);
            index = 10; // Hardcoded to 10 for now
        }
        else
        {
            index = Random.Range(0, 10); // Weights
        }
        switch (index)
        {
            case 10:
                SpawnPart(type); // From toSpawn
                break;
            case 0 or 1:
                SpawnPart(Type.BATTERY); // 2
                break;
            case 2 or 3 or 4:
                SpawnPart(Type.CHIP); // 3
                break;
            case 5 or 6:
                SpawnPart(Type.DISK); // 2
                break;
            case 7 or 8 or 9:
                SpawnPart(Type.SCREEN); // 3
                break;
        }
        Debug.Log("Spawn " + type.ToString());
        // Eventually would be nice to just count every object of every type on the scene and produce depending on that.
    }

    private void SpawnPart(Type type)
    {
        Vector3 point;
        switch (type)
        {
            case Type.BATTERY:
                point = new Vector3(-3.5f, 0.5f, -7f);
                break;
            case Type.CHIP:
                point = new Vector3(-2.5f, 0.5f, -7f);
                break;
            case Type.DISK:
                point = new Vector3(2.5f, 0.5f, -7f);
                break;
            case Type.SCREEN:
                point = new Vector3(3.5f, 0.5f, -7f);
                break;
            default:
                point = craftSpawnPoint;
                Debug.Log("Craft");
                break;
        }
        GameObject prefab = GetPrefabFromType(type);
        Vector3 position = gridManager.GetSnappedPosition(point);

        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        NetworkObject no = go.GetComponent<NetworkObject>();
        if (no != null) no.Spawn();
        Grabbable grab = go.GetComponent<Grabbable>();
        grab.type.Value = type; // This is probably set only on server side, not on client side it seems.
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
        // Create order
        int type = Random.Range(deviceIndex0, lastTypeIndex + 1); // Range's max is exclusive
        lastID += 1;
        Order order = new Order(lastID, timeOrder, (Type)type);
        orders.Add(order);
        // Recipe ingredients
        List<Type> ingredientsList = recipesSO.GetIngredientList((Type)type);
        toSpawn.AddRange(ingredientsList);
        // Update UI
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
        SpawnPart(type);
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
