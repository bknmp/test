using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class PhotonHandler : MonoBehaviour
{
	public static PhotonHandler SP;

	public int updateInterval;

	public int updateIntervalOnSerialize;

	private int nextSendTickCount;

	private int nextSendTickCountOnSerialize;

	private static bool sendThreadShouldRun;

	private static Stopwatch timerToStopConnectionInBackground;

	protected internal static bool AppQuits;

	protected internal static Type PingImplementation;

	private const string PlayerPrefsKey = "PUNCloudBestRegion";

	internal static CloudRegionCode BestRegionCodeInPreferences
	{
		get
		{
			string @string = PlayerPrefs.GetString("PUNCloudBestRegion", "");
			if (!string.IsNullOrEmpty(@string))
			{
				return Region.Parse(@string);
			}
			return CloudRegionCode.none;
		}
		set
		{
			if (value == CloudRegionCode.none)
			{
				PlayerPrefs.DeleteKey("PUNCloudBestRegion");
			}
			else
			{
				PlayerPrefs.SetString("PUNCloudBestRegion", value.ToString());
			}
		}
	}

	protected void Awake()
	{
		if (SP != null && SP != this && SP.gameObject != null)
		{
			UnityEngine.Object.DestroyImmediate(SP.gameObject);
		}
		SP = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		updateInterval = 1000 / PhotonNetwork.sendRate;
		updateIntervalOnSerialize = 1000 / PhotonNetwork.sendRateOnSerialize;
		StartFallbackSendAckThread();
	}

	protected void Start()
	{
		SceneManager.sceneLoaded += delegate
		{
			PhotonNetwork.networkingPeer.NewSceneLoaded();
			PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
		};
	}

	protected void OnApplicationQuit()
	{
		AppQuits = true;
		StopFallbackSendAckThread();
		PhotonNetwork.Disconnect();
	}

	protected void OnApplicationPause(bool pause)
	{
		if (PhotonNetwork.BackgroundTimeout > 0.1f)
		{
			if (timerToStopConnectionInBackground == null)
			{
				timerToStopConnectionInBackground = new Stopwatch();
			}
			timerToStopConnectionInBackground.Reset();
			if (pause)
			{
				timerToStopConnectionInBackground.Start();
			}
			else
			{
				timerToStopConnectionInBackground.Stop();
			}
		}
	}

	protected void OnDestroy()
	{
		StopFallbackSendAckThread();
	}

	protected void Update()
	{
		if (PhotonNetwork.networkingPeer == null)
		{
			UnityEngine.Debug.LogError("NetworkPeer broke!");
		}
		else
		{
			if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated || PhotonNetwork.connectionStateDetailed == ClientState.Disconnected || PhotonNetwork.offlineMode || !PhotonNetwork.isMessageQueueRunning)
			{
				return;
			}
			bool flag = true;
			while (PhotonNetwork.isMessageQueueRunning && flag)
			{
				flag = PhotonNetwork.networkingPeer.DispatchIncomingCommands();
			}
			int num = (int)(Time.realtimeSinceStartup * 1000f);
			if (PhotonNetwork.isMessageQueueRunning && num > nextSendTickCountOnSerialize)
			{
				PhotonNetwork.networkingPeer.RunViewUpdate();
				nextSendTickCountOnSerialize = num + updateIntervalOnSerialize;
				nextSendTickCount = 0;
			}
			num = (int)(Time.realtimeSinceStartup * 1000f);
			if (num > nextSendTickCount)
			{
				bool flag2 = true;
				while (PhotonNetwork.isMessageQueueRunning && flag2)
				{
					flag2 = PhotonNetwork.networkingPeer.SendOutgoingCommands();
				}
				nextSendTickCount = num + updateInterval;
			}
		}
	}

	protected void OnJoinedRoom()
	{
		PhotonNetwork.networkingPeer.LoadLevelIfSynced();
	}

	protected void OnCreatedRoom()
	{
		PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
	}

	public static void StartFallbackSendAckThread()
	{
		if (!sendThreadShouldRun)
		{
			sendThreadShouldRun = true;
			SupportClass.StartBackgroundCalls(FallbackSendAckThread);
		}
	}

	public static void StopFallbackSendAckThread()
	{
		sendThreadShouldRun = false;
	}

	public static bool FallbackSendAckThread()
	{
		if (sendThreadShouldRun && !PhotonNetwork.offlineMode && PhotonNetwork.networkingPeer != null)
		{
			if (timerToStopConnectionInBackground != null && PhotonNetwork.BackgroundTimeout > 0.1f && (float)timerToStopConnectionInBackground.ElapsedMilliseconds > PhotonNetwork.BackgroundTimeout * 1000f)
			{
				if (PhotonNetwork.connected)
				{
					PhotonNetwork.Disconnect();
				}
				timerToStopConnectionInBackground.Stop();
				timerToStopConnectionInBackground.Reset();
				return sendThreadShouldRun;
			}
			if (!PhotonNetwork.isMessageQueueRunning || PhotonNetwork.networkingPeer.ConnectionTime - PhotonNetwork.networkingPeer.LastSendOutgoingTime > 200)
			{
				PhotonNetwork.networkingPeer.SendAcksOnly();
			}
		}
		return sendThreadShouldRun;
	}

	protected internal static void PingAvailableRegionsAndConnectToBest()
	{
		SP.StartCoroutine(SP.PingAvailableRegionsCoroutine(connectToBest: true));
	}

	internal IEnumerator PingAvailableRegionsCoroutine(bool connectToBest)
	{
		while (PhotonNetwork.networkingPeer.AvailableRegions == null)
		{
			if (PhotonNetwork.connectionStateDetailed != ClientState.ConnectingToNameServer && PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToNameServer)
			{
				UnityEngine.Debug.LogError("Call ConnectToNameServer to ping available regions.");
				yield break;
			}
			UnityEngine.Debug.Log("Waiting for AvailableRegions. State: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.Server + " PhotonNetwork.networkingPeer.AvailableRegions " + (PhotonNetwork.networkingPeer.AvailableRegions != null).ToString());
			yield return new WaitForSeconds(0.25f);
		}
		if (PhotonNetwork.networkingPeer.AvailableRegions == null || PhotonNetwork.networkingPeer.AvailableRegions.Count == 0)
		{
			UnityEngine.Debug.LogError("No regions available. Are you sure your appid is valid and setup?");
			yield break;
		}
		PhotonPingManager pingManager = new PhotonPingManager();
		foreach (Region availableRegion in PhotonNetwork.networkingPeer.AvailableRegions)
		{
			SP.StartCoroutine(pingManager.PingSocket(availableRegion));
		}
		while (!pingManager.Done)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Region bestRegion = pingManager.BestRegion;
		BestRegionCodeInPreferences = bestRegion.Code;
		UnityEngine.Debug.Log("Found best region: '" + bestRegion.Code + "' ping: " + bestRegion.Ping + ". Calling ConnectToRegionMaster() is: " + connectToBest.ToString());
		if (connectToBest)
		{
			PhotonNetwork.networkingPeer.ConnectToRegionMaster(bestRegion.Code);
		}
	}
}
