using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Rigidbody rb;
    private float horizontal;
    private float vertical;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }
    void FixedUpdate()
    {
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }
}