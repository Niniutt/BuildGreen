using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting.FullSerializer;

enum DeviceType
{
    TV = 0,
    SERVER = 1,
    PHONE = 2,
    PC = 3,
}

enum MaterialType
{
    PLASTIC = 0,
    METAL = 1,
    GLASS = 2,
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
    private float deltaOrders = 15f; // Time between each order
    private int nbDeviceTypes = 4;
    private int timeOrder = 30; // Max time to deliver an order
    private int lastID = 0;
    private float deltaCheck = 1f;
    private Order?[] displayOrders = new Order?[3];
    private List<Order> orders = new();
    private int maxOrders;

    void Start()
    {
        // Find canva place and put up a first order

        InvokeRepeating("StartOrder", 0, deltaOrders);
        InvokeRepeating("CheckOrders", deltaCheck, deltaCheck);

        maxOrders = displayOrders.Length;
    }

    private void StartOrder ()
    {
        int type = Random.Range(0, nbDeviceTypes - 1);
        lastID += 1;
        Order order = new Order(lastID, timeOrder, (DeviceType)type);
        Debug.Log("Start order " + lastID + " with " + order.deviceType);
        orders.Add(order);
        UpdateDisplayOrders();
    }

    private void UpdateDisplayOrders()
    {
        int index = 0;
        foreach (Order order in orders)
        {
            if (order.status == OrderStatus.RUNNING && index < maxOrders)
            {
                displayOrders[index] = order;
                index++;
            }
            if (index == maxOrders)
            {
                break;
            }
        }
        if (index == 0) // If  no more running orders, add one
        {
            StartOrder();
        }
        if (index < maxOrders) // If list not completed
        {
            for (int i = index; i < maxOrders; i++)
            {
                displayOrders[index] = null;
                index++;
            }
        }
        DisplayOrders();
    }

    private void DisplayOrders()
    {
        foreach (Order? displayOrder in displayOrders)
        {
            if (displayOrder.HasValue)
            {
                displayOrder.Value.print();
            }
        }
    }

    private void CheckOrders()
    {
        for (int i = 0; i < orders.Count; i++)
        {
            Order order = orders[i];
            if (order.status == OrderStatus.RUNNING)
            {
                if (order.remainingTime == 0)
                {
                    // Order lost
                    Debug.Log("Lost order " + order.ID);
                    order.status = OrderStatus.FAILED;
                    UpdateDisplayOrders();
                }
                order.remainingTime -= 1;
                orders[i] = order;
            }
        }
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
            }
        }
        // Change display
        UpdateDisplayOrders();
    }
}
