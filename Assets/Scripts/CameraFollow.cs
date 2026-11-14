using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform target;

    // LateUpdate is called after all Update functions have been called. Put camera code here.
    private void LateUpdate()
    {
        if(target != null)
        {
            Vector3 vector3 = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = vector3;
        }
    }
}
