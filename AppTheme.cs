using System.Drawing;

internal class AppTheme
{
	public Color Background;

	public Color ToolbarBg;

	public Color SidebarBg;

	public Color GridBg;

	public Color GridAltBg;

	public Color SelectedBg;

	public Color AccentColor;

	// 游戏区文件夹名字的颜色
	public Color FolderText;

	public Color TextColor;

	public Color TextSecColor;

	public Color HeaderBg;

	public Color HeaderText;

	public Color BorderColor;

	public Color StatusBarBg;

	public Color ButtonBg;

	public Color IconBorder;

	public Color SelectedTextColor;

	public Color SelTextColor;

	public Color SearchBg;

	public Color SearchHint;

	public static AppTheme Dark = new AppTheme
	{
		Background = Color.FromArgb(18, 23, 36),
		ToolbarBg = Color.FromArgb(13, 17, 23),
		SidebarBg = Color.FromArgb(18, 23, 36),
		GridBg = Color.FromArgb(18, 23, 36),
		GridAltBg = Color.FromArgb(26, 33, 58),
		SelectedBg = Color.FromArgb(233, 69, 96),
		AccentColor = Color.FromArgb(233, 69, 96),
		FolderText = Color.FromArgb(230, 184, 192),
		TextColor = Color.White,
		TextSecColor = Color.LightGray,
		HeaderBg = Color.Black,
		HeaderText = Color.White,
		BorderColor = Color.FromArgb(40, 40, 60),
		StatusBarBg = Color.FromArgb(18, 23, 36),
		ButtonBg = Color.FromArgb(40, 40, 60),
		IconBorder = Color.FromArgb(255, 140, 140, 140),
		SelectedTextColor = Color.Black,
		SelTextColor = Color.FromArgb(233, 69, 96),
		SearchBg = Color.FromArgb(48, 47, 59),
		SearchHint = Color.FromArgb(139, 148, 158)
	};

	public static AppTheme Light = new AppTheme
	{
		Background = Color.White,
		ToolbarBg = Color.White,
		SidebarBg = Color.White,
		GridBg = Color.White,
		GridAltBg = Color.FromArgb(232, 232, 232),
		SelectedBg = Color.FromArgb(49, 194, 124),
		AccentColor = Color.FromArgb(49, 194, 124),
		FolderText = Color.FromArgb(25, 124, 75),
		TextColor = Color.FromArgb(30, 30, 30),
		TextSecColor = Color.Gray,
		HeaderBg = Color.FromArgb(230, 241, 218),
		HeaderText = Color.FromArgb(30, 30, 30),
		BorderColor = Color.FromArgb(211, 227, 190),
		StatusBarBg = Color.White,
		ButtonBg = Color.FromArgb(240, 242, 245),
		IconBorder = Color.FromArgb(255, 140, 140, 140),
		SelectedTextColor = Color.White,
		SelTextColor = Color.FromArgb(113, 188, 23),
		SearchBg = Color.FromArgb(245, 245, 245),
		SearchHint = Color.FromArgb(153, 153, 153)
	};
}
