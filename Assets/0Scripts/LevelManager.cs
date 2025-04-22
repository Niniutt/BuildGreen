using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

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

struct Order
{
    public int ID;
    public int remainingTime;
    public DeviceType deviceType;

    public Order(int id, int r, DeviceType d)
    {
        ID = id;
        remainingTime = r;
        deviceType = d;
    }
}

public class LevelManager : MonoBehaviour
{
    private float deltaOrders = 5f; // Time between each order
    private int nbDeviceTypes = 4;
    private int timeOrder = 30; // Max time to deliver an order
    private int lastID = 0;
    private float deltaCheck = 1f;
    private List<Order> orders = new();
    private float destroyDelay = 0.5f;

    void Start()
    {
        // Find canva place and put up a first order

        InvokeRepeating("StartOrder", 0, deltaOrders);
        InvokeRepeating("CheckOrders", deltaCheck, deltaCheck);
    }

    private void StartOrder ()
    {
        int type = Random.Range(0, nbDeviceTypes - 1);
        lastID += 1;
        Order order = new Order(lastID, timeOrder, (DeviceType)type);
        orders.Add(order);
    }

    private void CheckOrders()
    {
        for (int i = 0; i < orders.Count; i++)
        {
            Order order = orders[i];
            if (order.remainingTime == 0)
            {
                // Order lost
                // Debug.Log("Lost order " + order.ID);
            }
            order.remainingTime -= 1;
            orders[i] = order;
        }
    }

    public void DeliverOrder(GameObject obj)
    {
        // deliveryCollider call
        Destroy(obj, destroyDelay);
    }
}
