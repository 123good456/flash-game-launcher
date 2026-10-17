using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal class WheelRouter : IMessageFilter
{
	private struct POINT
	{
		public int X;

		public int Y;
	}

	private const int WM_MOUSEWHEEL = 522;

	public GamePanel Target;

	public Control SideArea;

	public Action<int> SideScroll;

	[DllImport("user32.dll")]
	private static extern nint WindowFromPoint(POINT p);

	private static bool Inside(nint h, Control root)
	{
		Control val = Control.FromHandle((IntPtr)h);
		while (val != null && val != root)
		{
			val = val.Parent;
		}
		return val == root;
	}

	public bool PreFilterMessage(ref Message m)
	{
		if (m.Msg != 522)
		{
			return false;
		}
		nint num = WindowFromPoint(new POINT
		{
			X = Cursor.Position.X,
			Y = Cursor.Position.Y
		});
		if (num == IntPtr.Zero)
		{
			return false;
		}
		int num2 = (short)((((IntPtr)(nint)m.WParam).ToInt64() >> 16) & 0xFFFF);
		if (SideArea != null && SideScroll != null && Inside(num, SideArea))
		{
			SideScroll(num2);
			return true;
		}
		if (Target == null || !((Control)Target).IsHandleCreated)
		{
			return false;
		}
		if (m.HWnd == ((Control)Target).Handle)
		{
			return false;
		}
		if (!Inside(num, (Control)(object)Target))
		{
			return false;
		}
		Target.ScrollBy(num2);
		return true;
	}
}
