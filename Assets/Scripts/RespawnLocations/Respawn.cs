using UnityEngine;

public class Respawn : MonoBehaviour
{

    private Vector3 startPos;
    private Vector3 topRampPos;


    // Start is called before the first frame update
    void Start()
    {
        // set spawn position
        startPos = transform.position;
        topRampPos = new Vector3(33.14f, 52f, 132.11f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        // if player collides with respawn, respawn player
        if (collision.gameObject.tag == "Respawn")
        {
            transform.position = startPos;
            Debug.Log("Respawned");
        }
        // if player collides with ramp spawner, respawn at top of ramp
        else if (collision.gameObject.tag == "Finish")
        {
            transform.position = topRampPos;
            Debug.Log("Spawned at Top of Ramp");
        }


    }
}
