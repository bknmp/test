using GameMessages;
using LightUI;
using LightUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XLua
{
	public static class SysGenConfig
	{
		[LuaCallCSharp(GenFlag.No)]
		[ReflectionUse]
		public static List<Type> LuaCallCSharp_TypeList = new List<Type>
		{
			typeof(Application),
			typeof(Time),
			typeof(Screen),
			typeof(Resources),
			typeof(Physics),
			typeof(Rigidbody),
			typeof(Collider),
			typeof(Camera),
			typeof(UnityEngine.Object),
			typeof(GameObject),
			typeof(Transform),
			typeof(Material),
			typeof(RectTransform),
			typeof(Component),
			typeof(Text),
			typeof(Image),
			typeof(RawImage),
			typeof(Sprite),
			typeof(Texture),
			typeof(Texture2D),
			typeof(Shader),
			typeof(Renderer),
			typeof(Texture),
			typeof(Behaviour),
			typeof(MonoBehaviour),
			typeof(LayerMask),
			typeof(SkinnedMeshRenderer),
			typeof(MeshRenderer),
			typeof(BoxCollider),
			typeof(MeshCollider),
			typeof(SphereCollider),
			typeof(CapsuleCollider),
			typeof(Animation),
			typeof(AnimationClip),
			typeof(AnimationState),
			typeof(AnimationBlendMode),
			typeof(XLuaHotFixManager),
			typeof(LocalPlayerDatabase),
			typeof(LocalResources),
			typeof(InGameScene),
			typeof(AssetBundleUpdater),
			typeof(Delegates),
			typeof(ResourceSource),
			typeof(GameObjectUtility),
			typeof(StringUtility),
			typeof(UILobby),
			typeof(UIPage),
			typeof(UIPopup),
			typeof(UIStateColor),
			typeof(UIStateImage),
			typeof(UIStateItem),
			typeof(UIStateRawImage),
			typeof(HttpRequestBase<HttpResponseBase>)
		};

		[CSharpCallLua]
		[ReflectionUse]
		public static List<Type> CSharpCallLua_TypeList = new List<Type>
		{
			typeof(UnityAction),
			typeof(Delegates.ObjectCallback<HttpResponseBase>),
			typeof(Delegates.ObjectCallback<Texture2D>),
			typeof(Delegates.ObjectCallback<Sprite>)
		};

		[GCOptimize(OptimizeFlag.Default)]
		private static List<Type> GCOptimize => new List<Type>
		{
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Color),
			typeof(Quaternion),
			typeof(Ray),
			typeof(Bounds),
			typeof(Ray2D)
		};

		[AdditionalProperties]
		private static Dictionary<Type, List<string>> AdditionalProperties => new Dictionary<Type, List<string>>
		{
			{
				typeof(Ray),
				new List<string>
				{
					"origin",
					"direction"
				}
			},
			{
				typeof(Ray2D),
				new List<string>
				{
					"origin",
					"direction"
				}
			},
			{
				typeof(Bounds),
				new List<string>
				{
					"center",
					"extents"
				}
			}
		};
	}
}
