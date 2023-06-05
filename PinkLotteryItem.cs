using LightUI;
using LightUtility;
using System;
using System.Collections;
using UnityEngine;

internal class PinkLotteryItem : DropItemUI_Binder
{
	public UIStateItem m_BG;

	public GameObject m_LimitLottery_GlowEffect;

	private ParticleSystem m_FX;

	private int m_Index;

	private bool m_HighLighting;

	private Coroutine m_Coroutine;

	private PinkLotteryPage_Bind m_PinkLotteryPageUI;

	public new void Bind(CommonDataCollection args)
	{
		base.Bind(args);
		m_BG.State = args["quality"];
		m_Index = args["index"];
		m_PinkLotteryPageUI = (args["pinkLotteryPageUI"].val as PinkLotteryPage_Bind);
		int num = m_DropItemUI.m_ItemName.text.IndexOf('\n');
		if (num > 0)
		{
			m_DropItemUI.m_ItemName.text = m_DropItemUI.m_ItemName.text.Substring(0, num);
		}
		ClearCallbacks();
		PinkLotteryPage_Bind pinkLotteryPageUI = m_PinkLotteryPageUI;
		pinkLotteryPageUI.m_HighLight = (Action<int>)Delegate.Combine(pinkLotteryPageUI.m_HighLight, new Action<int>(Highlight));
		PinkLotteryPage_Bind pinkLotteryPageUI2 = m_PinkLotteryPageUI;
		pinkLotteryPageUI2.m_SkipChange = (Action<bool>)Delegate.Combine(pinkLotteryPageUI2.m_SkipChange, new Action<bool>(OnSkipChange));
	}

	private void ClearCallbacks()
	{
		PinkLotteryPage_Bind pinkLotteryPageUI = m_PinkLotteryPageUI;
		pinkLotteryPageUI.m_HighLight = (Action<int>)Delegate.Remove(pinkLotteryPageUI.m_HighLight, new Action<int>(Highlight));
		PinkLotteryPage_Bind pinkLotteryPageUI2 = m_PinkLotteryPageUI;
		pinkLotteryPageUI2.m_SkipChange = (Action<bool>)Delegate.Remove(pinkLotteryPageUI2.m_SkipChange, new Action<bool>(OnSkipChange));
	}

	private void OnDisable()
	{
		ClearCallbacks();
	}

	private void Highlight(int index)
	{
		if (index == m_Index)
		{
			if (m_FX == null)
			{
				m_FX = UnityEngine.Object.Instantiate(m_LimitLottery_GlowEffect, m_Host.transform).GetComponentInChildren<ParticleSystem>();
			}
			m_Coroutine = m_Host.StartCoroutine(DoHighlight());
		}
	}

	private IEnumerator DoHighlight()
	{
		m_FX.gameObject.SetActive(value: true);
		m_HighLighting = true;
		m_FX.Play();
		yield return Yielders.GetWaitForSeconds(0.3f);
		m_FX.Stop();
		m_FX.Play();
		yield return Yielders.GetWaitForSeconds(0.3f);
		m_FX.Stop();
		m_FX.Play();
		yield return Yielders.GetWaitForSeconds(0.3f);
		m_FX.Stop();
		if (m_PinkLotteryPageUI.m_HighLightFinish != null)
		{
			m_PinkLotteryPageUI.m_HighLightFinish();
		}
		m_HighLighting = false;
		m_FX.gameObject.SetActive(value: false);
	}

	private void OnSkipChange(bool isOn)
	{
		if (isOn && m_HighLighting && m_Coroutine != null)
		{
			m_Host.StopCoroutine(m_Coroutine);
			if (m_PinkLotteryPageUI.m_HighLightFinish != null)
			{
				m_PinkLotteryPageUI.m_HighLightFinish();
			}
			m_HighLighting = false;
			m_FX.gameObject.SetActive(value: false);
		}
	}
}
