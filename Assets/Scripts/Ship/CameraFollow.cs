using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;

    private void Update()
    {
        MoveForward();
    }

    private void MoveForward()
    {
        transform.position += new Vector3(0, 0, speed * Time.deltaTime);
    }
}
