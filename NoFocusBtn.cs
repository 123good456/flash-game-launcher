using System.Windows.Forms;

internal class NoFocusBtn : Button
{
	protected override bool ShowFocusCues => false;

	public NoFocusBtn()
	{
		base.SetStyle((ControlStyles)512, false);
		base.TabStop = false;
	}
}
