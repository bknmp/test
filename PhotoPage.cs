using GameMessages;
using LightUI;
using LightUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

internal class PhotoPage
{
	public UIDataBinder m_Host;

	public GameObject m_Empty;

	public GameObject m_NotEmpty;

	public Button m_MenuBtn;

	public Text m_Tips;

	public Text m_BrowseNum;

	public UIStateImage m_LikeIcon;

	public Text m_LikeNum;

	public UITemplateInitiator m_Icons;

	public UploadPhotoButton m_UploadPhotoButton;

	public PlayerIcon m_Photo;

	public GameObject m_PrasiedEffect;

	public Button m_LikeBtn;

	public PhotoMenu m_PhotonMenu;

	public GameObject m_Right;

	public UIScrollRect m_IconsScrollView;

	public Button m_OtherMenuBtn;

	public Text m_PhotoAmountTitle;

	public Text m_PhotoAmount;

	public UIPopup m_PhotoDelectedTipUI;

	private uint m_PlayerID;

	private HttpResponsePlayerInfo m_PlayerInfo;

	private HttpResponsePhotoList m_PhotosInfo;

	private string m_DeletedPhoto = "";

	private int m_SelectedIndex;

	private int m_TopId = -1;

	private PhotoInfo m_SelectedPhotoInfo;

	private string m_TipsFormat;

	private static HashSet<string> BrowsedPhotos = new HashSet<string>();

	public static Action<int, int> OnSelectedPhotoEvent;

	public void Bind(CommonDataCollection args)
	{
		if (m_TipsFormat == null)
		{
			m_TipsFormat = m_Tips.text;
		}
		m_PlayerID = args["roleID"];
		m_PlayerInfo = (HttpResponsePlayerInfo)args["playerInfo"].val;
		bool myself = m_PlayerID == LocalPlayerDatabase.LoginInfo.roleID;
		if (TabPageAnchor.TabEnter)
		{
			TabPageAnchor.TabEnter = false;
			m_TopId = -1;
			m_SelectedIndex = 0;
			m_IconsScrollView.ScrollToStart(immediately: true);
			m_NotEmpty.SetActive(value: false);
			m_Empty.SetActive(value: false);
			m_Right.SetActive(value: false);
		}
		LocalPlayerDatabase.RequestPhotosInfo(m_PlayerID, delegate(HttpResponsePhotoList res)
		{
			m_PhotosInfo = res;
			if (myself)
			{
				LocalPlayerDatabase.PhotosInfo = m_PhotosInfo;
			}
			RefreshUI();
			LoadCustomIcons();
		});
		m_MenuBtn.gameObject.SetActive(myself);
		m_OtherMenuBtn.gameObject.SetActive(!myself);
		m_Host.EventProxy(m_MenuBtn, "OnMenuClicked");
		m_Host.EventProxy(m_OtherMenuBtn, "OnMenuClicked");
		m_Host.EventProxy(m_LikeBtn, "OnLikeClicked");
		OnSelectedPhotoEvent = OnPhotoSelected;
		m_PhotonMenu.Hide();
	}

	private void RefreshUI()
	{
		bool flag = m_PlayerID == LocalPlayerDatabase.LoginInfo.roleID;
		m_UploadPhotoButton.gameObject.SetActive(flag);
		if (flag)
		{
			CheckPhotonState();
			List<PhotoInfo> list = m_PhotosInfo.photos.ToList().FindAll((PhotoInfo x) => x.reviewState != 1);
			m_PhotoAmountTitle.text = "/" + m_PhotosInfo.photoAmountLimit;
			m_PhotoAmount.text = list.Count.ToString();
			if (list.Count > m_PhotosInfo.photoAmountLimit)
			{
				m_PhotoAmount.color = Color.red;
			}
			else
			{
				m_PhotoAmount.color = new Color(141f, 176f, 242f, 1f);
			}
		}
		else
		{
			List<PhotoInfo> list2 = m_PhotosInfo.photos.ToList().FindAll((PhotoInfo x) => x.reviewState != 1);
			m_PhotoAmountTitle.text = "/" + m_PhotosInfo.photoAmountLimit;
			m_PhotoAmount.text = list2.Count.ToString();
		}
	}

	private void CheckPhotonState()
	{
		PhotoInfo[] photos = m_PhotosInfo.photos;
		foreach (PhotoInfo photoInfo in photos)
		{
			if (photoInfo.reviewState == 1)
			{
				m_DeletedPhoto = photoInfo.iconURL;
			}
		}
		if (m_DeletedPhoto.Length > 0)
		{
			CommonDataCollection commonDataCollection = new CommonDataCollection();
			commonDataCollection["url"] = m_DeletedPhoto;
			UILobby.Current.ShowUI(m_PhotoDelectedTipUI, commonDataCollection).OnCloseOnce = delegate
			{
				m_DeletedPhoto = "";
			};
		}
	}

	public void OnMenuClicked()
	{
		m_PhotonMenu.Show(m_SelectedPhotoInfo, m_PlayerID);
	}

	public void OnLikeClicked()
	{
		if (m_PlayerID == LocalPlayerDatabase.LoginInfo.roleID)
		{
			UILobby.Current.ShowTips(Localization.TipsPhtotPraisedSelf);
		}
		else if (m_SelectedPhotoInfo.isPraised)
		{
			UILobby.Current.ShowTips(Localization.TipsPhotoHadPraised);
		}
		else
		{
			LocalPlayerDatabase.OperatePhoto(m_SelectedPhotoInfo.id, m_PlayerID, PhotoOperate.Like, delegate
			{
				UnityEngine.Object.Instantiate(m_PrasiedEffect, m_LikeBtn.transform, worldPositionStays: false);
				m_LikeIcon.State = 1;
				m_LikeNum.text = (++m_SelectedPhotoInfo.praisedTimes).ToString();
				m_SelectedPhotoInfo.isPraised = true;
			});
		}
	}

	private void LoadCustomIcons()
	{
		List<PhotoInfo> list = m_PhotosInfo.photos.ToList().FindAll((PhotoInfo x) => x.reviewState != 1);
		int num;
		if (LocalPlayerDatabase.EndUploadPhoto != null)
		{
			num = ArrayUtility.IndexOf((from a in list
				select a.id).ToArray(), LocalPlayerDatabase.EndUploadPhoto.id);
			LocalPlayerDatabase.EndUploadPhoto = null;
		}
		else
		{
			int num2 = ArrayUtility.IndexOf((from a in list
				select a.id).ToArray(), m_PhotosInfo.stickID);
			if (num2 == -1 || m_PhotosInfo.stickID == m_TopId)
			{
				num = ((m_SelectedIndex < list.Count) ? m_SelectedIndex : (list.Count - 1));
			}
			else
			{
				m_TopId = m_PhotosInfo.stickID;
				PhotoInfo item = list[num2];
				list.RemoveAt(num2);
				list.Insert(0, item);
				num = 0;
			}
		}
		CommonDataCollection commonDataCollection = new CommonDataCollection();
		for (int i = 0; i < list.Count; i++)
		{
			PhotoInfo photoInfo = list[i];
			commonDataCollection[i]["photo"].val = photoInfo;
			commonDataCollection[i]["selected"] = (i == num);
			commonDataCollection[i]["index"] = i;
			commonDataCollection[i]["state"] = ((m_PlayerInfo.publicInfo.icon == photoInfo.iconURL) ? 3 : photoInfo.reviewState);
		}
		m_Right.SetActive(value: true);
		m_Icons.Args = commonDataCollection;
		m_UploadPhotoButton.transform.SetAsLastSibling();
		if (list.Count > 0)
		{
			m_NotEmpty.SetActive(value: true);
			m_Empty.SetActive(value: false);
		}
		else
		{
			m_NotEmpty.SetActive(value: false);
			m_Empty.SetActive(value: true);
			m_Empty.GetComponentInChildren<Text>().text = Localization.TipsPhotoWarning;
		}
	}

	private void OnPhotoSelected(int id, int index)
	{
		m_SelectedIndex = index;
		if (m_PhotosInfo == null || m_PhotosInfo.photos == null)
		{
			return;
		}
		List<PhotoInfo> list = m_PhotosInfo.photos.ToList();
		if (list.Count == 0)
		{
			return;
		}
		List<PhotoInfo> list2 = list.FindAll((PhotoInfo x) => x.reviewState != 1);
		if (list2 == null || list2.Count == 0)
		{
			return;
		}
		PhotoInfo photoInfo = list2.FirstOrDefault((PhotoInfo t) => t.id == id);
		if (photoInfo != null)
		{
			if ((bool)m_Photo)
			{
				m_Photo.SetTexture(photoInfo.fullURL);
			}
			if ((bool)m_BrowseNum)
			{
				m_BrowseNum.text = photoInfo.scanedTimes.ToString();
			}
			if ((bool)m_LikeIcon)
			{
				m_LikeIcon.State = (photoInfo.isPraised ? 1 : 0);
			}
			if ((bool)m_LikeNum)
			{
				m_LikeNum.text = photoInfo.praisedTimes.ToString();
			}
			if (m_PlayerID != LocalPlayerDatabase.LoginInfo.roleID && !BrowsedPhotos.Contains(photoInfo.fullURL))
			{
				BrowsedPhotos.Add(photoInfo.fullURL);
				LocalPlayerDatabase.OperatePhoto(photoInfo.id, m_PlayerID, PhotoOperate.Browse);
			}
			m_SelectedPhotoInfo = photoInfo;
			if ((bool)m_Tips)
			{
				m_Tips.text = string.Format(m_TipsFormat, index + 1, list2.Count, m_PhotosInfo.photoAmountLimit);
			}
		}
	}
}
