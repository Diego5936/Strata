using UnityEngine;

public class SeekerController : MonoBehaviour
{
    //Utils
    GeneManager geneManager;

    //Seeker Controls
    float moveSpeed = 0.2f;
    float rotationSpeed = 4f;
    int maxEnergy = 500; 

    //Network Variables
    [Header("Network Settings")]
    public NeatNetwork myNetwork;
    public int myBrainIdx;
    public int sensorsNum;

    //Raycast variables
    float[] sensors;
    float maxRayDistance = 200f;

    [Header("Seeker STATS")]
    public float energy;
    public int foodEaten;
    public float fitness;
    private float startTime;

    void Start()
    {
        geneManager = GameObject.FindObjectOfType<GeneManager>();

        sensors = new float[sensorsNum];
    }

    void FixedUpdate()
    {
        InputSensors();
        float[] outputs = myNetwork.FeedForwardNetwork(sensors);

        MoveSeeker(outputs[0], outputs[1]);

        if (energy <= 0)
            Death();
    }

    //Resets network, sensors, energy and position
    public void ResetSeeker(int brainIdx, NeatNetwork network, int sensors, Color color)
    {
        myBrainIdx = brainIdx;
        myNetwork = network;
        sensorsNum = sensors;
        energy = maxEnergy;
        this.gameObject.GetComponent<SpriteRenderer>().color = color;

        transform.SetPositionAndRotation(Utils.RandomPosition(), Quaternion.identity);
        gameObject.SetActive(true);

        startTime = Time.time;
    }

    //Raycasts
    private void InputSensors()
    {
        Vector2 position2D = (Vector2)(transform.position) + (Vector2)transform.up * (GetComponent<SpriteRenderer>().bounds.extents.y + 0.1f);

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

        //Visual Raycasts
        // Vector3 position3D = new Vector3(position2D.x, position2D.y, transform.position.z);
        // Debug.DrawLine(position3D, position3D + transform.up * forwardRay.distance, sensors[3] == 1 ? Color.green : Color.white);
        // Debug.DrawLine(position3D, position3D + (Vector3)(rightDirection * rightRay.distance), sensors[4] == 1 ? Color.green : Color.white);
        // Debug.DrawLine(position3D, position3D + (Vector3)(leftDirection * leftRay.distance), sensors[5] == 1 ? Color.green : Color.white);
    }

    //Takes in acceleration and rotation to move
    private void MoveSeeker(float acceleration, float rotation)
    {
        transform.position += transform.up * acceleration * moveSpeed;

        transform.Rotate(Vector3.forward * -rotation * rotationSpeed);

        energy--;
    }

    //Passes seeker stats to GeneManager and disables object
    private void Death()
    {
        float timeAlive = Time.time - startTime;

        geneManager.seekerDeath(myBrainIdx, timeAlive, foodEaten);

        gameObject.SetActive(false);
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
            foodEaten++; 
        }
    }
}
