using System;
using System.Windows.Forms;

internal class Program
{
	[STAThread]
	private static void Main()
	{
		try
		{
			Application.SetHighDpiMode((HighDpiMode)1);
		}
		catch
		{
		}
		Dpi.Init();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run((Form)(object)new MainForm());
	}
}
