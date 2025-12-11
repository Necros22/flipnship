using UnityEngine;

public class RingTrigger : MonoBehaviour
{
    [Header("Puerta asignada a este anillo")]
    public GameObject targetWall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetWall != null)
                Destroy(targetWall);

            Destroy(gameObject);
        }
    }
}
