using ExitGames.Client.Photon.Voice;
using LightUtility;
using Photon;
using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
[AddComponentMenu("Photon Voice/Photon Voice Speaker")]
public class PhotonVoiceSpeaker : Photon.MonoBehaviour
{
	private IAudioOut player;

	private bool started;

	public long LastRecvTime
	{
		get;
		private set;
	}

	public bool IsPlaying => player.IsPlaying;

	public int CurrentBufferLag => player.CurrentBufferLag;

	public bool IsVoiceLinked
	{
		get
		{
			if (player != null)
			{
				return started;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (PhotonVoiceSettings.Instance != null)
		{
			player = new AudioStreamPlayer(GetComponent<AudioSource>(), "PUNVoice: PhotonVoiceSpeaker:", PhotonVoiceSettings.Instance.DebugInfo);
			PhotonVoiceNetwork.LinkSpeakerToRemoteVoice(this);
		}
	}

	private void Start()
	{
		BatchUpdateManager.Inst.AddUpdate(this);
	}

	internal void OnVoiceLinked(int frequency, int channels, int frameSamplesPerChannel, int playDelayMs)
	{
		if (player != null)
		{
			player.Start(frequency, channels, frameSamplesPerChannel, playDelayMs);
			started = true;
		}
	}

	internal void OnVoiceUnlinked()
	{
		Loom.QueueOnMainThread(delegate
		{
			Cleanup();
		});
	}

	public override void BatchUpdate()
	{
		if (player != null)
		{
			player.Service();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (player != null)
		{
			PhotonVoiceNetwork.UnlinkSpeakerFromRemoteVoice(this);
			Cleanup();
		}
	}

	private void OnApplicationQuit()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (player != null)
		{
			player.Stop();
		}
		started = false;
	}

	internal void OnAudioFrame(float[] frame)
	{
		if (player != null)
		{
			LastRecvTime = DateTime.Now.Ticks;
			player.OnAudioFrame(frame);
		}
	}
}
