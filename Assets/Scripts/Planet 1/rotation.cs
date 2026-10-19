using UnityEngine;

public class rotation : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 30, 0);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
