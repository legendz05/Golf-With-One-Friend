using UnityEngine;

public class MovingBackground : MonoBehaviour
{
    public float speed;

    private Vector2 direction;
    private Vector2 velocity;

    // Update is called once per frame
    void Update()
    {
        direction = Vector2.left.normalized;
        velocity = direction * speed * Time.deltaTime;

        transform.Translate(velocity);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Destroyer"))
        {
            Destroy(gameObject);
        }
    }
}
