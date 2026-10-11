using UnityEngine;

public class StackPartController : MonoBehaviour
{
    private Rigidbody rb;
    private MeshRenderer meshRender;
    private StackController stackController;
    private Collider col;
    void Awake()
    {
         rb = GetComponent<Rigidbody>();
        meshRender = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        stackController = transform.parent.GetComponent<StackController>();
    }

    public void Shatter()
    {
        rb.isKinematic = false;
        col.enabled = false;

        Vector3 forcePoint = transform.parent.position;
        float paretXpos = transform.parent.position.x;
        float xPos = meshRender.bounds.center.x;

        Vector3 subDir = (paretXpos - xPos < 0) ? Vector3.right : Vector3.left;
        Vector3 dir = (Vector3.up * 1.5f + subDir).normalized;

        float force = Random.Range(20, 35);
        float torque = Random.Range(110, 180);

        rb.AddForceAtPosition(dir * force, forcePoint, ForceMode.Impulse);
        rb.AddTorque(Vector3.left * torque);
        rb.velocity = Vector3.down;
    }
}
