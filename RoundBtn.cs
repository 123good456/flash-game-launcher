using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

internal class RoundBtn : NoFocusBtn
{
	public int Radius = 12;

	public Color BorderColor = Color.Empty;

	public Color BorderColorHot = Color.Empty;

	public int BorderWidth = 1;

	public int BorderWidthHot = 2;

	private bool hot;

	public RoundBtn()
	{
		BorderWidth = Dpi.Px(1);
		BorderWidthHot = Dpi.Px(2);
		base.MouseEnter += (object? s, EventArgs e) =>
		{
			hot = true;
			base.Invalidate();
		};
		base.MouseLeave += (object? s, EventArgs e) =>
		{
			hot = false;
			base.Invalidate();
		};
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected Obj, but got Unknown
		base.OnPaint(e);
		Color color = ((hot && BorderColorHot != Color.Empty) ? BorderColorHot : BorderColor);
		if (color == Color.Empty || Radius <= 1)
		{
			return;
		}
		int num = Math.Max(1, hot ? BorderWidthHot : BorderWidth);
		e.Graphics.SmoothingMode = (SmoothingMode)4;
		int radius = Math.Max(2, Radius - 1);
		GraphicsPath val = Gfx.RoundRect(new Rectangle(1, 1, Math.Max(1, base.Width - 2), Math.Max(1, base.Height - 2)), radius);
		try
		{
			Pen val2 = new Pen(color, (float)num);
			try
			{
				e.Graphics.DrawPath(val2, val);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
