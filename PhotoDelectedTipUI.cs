using LightUI;

public class PhotoDelectedTipUI
{
	public UIDataBinder m_Host;

	public PlayerIcon m_Photo;

	public void Bind(CommonDataCollection args)
	{
		m_Photo.SetTexture(args["url"]);
	}
}
