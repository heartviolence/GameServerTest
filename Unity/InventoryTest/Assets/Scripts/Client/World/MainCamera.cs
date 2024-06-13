
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [field: SerializeField]
    public Transform CameraHolder { get; set; }

    [field: SerializeField]
    public Transform Transform { get; set; }

    [field: SerializeField]
    public Camera Component { get; set; }

    Transform attachTarget;

    public void Attach(Transform transform)
    {
        attachTarget = transform;
    }

    public void Detach()
    {
        attachTarget = null;
    }

    private void Update()
    {
        if (attachTarget != null)
        {
            CameraHolder.position = attachTarget.position;
        }
    }
}