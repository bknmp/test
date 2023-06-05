using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PhraseManager
{
	[Serializable]
	public class PhraseStore
	{
		[SerializeField]
		public int typeID;

		[SerializeField]
		public List<string> phrases;

		public PhraseStore(int typeID, List<string> phrases)
		{
			this.typeID = typeID;
			this.phrases = phrases;
		}
	}

	private static string PhraseFile = "";

	private static Dictionary<int, PhraseStore> m_PhraseDict = new Dictionary<int, PhraseStore>();

	private static Dictionary<int, PhraseStore> m_SerializableData = new Dictionary<int, PhraseStore>();

	public static void Load(uint roleID)
	{
		PhraseFile = $"{Application.persistentDataPath}/{roleID}Phrases.json";
		try
		{
			string text = "";
			if (File.Exists(PhraseFile))
			{
				text = File.ReadAllText(PhraseFile);
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					m_SerializableData = JsonUtility.FromJson<Serialization<int, PhraseStore>>(text).ToDictionary();
					foreach (KeyValuePair<int, PhraseStore> serializableDatum in m_SerializableData)
					{
						m_PhraseDict[serializableDatum.Key] = serializableDatum.Value;
					}
					m_SerializableData.Clear();
				}
				catch (Exception message)
				{
					SetDefaultPhraseDict();
					UnityEngine.Debug.LogWarning(message);
				}
			}
			if (m_PhraseDict.Count == 0)
			{
				SetDefaultPhraseDict();
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	private static void SetDefaultPhraseDict()
	{
		m_PhraseDict.Clear();
		m_PhraseDict[1] = new PhraseStore(1, LocalResources.PhraseTable.Find((PhraseInfo x) => x.Type == PhraseType.ThiefTeam).Messages.ToList());
		m_PhraseDict[2] = new PhraseStore(2, LocalResources.PhraseTable.Find((PhraseInfo x) => x.Type == PhraseType.ThiefAll).Messages.ToList());
		m_PhraseDict[3] = new PhraseStore(3, LocalResources.PhraseTable.Find((PhraseInfo x) => x.Type == PhraseType.PoliceTeam).Messages.ToList());
		m_PhraseDict[4] = new PhraseStore(4, LocalResources.PhraseTable.Find((PhraseInfo x) => x.Type == PhraseType.PoliceAll).Messages.ToList());
	}

	public static void Save()
	{
		try
		{
			if (m_PhraseDict.Count != 0)
			{
				m_SerializableData.Clear();
				foreach (KeyValuePair<int, PhraseStore> item in m_PhraseDict)
				{
					m_SerializableData[item.Key] = item.Value;
				}
				string contents = JsonUtility.ToJson(new Serialization<int, PhraseStore>(m_SerializableData));
				File.WriteAllText(PhraseFile, contents);
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	public static List<string> GetPhrases(int type)
	{
		if (!m_PhraseDict.ContainsKey(type))
		{
			return null;
		}
		return m_PhraseDict[type].phrases;
	}

	public static void ModifyPhrase(int type, int id, string text)
	{
		if (m_PhraseDict.ContainsKey(type))
		{
			List<string> phrases = m_PhraseDict[type].phrases;
			if (id <= phrases.Count)
			{
				phrases[id - 1] = text;
			}
			else
			{
				UnityEngine.Debug.LogError("Array index is out of range");
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Can't find the key : " + type);
		}
	}
}
