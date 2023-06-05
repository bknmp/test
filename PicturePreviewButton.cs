using LightUI;
using LightUtility;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PicturePreviewButton : MonoBehaviour
{
	public UIPopup m_PreviewUI;

	public Vector2 m_PreviewSize;

	public RawImage m_RawImage;

	private Texture m_Texture;

	private string m_Url;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(OnPreview);
		SetInfo(m_RawImage.mainTexture);
	}

	public void SetInfo(Texture tex)
	{
		m_Url = string.Empty;
		m_Texture = tex;
	}

	public void SetInfo(string url)
	{
		m_Url = url;
		RemoteTexture componentInChildren = GetComponentInChildren<RemoteTexture>();
		if (componentInChildren != null)
		{
			componentInChildren.SetTexture(url);
		}
	}

	private void OnClose()
	{
		UILobby.Current.GoBack();
	}

	private void OnPreview()
	{
		if (!string.IsNullOrEmpty(m_Url))
		{
			SetInfo(m_RawImage.mainTexture);
		}
		if (m_Texture != null)
		{
			GotoPreview();
		}
	}

	private void GotoPreview()
	{
		UILobby.Current.Popup(m_PreviewUI);
		RawImage componentInChildren = UILobby.Current.CurrentPopup().GetComponentInChildren<RawImage>();
		componentInChildren.texture = m_Texture;
		Texture texture = componentInChildren.texture;
		int num = texture.width;
		int num2 = texture.height;
		if (m_PreviewSize != Vector2.zero)
		{
			num = (int)m_PreviewSize.x;
			num2 = (int)m_PreviewSize.y;
		}
		componentInChildren.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num2);
	}
}
