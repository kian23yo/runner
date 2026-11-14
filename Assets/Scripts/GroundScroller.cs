using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [Header("滚动")]
    [SerializeField] private float speed = 5f;

    [Tooltip("相邻两块故意重叠多少（世界单位）。0.05 约等于 5 像素，用来盖住亚像素接缝")]
    [SerializeField] private float overlap = 0.05f;

    [Header("地面段（留空会自动收集子物体）")]
    [SerializeField] private Transform[] segments;

    private Camera cam;
    private float segmentWidth;

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("GroundScroller: 找不到带 MainCamera 标签的相机", this);
            enabled = false;
            return;
        }

        if (segments == null || segments.Length == 0)
        {
            segments = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                segments[i] = transform.GetChild(i);
        }

        if (segments.Length == 0)
        {
            Debug.LogError("GroundScroller: Ground 下面没有子物体", this);
            enabled = false;
            return;
        }

        segmentWidth = MeasureWidth(segments[0]);
        Layout();
    }

    private float MeasureWidth(Transform t)
    {
        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds.size.x;
        return t.lossyScale.x;
    }

    private float CameraHalfWidth => cam.orthographicSize * cam.aspect;
    private float CameraLeft => cam.transform.position.x - CameraHalfWidth;

    private void Layout()
    {
        float x = CameraLeft - segmentWidth;
        for (int i = 0; i < segments.Length; i++)
        {
            Vector3 p = segments[i].position;
            p.x = x + i * (segmentWidth - overlap);
            segments[i].position = p;
        }
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;

        for (int i = 0; i < segments.Length; i++)
        {
            Transform seg = segments[i];
            seg.position += Vector3.left * step;

            if (seg.position.x + segmentWidth * 0.5f < CameraLeft)
            {
                float maxX = float.MinValue;
                for (int j = 0; j < segments.Length; j++)
                    if (segments[j].position.x > maxX) maxX = segments[j].position.x;

                seg.position = new Vector3(maxX + segmentWidth - overlap, seg.position.y, seg.position.z);
            }
        }
    }
}
