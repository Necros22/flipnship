using UnityEngine;

public class IntroWall : MonoBehaviour
{
    [SerializeField] private float delay = 2f;

    private Rigidbody rb;
    private float timer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        timer = delay;

        rb.useGravity = false;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            rb.useGravity = true;
        }
    }
}
