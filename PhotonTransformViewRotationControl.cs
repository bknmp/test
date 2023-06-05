using UnityEngine;

public class PhotonTransformViewRotationControl
{
	private PhotonTransformViewRotationModel m_Model;

	private Quaternion m_NetworkRotation;

	public PhotonTransformViewRotationControl(PhotonTransformViewRotationModel model)
	{
		m_Model = model;
	}

	public Quaternion GetNetworkRotation()
	{
		return m_NetworkRotation;
	}

	public Quaternion GetRotation(Quaternion currentRotation)
	{
		switch (m_Model.InterpolateOption)
		{
		default:
			return m_NetworkRotation;
		case PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards:
			return Quaternion.RotateTowards(currentRotation, m_NetworkRotation, m_Model.InterpolateRotateTowardsSpeed * Time.deltaTime);
		case PhotonTransformViewRotationModel.InterpolateOptions.Lerp:
			return Quaternion.Lerp(currentRotation, m_NetworkRotation, m_Model.InterpolateLerpSpeed * Time.deltaTime);
		}
	}

	public void OnPhotonSerializeView(Quaternion currentRotation, PhotonStream stream, PhotonMessageInfo info)
	{
		if (!m_Model.SynchronizeEnabled)
		{
			return;
		}
		if (stream.isWriting)
		{
			if (m_Model.SyncZX)
			{
				stream.SendNext(currentRotation);
			}
			else
			{
				stream.SendNext(currentRotation.eulerAngles.y);
			}
			m_NetworkRotation = currentRotation;
		}
		else if (m_Model.SyncZX)
		{
			m_NetworkRotation = (Quaternion)stream.ReceiveNext();
		}
		else
		{
			m_NetworkRotation = Quaternion.Euler(0f, (float)stream.ReceiveNext(), 0f);
		}
	}

	public void Warp(Quaternion rot)
	{
		m_NetworkRotation = rot;
	}
}
