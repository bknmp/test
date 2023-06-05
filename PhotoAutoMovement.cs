using UnityEngine;

public class PhotoAutoMovement : MonoBehaviour
{
	public float m_Speed = 10f;

	public float m_RecoverTime = 1f;

	private bool m_Interrupted;

	private float m_MovingDirection;

	private float m_InterruptTime;

	private float m_LastPosY;

	private float m_SelfDeltaY;

	private RectTransform m_ParentRect;

	private RectTransform m_Rect;

	private void OnEnable()
	{
		m_ParentRect = base.transform.parent.GetComponent<RectTransform>();
		m_Rect = GetComponent<RectTransform>();
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = 0f;
		base.transform.localPosition = localPosition;
		m_LastPosY = localPosition.y;
		m_MovingDirection = 1f;
	}

	private void Update()
	{
		Vector3 localPosition = base.transform.localPosition;
		float num = localPosition.y - m_LastPosY;
		m_LastPosY = localPosition.y;
		if (Mathf.Abs(num - m_SelfDeltaY) > 0.001f)
		{
			m_InterruptTime = Time.time;
			m_Interrupted = true;
		}
		if (m_Interrupted)
		{
			if (Time.time - m_InterruptTime > m_RecoverTime)
			{
				m_Interrupted = false;
			}
			m_SelfDeltaY = 0f;
			return;
		}
		m_SelfDeltaY = m_MovingDirection * m_Speed * Time.deltaTime;
		localPosition.y += m_SelfDeltaY;
		if (localPosition.y > m_Rect.rect.height - m_ParentRect.rect.height)
		{
			m_MovingDirection = -1f;
			localPosition.y = m_Rect.rect.height - m_ParentRect.rect.height;
		}
		else if (localPosition.y < 0f)
		{
			m_MovingDirection = 1f;
			localPosition.y = 0f;
		}
		base.transform.localPosition = localPosition;
	}
}
