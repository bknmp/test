using UnityEngine.UI;

namespace CustomRoom
{
	public static class CustomRoomUtilityExtension
	{
		public static int GetToggleGroupValue(this ToggleGroup toggleGroup)
		{
			return ToggleGroupUtility.GetToggleGroupValue(toggleGroup);
		}

		public static bool Contains(this RoomCustomConfigInfo info, RoleType roleType)
		{
			if (info.RoleType.Length != 0)
			{
				return info.RoleType.Contains((int)roleType);
			}
			return true;
		}
	}
}
