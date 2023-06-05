using GameMessages;
using LightUI;
using LightUtility;
using UnityEngine;
using UnityEngine.UI;

public class PictureItem : MonoBehaviour
{
	public UIStateItem m_State;

	public Button m_AddButton;

	public Button m_DeleteButton;

	public PicturePreviewButton m_PreviewButton;

	public RemoteTexture m_RemoteTexture;

	private int m_Idx;

	private Delegates.ObjectCallback2<int, string> m_OnUploadSuccess;

	private Delegates.ObjectCallback<int> m_OnDeleteSuccess;

	private void Awake()
	{
		m_AddButton.onClick.AddListener(OnAdd);
		m_DeleteButton.onClick.AddListener(OnDelete);
	}

	public void SetInfo(int idx, string picUrl, Delegates.ObjectCallback2<int, string> onUploadSuccess, Delegates.ObjectCallback<int> onDeleteSuccess)
	{
		m_Idx = idx;
		m_OnUploadSuccess = onUploadSuccess;
		m_OnDeleteSuccess = onDeleteSuccess;
		if (string.IsNullOrEmpty(picUrl))
		{
			m_State.State = 1;
			return;
		}
		m_State.State = 0;
		m_PreviewButton.SetInfo(picUrl);
	}

	private void OnAdd()
	{
		UploadPhotoManager.Inst.StartUploadPicture(UploadType.IOSRechargeFeedback, UploadSuccess);
	}

	private void UploadSuccess(Texture2D tex, string url)
	{
		if (m_OnUploadSuccess != null)
		{
			m_OnUploadSuccess(m_Idx, url);
		}
		m_State.State = 0;
		m_RemoteTexture.SetTexture(string.Empty, tex);
		RemoteTextureManager.Inst.SaveTextureToStorage(tex, url);
		m_PreviewButton.SetInfo(tex);
	}

	private void OnDelete()
	{
		m_RemoteTexture.SetTexture(null);
		m_State.State = 1;
		if (m_OnDeleteSuccess != null)
		{
			m_OnDeleteSuccess(m_Idx);
		}
	}
}
