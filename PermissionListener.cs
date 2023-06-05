using LightUtility;
using UnityEngine;

public class PermissionListener : MonoBehaviour
{
	public enum Permission
	{
		AUDIO
	}

	private static PermissionListener _inst;

	private static bool IsInitialized;

	public Delegates.IntCallback m_Callback;

	public static PermissionListener Instance
	{
		get
		{
			if (_inst == null && Application.isPlaying)
			{
				GameObject gameObject = new GameObject("PermissionListener");
				_inst = gameObject.AddComponent<PermissionListener>();
				ResManager.DontDestroyOnLoad(gameObject);
			}
			return _inst;
		}
	}

	private void OnApplicationPause(bool pause)
	{
	}

	public void Initialize()
	{
		if (!IsInitialized)
		{
			IsInitialized = true;
			PermissionRecordAudio.AudioPermissionGranted = false;
			UnityEngine.Debug.Log("PermissionListener Initialize");
		}
	}

	public void SetCallBack(Delegates.IntCallback callback)
	{
		m_Callback = callback;
	}

	private void OnRequestPermissionResult(string result)
	{
		UnityEngine.Debug.Log("OnRequestPermissionResult " + result);
		int result2 = 0;
		int.TryParse(result, out result2);
		if (m_Callback != null)
		{
			m_Callback(result2);
		}
	}
}
