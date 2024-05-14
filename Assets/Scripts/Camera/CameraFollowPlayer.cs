using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform cameraPosition;

    // Update is called once per frame
    private void Update()
    {
        transform.position = cameraPosition.position;
    }
}
