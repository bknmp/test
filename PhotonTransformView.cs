using LightUtility;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[AddComponentMenu("Photon Networking/Photon Transform View")]
public class PhotonTransformView : BatchUpdateBehaviour, IPunObservable
{
	[SerializeField]
	private PhotonTransformViewPositionModel m_PositionModel = new PhotonTransformViewPositionModel();

	[SerializeField]
	private PhotonTransformViewRotationModel m_RotationModel = new PhotonTransformViewRotationModel();

	[SerializeField]
	private PhotonTransformViewScaleModel m_ScaleModel = new PhotonTransformViewScaleModel();

	private PhotonTransformViewPositionControl m_PositionControl;

	private PhotonTransformViewRotationControl m_RotationControl;

	private PhotonTransformViewScaleControl m_ScaleControl;

	private PhotonView m_PhotonView;

	private SeparatePhysicsObject m_PhysicsObject;

	private float m_InterpolateMaxDuration = 0.5f;

	private float m_LastReceivedNetworkTime;

	private bool m_firstTake;

	private void Awake()
	{
		m_PhotonView = GetComponent<PhotonView>();
		m_PhysicsObject = GetComponent<SeparatePhysicsObject>();
		m_PositionControl = new PhotonTransformViewPositionControl(m_PositionModel);
		m_RotationControl = new PhotonTransformViewRotationControl(m_RotationModel);
		m_ScaleControl = new PhotonTransformViewScaleControl(m_ScaleModel);
	}

	private void Start()
	{
		BatchUpdateManager.Inst.AddUpdate(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_firstTake = true;
	}

	public override void BatchUpdate()
	{
		if (m_PhotonView == null || m_PhotonView.isMine || !PhotonNetwork.connected)
		{
			if (m_PhysicsObject != null)
			{
				m_PhysicsObject.m_PhysicsBody.SetDisablePhysics(disablePhysics: false);
			}
			return;
		}
		if (m_PhysicsObject != null)
		{
			m_PhysicsObject.m_PhysicsBody.SetDisablePhysics(disablePhysics: true);
		}
		UpdatePosition();
		UpdateRotation();
		UpdateScale();
	}

	private void UpdatePosition()
	{
		if (m_PositionModel.SynchronizeEnabled && !(Time.time - m_LastReceivedNetworkTime > m_InterpolateMaxDuration))
		{
			if (m_PhysicsObject != null)
			{
				m_PhysicsObject.m_PhysicsBody.position = m_PositionControl.UpdatePosition(m_PhysicsObject.m_PhysicsBody.position);
			}
			else
			{
				base.transform.localPosition = m_PositionControl.UpdatePosition(base.transform.localPosition);
			}
		}
	}

	private void UpdateRotation()
	{
		if (m_RotationModel.SynchronizeEnabled && !(Time.time - m_LastReceivedNetworkTime > m_InterpolateMaxDuration))
		{
			if (m_PhysicsObject != null)
			{
				m_PhysicsObject.m_PhysicsBody.rotation = m_RotationControl.GetRotation(m_PhysicsObject.m_PhysicsBody.rotation);
			}
			else
			{
				base.transform.localRotation = m_RotationControl.GetRotation(base.transform.localRotation);
			}
		}
	}

	private void UpdateScale()
	{
		if (m_ScaleModel.SynchronizeEnabled && !(Time.time - m_LastReceivedNetworkTime > m_InterpolateMaxDuration))
		{
			base.transform.localScale = m_ScaleControl.GetScale(base.transform.localScale);
		}
	}

	public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
	{
		m_PositionControl.SetSynchronizedValues(speed, turnSpeed);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (m_PhysicsObject != null)
		{
			m_PositionControl.OnPhotonSerializeView(m_PhysicsObject.m_PhysicsBody.position, stream, info);
			m_RotationControl.OnPhotonSerializeView(m_PhysicsObject.m_PhysicsBody.rotation, stream, info);
		}
		else
		{
			m_PositionControl.OnPhotonSerializeView(base.transform.localPosition, stream, info);
			m_RotationControl.OnPhotonSerializeView(base.transform.localRotation, stream, info);
		}
		m_ScaleControl.OnPhotonSerializeView(base.transform.localScale, stream, info);
		if (!m_PhotonView.isMine && m_PositionModel.DrawErrorGizmo)
		{
			DoDrawEstimatedPositionError();
		}
		if (!stream.isReading)
		{
			return;
		}
		m_LastReceivedNetworkTime = Time.time;
		if (!m_firstTake)
		{
			return;
		}
		m_firstTake = false;
		if (m_PositionModel.SynchronizeEnabled)
		{
			if (m_PhysicsObject != null)
			{
				m_PhysicsObject.m_PhysicsBody.position = m_PositionControl.GetNetworkPosition();
			}
			else
			{
				base.transform.localPosition = m_PositionControl.GetNetworkPosition();
			}
		}
		if (m_RotationModel.SynchronizeEnabled)
		{
			if (m_PhysicsObject != null)
			{
				m_PhysicsObject.m_PhysicsBody.rotation = m_RotationControl.GetNetworkRotation();
			}
			else
			{
				base.transform.localRotation = m_RotationControl.GetNetworkRotation();
			}
		}
		if (m_ScaleModel.SynchronizeEnabled)
		{
			base.transform.localScale = m_ScaleControl.GetNetworkScale();
		}
	}

	private void DoDrawEstimatedPositionError()
	{
		Vector3 vector = m_PositionControl.GetNetworkPosition();
		if (base.transform.parent != null)
		{
			vector = base.transform.parent.position + vector;
		}
		UnityEngine.Debug.DrawLine(vector, base.transform.position, Color.red, 2f);
		UnityEngine.Debug.DrawLine(base.transform.position, base.transform.position + Vector3.up, Color.green, 2f);
		UnityEngine.Debug.DrawLine(vector, vector + Vector3.up, Color.red, 2f);
	}
}
