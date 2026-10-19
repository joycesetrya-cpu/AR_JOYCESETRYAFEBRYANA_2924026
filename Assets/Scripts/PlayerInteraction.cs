using UnityEngine;
public class PlayerInteraction : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        HandleContact(other.gameObject);
    }
    void OnCollisionEnter(Collision collision)
    {
        HandleContact(collision.gameObject);
    }
    private void HandleContact(GameObject touchedObject)
    {
        if (touchedObject.CompareTag("Target"))
        {
            Debug.Log("Target tersentuh. Praktikum berhasil.");
            touchedObject.SetActive(false);
        }
        else if (touchedObject.CompareTag("Obstacle"))
        {
            Debug.Log("Obstacle tersentuh: " + touchedObject.name);
        }
    }
}