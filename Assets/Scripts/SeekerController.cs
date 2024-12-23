using UnityEngine;

public class SeekerController : MonoBehaviour
{
    //Seeker Controls
    private float moveSpeed = 0.2f;
    private float rotationSpeed = 6;

    //Seeker Stats
    private float maxEnergy = 500;
    public float energy;

    void Start()
    {
        energy = maxEnergy;
    }

    void FixedUpdate()
    {
        float acceleration = 0f;
        float rotation = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            acceleration = 1f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            acceleration = -1f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotation = -1f;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotation = 1f;
        }

        MoveSeeker(acceleration, rotation);

        if (energy <= 0)
            Death();
        else
            energy--;
    }

    //Takes in acceleration and rotation to move
    private void MoveSeeker(float acceleration, float rotation)
    {
        transform.position += transform.up * acceleration * moveSpeed;

        transform.Rotate(Vector3.forward * rotation * rotationSpeed);
    }

    //Resets seeker position
    private void Death()
    {
        transform.position = Vector3.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Border")
        {
            Death();
        }
        if (other.tag == "Food")
        {
            energy += 250;
            Debug.Log("Ate food");
        }
    }
}
