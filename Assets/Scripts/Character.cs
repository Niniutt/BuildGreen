using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController controller;

    public float speed = 20.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(speed * Time.deltaTime * move);
    }
}
