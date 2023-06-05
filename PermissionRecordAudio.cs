using LightUI;

public class PermissionRecordAudio
{
	public delegate void OnVoiceDeny();

	public static OnVoiceDeny onVoiceDeny;

	private static bool m_AudioPermissionGranted;

	public static bool AudioPermissionGranted
	{
		get
		{
			return m_AudioPermissionGranted;
		}
		set
		{
			m_AudioPermissionGranted = value;
		}
	}

	public static void CallBack(int result)
	{
		m_AudioPermissionGranted = (result == 1);
		if (result == 1)
		{
			GameSettings.Inst.VoiceEnable = true;
			GameSettings.Inst.SavePlayerSettings();
			UILobby.Current.ShowTips(Localization.TipsVoiceOn);
			return;
		}
		UILobby.Current.ShowTips(Localization.TipsVoicePermission);
		GameSettings.Inst.VoiceEnable = false;
		GameSettings.Inst.SavePlayerSettings();
		if (onVoiceDeny != null)
		{
			onVoiceDeny();
		}
	}
}
