using System;
using System.Drawing;

internal static class Dpi
{
	public static float S = 1f;

	public static int Px(int v)
	{
		return (int)Math.Round((float)v * S);
	}

	public static float PxF(float v)
	{
		return v * S;
	}

	public static void Init()
	{
		try
		{
			Graphics val = Graphics.FromHwnd((IntPtr)IntPtr.Zero);
			try
			{
				S = val.DpiX / 96f;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
			S = 1f;
		}
		if (S < 1f || S > 4f)
		{
			S = 1f;
		}
	}
}
