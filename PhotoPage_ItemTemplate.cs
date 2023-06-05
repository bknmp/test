using GameMessages;
using LightUI;

internal class PhotoPage_ItemTemplate
{
	public UIDataBinder m_Host;

	public PlayerIcon m_Icon;

	public UIStateItem m_State;

	public UISelectableButton m_SelectableBtn;

	public SelectableToggleGroup m_SelectableGroup;

	private int m_Index;

	private PhotoInfo m_PhotoInfo;

	public void Bind(CommonDataCollection args)
	{
		m_PhotoInfo = (PhotoInfo)args["photo"].val;
		m_Index = args["index"];
		bool num = args["selected"];
		m_State.State = args["state"];
		m_Icon.SetTexture(m_PhotoInfo.iconURL);
		m_SelectableBtn.onButtonClicked.RemoveAllListeners();
		m_SelectableBtn.onButtonClicked.AddListener(SelectPhoto);
		m_SelectableGroup.RegisterToggle(m_SelectableBtn);
		if (num)
		{
			args["selected"] = false;
			SelectPhoto();
		}
	}

	public void SelectPhoto()
	{
		m_SelectableGroup.NotifyToggleOn(m_SelectableBtn);
		PhotoPage.OnSelectedPhotoEvent?.Invoke(m_PhotoInfo.id, m_Index);
	}
}
