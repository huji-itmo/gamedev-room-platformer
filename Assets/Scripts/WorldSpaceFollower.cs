using UnityEngine;

public class WorldSpaceFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationOffset;

    [Header("Face Mode")]
    [SerializeField] private FaceMode faceMode = FaceMode.MatchRotation;
    public enum FaceMode { MatchRotation, FaceTarget, None }

    [Header("Auto Scale")]
    [SerializeField] private bool autoScale = false;
    [SerializeField] private float desiredWorldWidth = 0.5f;

    private Canvas _canvas;

    private void Awake()
    {
        if (target == null)
        {
            Camera cam = Camera.main;
            if (cam != null) target = cam.transform;
        }
        _canvas = GetComponent<Canvas>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = target.position + target.forward * positionOffset.z
                      + target.up * positionOffset.y
                      + target.right * positionOffset.x;
        transform.position = pos;

        switch (faceMode)
        {
            case FaceMode.MatchRotation:
                transform.rotation = target.rotation * Quaternion.Euler(rotationOffset);
                break;
            case FaceMode.FaceTarget:
                transform.LookAt(2 * transform.position - target.position);
                transform.rotation *= Quaternion.Euler(rotationOffset);
                break;
        }

        if (autoScale && _canvas != null)
        {
            RectTransform rt = _canvas.GetComponent<RectTransform>();
            if (rt != null && rt.rect.width > 0)
            {
                float scale = desiredWorldWidth / rt.rect.width;
                transform.localScale = Vector3.one * scale;
            }
        }
    }
}
