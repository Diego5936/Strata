using System;
using Unity.VisualScripting;
using UnityEngine;

public class SeekerController : MonoBehaviour
{
    //Seeker Controls
    private float moveSpeed = 0.2f;
    private float rotationSpeed = 4;

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
    private float maxRayDistance = 60f;

    void Start()
    {
        myNetwork = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        energy = maxEnergy;
        sensors = new float[inputNodes];
    }

    void FixedUpdate()
    {
        InputSensors();
        float[] outputs = myNetwork.FeedForwardNetwork(sensors);

        MoveSeeker(outputs[0], outputs[1]);

        if (energy <= 0)
            Death();
        else
            energy--;
    }

    private void InputSensors()
    {
        Vector2 position2D = (Vector2)(transform.position) + (Vector2)transform.up * 0.75f;

        RaycastHit2D forwardRay = Physics2D.Raycast(position2D, transform.up, maxRayDistance);
        sensors[0] = (forwardRay.collider != null && forwardRay.collider.CompareTag("Border")) ? forwardRay.distance / maxRayDistance: 1;
        sensors[3] = (forwardRay.collider != null && forwardRay.collider.CompareTag("Food")) ? 1 : 0;

        Vector2 rightDirection = Quaternion.Euler(0, 0, -45) * transform.up;
        RaycastHit2D rightRay = Physics2D.Raycast(position2D, rightDirection, maxRayDistance);
        sensors[1] = (rightRay.collider != null && rightRay.collider.CompareTag("Border")) ? rightRay.distance / maxRayDistance: 1;
        sensors[4] = (rightRay.collider != null && rightRay.collider.CompareTag("Food")) ? 1 : 0;

        Vector2 leftDirection = Quaternion.Euler(0, 0, 45) * transform.up;
        RaycastHit2D leftRay = Physics2D.Raycast(position2D, leftDirection, maxRayDistance);
        sensors[2] = (leftRay.collider != null && leftRay.collider.CompareTag("Border")) ? leftRay.distance / maxRayDistance: 1;
        sensors[5] = (leftRay.collider != null && leftRay.collider.CompareTag("Food")) ? 1 : 0;

        Vector3 position3D = new Vector3(position2D.x, position2D.y, transform.position.z);
        Debug.DrawLine(position3D, position3D + transform.up * maxRayDistance, sensors[3] == 1 ? Color.green : Color.white);
        Debug.DrawLine(position3D, position3D + (Vector3)(rightDirection * maxRayDistance), sensors[4] == 1 ? Color.green : Color.white);
        Debug.DrawLine(position3D, position3D + (Vector3)(leftDirection * maxRayDistance), sensors[5] == 1 ? Color.green : Color.white);
    }

    //Takes in acceleration and rotation to move
    private void MoveSeeker(float acceleration, float rotation)
    {
        transform.position += transform.up * acceleration * moveSpeed;

        transform.Rotate(Vector3.forward * -rotation * rotationSpeed);
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
