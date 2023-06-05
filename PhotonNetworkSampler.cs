using PerformanceSDK;
using PerformanceSDK.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotonNetworkSampler : ISampler
{
	[Serializable]
	public class PhotonLatencySamplerData
	{
		public int maxLatency;

		public int minLatency;

		public float avgLatency;

		public float variLatency;

		public int[] latencyData;

		public int totalOutgoingTraffic;

		public int totalIncomingTraffic;

		public int maxOutgoingTraffic;

		public int minOutgoingTraffic;

		public int maxIncomingTraffic;

		public int minIncomingTraffic;

		public float duration;

		public float sampleInterval;

		public int reconnectCount;
	}

	private PhotonLatencySamplerData reportData = new PhotonLatencySamplerData();

	private List<int> latency = new List<int>();

	private float sampleInterval = 1f;

	private float duration;

	private float avgLatency;

	private float variLatency;

	private int sampleFrameCount;

	private int maxLatency;

	private int minLatency = int.MaxValue;

	private float sampleTimer;

	private int sampleAccumFrameCounter;

	private int latencyAccum;

	private int outgoingTrafficAccum;

	private int incomingTrafficAccum;

	private int totalOutgoingTraffic;

	private int totalIncomingTraffic;

	private int maxOutgoingTraffic;

	private int minOutgoingTraffic = int.MaxValue;

	private int maxIncomingTraffic;

	private int minIncomingTraffic = int.MaxValue;

	public string tag
	{
		get;
		private set;
	}

	[SamplerCreator("Network")]
	public static ISampler Create(SampleSetting setting)
	{
		return new PhotonNetworkSampler(setting);
	}

	public PhotonNetworkSampler(SampleSetting setting)
	{
		tag = setting.tag;
	}

	private void Reset()
	{
		latency.Clear();
		sampleInterval = 1f;
		duration = 0f;
		avgLatency = 0f;
		variLatency = 0f;
		sampleFrameCount = 0;
		maxLatency = 0;
		minLatency = int.MaxValue;
		sampleTimer = 0f;
		sampleAccumFrameCounter = 0;
		latencyAccum = 0;
		totalOutgoingTraffic = 0;
		totalIncomingTraffic = 0;
		maxOutgoingTraffic = 0;
		minOutgoingTraffic = int.MaxValue;
		maxIncomingTraffic = 0;
		minIncomingTraffic = int.MaxValue;
	}

	public void OnSample()
	{
		float deltaTimeFromLastFrame = TimeManager.DeltaTimeFromLastFrame;
		duration += deltaTimeFromLastFrame;
		sampleTimer += deltaTimeFromLastFrame;
		sampleFrameCount++;
		NetworkingPeer networkingPeer = PhotonNetwork.networkingPeer;
		if (networkingPeer != null && !PhotonNetwork.offlineMode)
		{
			int roundTripTime = networkingPeer.RoundTripTime;
			float num = Mathf.Approximately(avgLatency, 0f) ? ((float)roundTripTime) : avgLatency;
			avgLatency = num + ((float)roundTripTime - num) / (float)sampleFrameCount;
			variLatency = (float)(sampleFrameCount - 1) / (float)sampleFrameCount * (variLatency + 1f / (float)sampleFrameCount * ((float)roundTripTime - num) * ((float)roundTripTime - num));
			latencyAccum += roundTripTime;
			outgoingTrafficAccum = ((networkingPeer.TrafficStatsOutgoing != null) ? networkingPeer.TrafficStatsOutgoing.TotalPacketBytes : 0);
			incomingTrafficAccum = ((networkingPeer.TrafficStatsIncoming != null) ? networkingPeer.TrafficStatsIncoming.TotalPacketBytes : 0);
			sampleAccumFrameCounter++;
			if (sampleTimer >= sampleInterval)
			{
				float num2 = (float)latencyAccum / (float)sampleAccumFrameCounter;
				latency.Add((int)num2);
				maxLatency = Mathf.Max((int)num2, maxLatency);
				minLatency = Mathf.Min((int)num2, minLatency);
				int num3 = outgoingTrafficAccum;
				int num4 = incomingTrafficAccum;
				latencyAccum = 0;
				outgoingTrafficAccum = 0;
				incomingTrafficAccum = 0;
				sampleTimer = 0f;
				sampleAccumFrameCounter = 0;
				maxOutgoingTraffic = Math.Max(num3, maxOutgoingTraffic);
				minOutgoingTraffic = Math.Min(num3, minOutgoingTraffic);
				maxIncomingTraffic = Math.Max(num4, maxIncomingTraffic);
				minIncomingTraffic = Math.Min(num4, minIncomingTraffic);
				totalOutgoingTraffic += num3;
				totalIncomingTraffic += num4;
				PhotonNetwork.NetworkStatisticsReset();
			}
		}
	}

	public void OnBeginSample()
	{
		Reset();
		PhotonNetwork.NetworkStatisticsReset();
	}

	public object OnEndSample()
	{
		reportData.latencyData = latency.ToArray();
		reportData.avgLatency = avgLatency;
		reportData.variLatency = variLatency;
		reportData.maxLatency = maxLatency;
		reportData.minLatency = minLatency;
		reportData.totalOutgoingTraffic = totalOutgoingTraffic;
		reportData.totalIncomingTraffic = totalIncomingTraffic;
		reportData.maxOutgoingTraffic = maxOutgoingTraffic;
		reportData.minOutgoingTraffic = minOutgoingTraffic;
		reportData.maxIncomingTraffic = maxIncomingTraffic;
		reportData.minIncomingTraffic = minIncomingTraffic;
		reportData.duration = duration;
		reportData.sampleInterval = sampleInterval;
		reportData.reconnectCount = ((null != InGameReconnect.Inst) ? InGameReconnect.Inst.ReconnectCount : 0);
		return reportData;
	}
}
