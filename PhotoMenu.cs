using GameMessages;
using LightUI;
using UnityEngine;
using UnityEngine.UI;

public class PhotoMenu : MonoBehaviour
{
	public Button m_SetIconButton;

	public Button m_SetTopButton;

	public Button m_DeleteButton;

	public Button m_ReportButton;

	public UIPopup m_ReportUI;

	private uint m_PlayerID;

	private PhotoInfo m_PhotoInfo;

	private void Awake()
	{
		m_SetIconButton.onClick.AddListener(OnSetIconButtonClicked);
		m_SetTopButton.onClick.AddListener(OnSetTopButtonClicked);
		m_DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
		m_ReportButton.onClick.AddListener(OnReportButtonClicked);
	}

	public void Show(PhotoInfo info, uint playerId)
	{
		m_PhotoInfo = info;
		m_PlayerID = playerId;
		base.gameObject.SetActive(value: true);
		SetMenuType();
	}

	private void SetMenuType()
	{
		if (m_PlayerID == LocalPlayerDatabase.LoginInfo.roleID)
		{
			m_SetIconButton.gameObject.SetActive(m_PhotoInfo.reviewState == 2);
			m_SetTopButton.gameObject.SetActive(m_PhotoInfo.reviewState == 2);
			m_DeleteButton.gameObject.SetActive(value: true);
			m_ReportButton.gameObject.SetActive(value: false);
		}
		else
		{
			m_SetIconButton.gameObject.SetActive(value: false);
			m_SetTopButton.gameObject.SetActive(value: false);
			m_DeleteButton.gameObject.SetActive(value: false);
			m_ReportButton.gameObject.SetActive(value: true);
		}
	}

	private void OnSetIconButtonClicked()
	{
		LocalPlayerDatabase.ChangeIcon(m_PhotoInfo.id, 1, delegate
		{
			PublicChatManager.m_PublicInfoChanged = true;
			UILobby.Current.ShowTips(Localization.TipsIconChanged);
			OnComplete();
		});
	}

	private void OnSetTopButtonClicked()
	{
		LocalPlayerDatabase.OperatePhoto(m_PhotoInfo.id, LocalPlayerDatabase.LoginInfo.roleID, PhotoOperate.SetTop, delegate
		{
			UILobby.Current.ShowTips(Localization.TipsPhotoSetTop);
			LocalPlayerDatabase.RefreshSelfPhotosInfo();
			OnComplete();
		});
	}

	private void OnDeleteButtonClicked()
	{
		bool isCurrentIcon = m_PhotoInfo.iconURL == LocalPlayerDatabase.PlayerInfo.publicInfo.icon;
		string msg = isCurrentIcon ? Localization.MsgBoxDeletePhotoAsIcon : Localization.MsgBoxDeletePhoto;
		UILobby.Current.ShowMessageBoxYesNo(msg, Localization.MsgBoxDelete, Localization.MsgBoxNo, Localization.MsgBoxTitle, delegate
		{
			LocalPlayerDatabase.DeletePhoto(m_PhotoInfo.id, delegate
			{
				UILobby.Current.ShowTips(Localization.TipsPhotoDeleted);
				if (isCurrentIcon)
				{
					LocalPlayerDatabase.RefreshPlayerInfo(delegate
					{
						LocalPlayerDatabase.RefreshSelfPhotosInfo();
					});
				}
				else
				{
					LocalPlayerDatabase.RefreshSelfPhotosInfo();
				}
				OnComplete();
			});
		}, null);
	}

	private void OnReportButtonClicked()
	{
		CommonDataCollection commonDataCollection = new CommonDataCollection();
		commonDataCollection["reportType"] = 1;
		commonDataCollection["photoID"] = m_PhotoInfo.id;
		commonDataCollection["photoUrl"] = m_PhotoInfo.fullURL;
		commonDataCollection["roleID"] = m_PlayerID;
		UILobby.Current.ShowUI(m_ReportUI, commonDataCollection);
		OnComplete();
	}

	private void OnComplete()
	{
		Hide();
	}

	public void Hide()
	{
		m_PlayerID = 0u;
		m_PhotoInfo = null;
		base.gameObject.SetActive(value: false);
	}
}
