using GameMessages;
using LightUI;
using LightUtility;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRoom
{
	public class CustomRoomUtility
	{
		public static Dictionary<RoomMode, GameMessages.RoomInfo[]> Rooms = new Dictionary<RoomMode, GameMessages.RoomInfo[]>();

		public static Dictionary<RoomMode, int[]> RoomIDs = new Dictionary<RoomMode, int[]>();

		public static GameMessages.RoomInfo SearchedRoomInfo;

		public static int SelectedRoomID;

		public static float UpdateInterval = 1f;

		public static bool WaitingForEnterMainGame;

		private static List<CustomParamID> _OrderedParamIDs;

		public static List<CustomParamID> OrderedParamIDs
		{
			get
			{
				if (_OrderedParamIDs == null)
				{
					_OrderedParamIDs = new List<CustomParamID>();
					LocalResources.RoomCustomConfigInfo.Sort((RoomCustomConfigInfo x, RoomCustomConfigInfo y) => x.ShowOrder.CompareTo(y.ShowOrder));
					foreach (RoomCustomConfigInfo item in LocalResources.RoomCustomConfigInfo)
					{
						if (item.Enabled && !_OrderedParamIDs.Contains(item.ParamID))
						{
							_OrderedParamIDs.Add(item.ParamID);
						}
					}
				}
				return _OrderedParamIDs;
			}
		}

		public static RoomConfig DefaultRoomConfig
		{
			get
			{
				RoomConfig roomConfig = new RoomConfig();
				GradeInfo defaultGradeInfo = LocalResources.GradeTable.First;
				GradeMappingInfo gradeMappingInfo = LocalResources.GradeMappingTable.Find((GradeMappingInfo x) => x.GradeId == defaultGradeInfo.Id);
				roomConfig.grade = gradeMappingInfo.Id;
				roomConfig.map = 0;
				roomConfig.thiefCnt = 4;
				roomConfig.policeCnt = 1;
				roomConfig.pwd = string.Empty;
				roomConfig.mode = 0;
				List<CustomParam> list = new List<CustomParam>();
				List<CustomParam> list2 = new List<CustomParam>();
				List<CustomParam> list3 = new List<CustomParam>();
				foreach (CustomParamID orderedParamID in OrderedParamIDs)
				{
					RoomCustomConfigInfo roomCustomConfig = LocalResources.GetRoomCustomConfig(orderedParamID);
					if (roomCustomConfig != null && roomCustomConfig.Enabled)
					{
						list.Add(new CustomParam
						{
							id = (int)roomCustomConfig.ParamID,
							val = roomCustomConfig.DefaultLevelIndex
						});
					}
					roomCustomConfig = LocalResources.GetRoomCustomConfig(orderedParamID, RoleType.Police);
					if (roomCustomConfig != null && roomCustomConfig.Enabled)
					{
						list2.Add(new CustomParam
						{
							id = (int)roomCustomConfig.ParamID,
							val = roomCustomConfig.DefaultLevelIndex
						});
					}
					roomCustomConfig = LocalResources.GetRoomCustomConfig(orderedParamID, RoleType.Boss);
					if (roomCustomConfig != null && roomCustomConfig.Enabled)
					{
						CustomParam customParam = new CustomParam();
						customParam.id = (int)roomCustomConfig.ParamID;
						customParam.val = roomCustomConfig.DefaultLevelIndex;
						list3.Add(customParam);
					}
				}
				roomConfig.thiefParams = list.ToArray();
				roomConfig.policeParams = list2.ToArray();
				roomConfig.bossParams = list3.ToArray();
				return roomConfig;
			}
		}

		public static RoomConfig CachedRoomConfig
		{
			get
			{
				string prefValueString = LocalPlayerDatabase.GetPrefValueString("CachedRoomConfig1");
				if (string.IsNullOrEmpty(prefValueString))
				{
					return DefaultRoomConfig;
				}
				return JsonUtility.FromJson<RoomConfig>(prefValueString);
			}
			set
			{
				LocalPlayerDatabase.SetPrefValue("CachedRoomConfig1", JsonUtility.ToJson(value));
			}
		}

		public static void RefreshCustomRoomList(RoomMode mode, bool isAllMap, MapType mapType, Delegates.VoidCallback onSuccess = null, Delegates.VoidCallback onFailure = null)
		{
			HttpRequestRefreshRoomList httpRequestRefreshRoomList = new HttpRequestRefreshRoomList();
			httpRequestRefreshRoomList.mode = (int)mode;
			httpRequestRefreshRoomList.map = (int)mapType;
			httpRequestRefreshRoomList.all = isAllMap;
			GameHttpManager.Inst.Send(httpRequestRefreshRoomList, delegate(HttpResponseRefreshRoomList res)
			{
				Rooms[mode] = res.rooms;
				RoomIDs[mode] = GetRoomIDs(res.rooms);
				SearchedRoomInfo = null;
				SetDefaultSelectedRoomID(mode, isAllMap, mapType);
				UpdateInterval = Mathf.Max(1f, res.updateInterval);
				UIDataEvents.Inst.InvokeEvent("OnCustomRoomListChanged");
			}, onSuccess, onFailure);
		}

		public static void RefreshCustomRoomStatusSlient(int[] roomIDs, Delegates.VoidCallback onSuccess = null)
		{
			if (roomIDs != null && roomIDs.Length != 0)
			{
				HttpRequestRefreshRoomStatus httpRequestRefreshRoomStatus = new HttpRequestRefreshRoomStatus();
				httpRequestRefreshRoomStatus.roomIDs = roomIDs;
				GameHttpManager.Inst.SendNoWait(httpRequestRefreshRoomStatus, delegate(HttpResponseRefreshRoomStatus response)
				{
					bool flag = false;
					bool flag2 = false;
					RoomStatusInfo[] rooms = response.rooms;
					foreach (RoomStatusInfo roomStatusInfo in rooms)
					{
						GameMessages.RoomInfo roomInfo = GetRoomInfo(roomStatusInfo.ID);
						if (roomInfo != null)
						{
							if (!flag)
							{
								flag = (roomInfo.policeCnt != roomStatusInfo.policeCnt || roomInfo.thiefCnt != roomStatusInfo.thiefCnt || roomInfo.status != roomStatusInfo.status);
							}
							if (roomStatusInfo.status == RoomStatus.Destroyed && roomInfo.status != RoomStatus.Destroyed)
							{
								flag2 = true;
							}
							roomInfo.status = roomStatusInfo.status;
							if (roomStatusInfo.status != RoomStatus.Destroyed)
							{
								roomInfo.policeCnt = roomStatusInfo.policeCnt;
								roomInfo.thiefCnt = roomStatusInfo.thiefCnt;
							}
						}
					}
					if (flag2)
					{
						UIDataEvents.Inst.InvokeEvent("OnCustomRoomListChanged");
					}
					else if (flag)
					{
						UIDataEvents.Inst.InvokeEvent("OnCustomRoomStatusChanged");
					}
				}, onSuccess);
			}
		}

		public static void SearchCustomRoom(int roomID, Delegates.VoidCallback onSuccess)
		{
			HttpRequestSearchRoom httpRequestSearchRoom = new HttpRequestSearchRoom();
			httpRequestSearchRoom.roomID = roomID;
			GameHttpManager.Inst.Send(httpRequestSearchRoom, delegate(HttpResponseSearchRoom response)
			{
				if (response.room.IsNull())
				{
					UILobby.Current.ShowTips(Localization.TipsRoomNotFound);
				}
				else
				{
					SearchedRoomInfo = response.room;
					SetSelectedRoomID(response.room.ID);
					if (onSuccess != null)
					{
						onSuccess();
					}
				}
			});
		}

		public static void CreatCustomRoom(RoomConfig config, Delegates.ObjectCallback<HttpResponseCreateRoom> onResponse, Delegates.VoidCallback onSuccess = null)
		{
			HttpRequestCreateRoom httpRequestCreateRoom = new HttpRequestCreateRoom();
			httpRequestCreateRoom.config = config;
			GameHttpManager.Inst.Send(httpRequestCreateRoom, onResponse, onSuccess);
		}

		public static void JoinCustomRoom(int roomID, string pwd, RoleType roleType, Delegates.ObjectCallback<HttpResponseJoinRoom> onResponse, Delegates.VoidCallback onSuccess = null)
		{
			HttpRequestJoinRoom httpRequestJoinRoom = new HttpRequestJoinRoom();
			httpRequestJoinRoom.roomID = roomID;
			httpRequestJoinRoom.pwd = pwd;
			httpRequestJoinRoom.roleType = (int)roleType;
			GameHttpManager.Inst.Send(httpRequestJoinRoom, onResponse, onSuccess);
		}

		public static void RequestPhotonServer(int teamID, Delegates.ObjectCallback<HttpResponsePhotonServer> onResponse, Delegates.VoidCallback onSuccess = null)
		{
			HttpRequestPhotonServer httpRequestPhotonServer = new HttpRequestPhotonServer();
			httpRequestPhotonServer.teamID = teamID;
			GameHttpManager.Inst.Send(httpRequestPhotonServer, onResponse, onSuccess);
		}

		private static int[] GetRoomIDs(GameMessages.RoomInfo[] rooms)
		{
			int[] array = new int[rooms.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (rooms[i].status != RoomStatus.Destroyed)
				{
					array[i] = rooms[i].ID;
				}
			}
			return array;
		}

		public static GameMessages.RoomInfo GetRoomInfo(int roomID)
		{
			foreach (KeyValuePair<RoomMode, GameMessages.RoomInfo[]> room in Rooms)
			{
				GameMessages.RoomInfo[] value = room.Value;
				foreach (GameMessages.RoomInfo roomInfo in value)
				{
					if (roomInfo.ID == roomID && roomInfo.status != RoomStatus.Destroyed)
					{
						return roomInfo;
					}
				}
			}
			if (SearchedRoomInfo != null && SearchedRoomInfo.ID == roomID)
			{
				return SearchedRoomInfo;
			}
			return null;
		}

		public static void SetSelectedRoomID(int roomID)
		{
			if (SelectedRoomID != roomID)
			{
				SelectedRoomID = roomID;
				RoomViewPage_RoomDetail.SetRoom(roomID);
				UIDataEvents.Inst.InvokeEvent("OnCustomRoomStatusChanged");
			}
		}

		public static void SetDefaultSelectedRoomID(RoomMode roomMode, bool isAllMap = true, MapType map = MapType.Type4v1)
		{
			GameMessages.RoomInfo[] roomList = GetRoomList(roomMode);
			if (roomList != null)
			{
				SetSelectedRoomID(roomList.Find((GameMessages.RoomInfo x) => (!isAllMap) ? (x.config.Map() == map) : true)?.ID ?? 0);
			}
		}

		public static GameMessages.RoomInfo[] GetRoomList(RoomMode roomMode)
		{
			GameMessages.RoomInfo[] value = new GameMessages.RoomInfo[0];
			Rooms.TryGetValue(roomMode, out value);
			List<GameMessages.RoomInfo> list = new List<GameMessages.RoomInfo>();
			if (value != null)
			{
				GameMessages.RoomInfo[] array = value;
				foreach (GameMessages.RoomInfo roomInfo in array)
				{
					if (roomInfo.status != RoomStatus.Destroyed)
					{
						list.Add(roomInfo);
					}
				}
			}
			return list.ToArray();
		}

		public static bool ValidateName(string customRoomName)
		{
			if (string.IsNullOrEmpty(customRoomName))
			{
				UILobby.Current.ShowTips(Localization.TipsInvalidName);
				return false;
			}
			if (customRoomName.Contains(" ") || customRoomName.Contains("\u3000"))
			{
				UILobby.Current.ShowTips(Localization.TipsNameNoSpace);
				return false;
			}
			if (Regex.IsMatch(customRoomName, "[{} \\[ \\] \\^ \\-*×――(^)$%~!@$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]"))
			{
				UILobby.Current.ShowTips(Localization.TipsNameIllegalCharacter);
				return false;
			}
			if (customRoomName.Length < 2)
			{
				UILobby.Current.ShowTips(Localization.TipsNameTooShort);
				return false;
			}
			if (GameSettings.Inst.ContainsSensitiveWords(customRoomName))
			{
				UILobby.Current.ShowTips(Localization.TipsContainsSensitiveWord);
				return false;
			}
			return true;
		}

		public static void JoinWitnessGame(uint followID)
		{
			if (followID == 0)
			{
				UnityEngine.Debug.LogWarning("Try to witness followID 0");
				return;
			}
			UnityEngine.Debug.LogFormat("JoinWitnessGame:{0}", followID);
			LocalPlayerDatabase.RequestWitnessGame(followID, delegate(HttpResponseWitnessGame res)
			{
				GameRuntime.WitnessMode = true;
				GameRuntime.DefaultFollowID = followID;
				GameRuntime.SetRoomServer(res.photonServerConfig.ip, res.photonServerConfig.port, "");
				GameRuntime.SetRoomNameAndLevel(res.photonRoomName);
				GameUtility.EnterMainGame();
			});
		}
	}
}
