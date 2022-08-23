using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
    [SerializeField] SpriteRenderer m_spriteRender;
    [SerializeField] TrailRenderer m_trail;

    public Vector3 m_targetPosition;
    public float m_speed;

    float m_time;
    Vector3 m_startPosition;
    bool m_moving;

    private void Start()
    {
    }

    private void Update()
    {
        if (!m_moving) return;

        m_time += Time.deltaTime * m_speed;

        transform.position = Vector3.Lerp(m_startPosition, m_targetPosition, EasingFunctions.easeInOutCubic(m_time));

        if (m_time >= 1)
        {
            transform.position = m_targetPosition;

            Destroy(gameObject); //TODO: use pool instead
        }

    }

    public void BeginMove()
    {
        m_moving = true;
        m_startPosition = transform.position;
    }

    public void UpdateRenderColor(Color color)
    {
        if (m_spriteRender)
        {
            m_spriteRender.color = color;
        }

        if (m_trail)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color / 2, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(color.a, 0.0f), new GradientAlphaKey(color.a, 1.0f) }
            );
            m_trail.colorGradient = gradient;
        }
    }
}