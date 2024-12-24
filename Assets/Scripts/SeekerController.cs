using UnityEngine;

public class SeekerController : MonoBehaviour
{
    //Seeker Controls
    private float moveSpeed = 0.2f;
    private float rotationSpeed = 6;

    //Seeker Stats
    private float maxEnergy = 500;
    public float energy;

    //Network Variables
    [Header("Network Settings")]
    public NeatNetwork myNetwork;
    public int myBrainIdx;
    public int inputNodes = 6;
    public int outputNodes = 2;
    public int hiddenNodes = 0;

    //Raycast variables
    private float[] sensors;
    private float maxRayDistance;
    private float minDist;
    private float maxDist;

    void Start()
    {
        energy = maxEnergy;
        sensors = new float[inputNodes];
    }

    void FixedUpdate()
    {
        InputSensors();
        float[] outputs = myNetwork.FeedForwardNetwork(sensors, minDist, maxDist);

        MoveSeeker(outputs[0], outputs[1]);

        if (energy <= 0)
            Death();
        else
            energy--;
    }

    private void InputSensors()
    {

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
        }
    }
}
