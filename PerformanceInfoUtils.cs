using System;

public static class PerformanceInfoUtils
{
	public static string GetMono()
	{
		return GC.GetTotalMemory(forceFullCollection: false).ToString();
	}

	public static float GetPss()
	{
		return AndroidUtils.GetPss();
	}

	public static float GetTemperature()
	{
		return AndroidUtils.GetTemperature();
	}

	public static void StartListenTemperature()
	{
		AndroidUtils.StartListenTemperature();
	}

	public static float GetStartUpTime()
	{
		return AndroidUtils.GetStartUpTime();
	}

	public static int GetCurCpu()
	{
		return AndroidUtils.GetHighestCurFreqCpu();
	}

	public static string GetCpuTimeInState(int processor)
	{
		return AndroidUtils.GetCpuTimeInState(processor);
	}
}
