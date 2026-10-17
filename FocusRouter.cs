using System.Windows.Forms;

internal class FocusRouter : IMessageFilter
{
	private const int WM_LBUTTONDOWN = 513;

	public Form Owner;

	public Control SearchPanel;

	public Control SearchBox;

	public bool PreFilterMessage(ref Message m)
	{
		if (m.Msg != 513)
		{
			return false;
		}
		if (Owner == null || SearchPanel == null || SearchBox == null)
		{
			return false;
		}
		if (!SearchBox.Focused)
		{
			return false;
		}
		if (SearchPanel.RectangleToScreen(SearchPanel.ClientRectangle).Contains(Cursor.Position))
		{
			return false;
		}
		((ContainerControl)Owner).ActiveControl = null;
		return false;
	}
}
