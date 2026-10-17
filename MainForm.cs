using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

internal class MainForm : Form
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SHFILEOPSTRUCT
	{
		public nint hwnd;

		public uint wFunc;

		public string pFrom;

		public string pTo;

		public ushort fFlags;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;

		public nint hNameMappings;

		public string lpszProgressTitle;
	}

	private Panel toolbar;

	private Label titleLabel;

	private Label catHeader;

	private ToolTip btnTip;

	private Panel searchPanel;

	private Label searchHint;

	private NoFocusBtn backBtn;

	private string navDir;

	private readonly List<string> navStack = new List<string>();

	private bool lastCatRightHit;

	private int catHoverIdx = -1;

	private BufPanel catBar;

	private Control catSideHost;

	// 分类栏底部留出的空隙，让分类栏不直接顶到底栏
	private Panel catBottomGap;

	private SplitContainer split;

	private string sidebarPath;

	private string viewPath;

	private int catBarDragY = -1;

	private int catBarDragTop;

	private int catBarLastTop = -1;

	private bool catBarShow;

	private readonly Timer catBarTimer = new Timer
	{
		Interval = 80
	};

	private readonly Timer catBarShowTimer = new Timer
	{
		Interval = 1200
	};

	private CatList catList;

	private Font catFont;

	private GamePanel gamePanel;

	private TextBox searchBox;

	private Label statusLabel;

	private RoundBtn viewToggleBtn;

	private RoundBtn refreshBtn;

	private RoundBtn themeToggleBtn;

	private NoFocusBtn minBtn;

	private NoFocusBtn closeBtn;

	private Panel statusBar;

	private Panel rootPanel;

	private ContextMenuStrip catCm;

	private ContextMenuStrip cm;

	private string gamesDir;

	private string iconMapPath;

	private string iconsDir;

	private readonly Dictionary<string, string> nameIcon = new Dictionary<string, string>();

	private string themePath;

	private List<GameItem> allGames = new List<GameItem>();

	private List<CatInfo> allCats = new List<CatInfo>();

	private string currentCat = "全部";

	private bool iconView = true;

	private Dictionary<string, int> catIconIdx = new Dictionary<string, int>();

	private Dictionary<string, int> gameIconIdx = new Dictionary<string, int>();

	private IconMap iconMap = new IconMap();

	private AppTheme theme = AppTheme.Dark;

	private Color[] palette = new Color[12]
	{
		Color.FromArgb(233, 69, 96),
		Color.FromArgb(249, 115, 22),
		Color.FromArgb(34, 197, 94),
		Color.FromArgb(59, 130, 246),
		Color.FromArgb(168, 85, 247),
		Color.FromArgb(236, 72, 153),
		Color.FromArgb(20, 184, 166),
		Color.FromArgb(245, 158, 11),
		Color.FromArgb(139, 92, 246),
		Color.FromArgb(239, 68, 68),
		Color.FromArgb(16, 185, 129),
		Color.FromArgb(99, 102, 241)
	};

	private bool isFull;

	private Rectangle preFullBounds;

	private int lastFullToggleTick;

	private const int WM_NCLBUTTONDOWN = 161;

	private const int HTCAPTION = 2;

	private bool minimizing;

	private bool useRegionFallback;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.Style |= 0xA0000;
			return createParams;
		}
	}

	public MainForm()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected Obj, but got Unknown
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Expected Obj, but got Unknown
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Expected Obj, but got Unknown
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Expected Obj, but got Unknown
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Expected Obj, but got Unknown
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Expected Obj, but got Unknown
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Expected Obj, but got Unknown
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Expected Obj, but got Unknown
		//IL_043e: Expected Obj, but got Unknown
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Expected Obj, but got Unknown
		//IL_04a0: Expected Obj, but got Unknown
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Expected Obj, but got Unknown
		//IL_05fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Expected Obj, but got Unknown
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Expected Obj, but got Unknown
		//IL_0710: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Expected Obj, but got Unknown
		//IL_0727: Unknown result type (might be due to invalid IL or missing references)
		//IL_0731: Expected Obj, but got Unknown
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Expected Obj, but got Unknown
		//IL_0749: Unknown result type (might be due to invalid IL or missing references)
		//IL_0753: Expected Obj, but got Unknown
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e3: Expected Obj, but got Unknown
		//IL_0854: Unknown result type (might be due to invalid IL or missing references)
		//IL_085e: Expected Obj, but got Unknown
		//IL_0966: Unknown result type (might be due to invalid IL or missing references)
		//IL_0970: Expected Obj, but got Unknown
		//IL_097d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0987: Expected Obj, but got Unknown
		//IL_0994: Unknown result type (might be due to invalid IL or missing references)
		//IL_099e: Expected Obj, but got Unknown
		//IL_09e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09eb: Expected Obj, but got Unknown
		//IL_09ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a20: Expected Obj, but got Unknown
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2d: Expected Obj, but got Unknown
		//IL_0a4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a56: Expected Obj, but got Unknown
		//IL_0a75: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7f: Expected Obj, but got Unknown
		//IL_0a8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a96: Expected Obj, but got Unknown
		//IL_0aba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac4: Expected Obj, but got Unknown
		//IL_0ad1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0adb: Expected Obj, but got Unknown
		//IL_0af3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afd: Expected Obj, but got Unknown
		//IL_0b02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b08: Expected Obj, but got Unknown
		//IL_0b0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b13: Expected Obj, but got Unknown
		//IL_0be3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bed: Expected Obj, but got Unknown
		//IL_0c8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c94: Expected Obj, but got Unknown
		//IL_0ccd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd7: Expected Obj, but got Unknown
		//IL_0d6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d77: Expected Obj, but got Unknown
		//IL_0d7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d83: Expected Obj, but got Unknown
		//IL_0d88: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8f: Expected Obj, but got Unknown
		//IL_0d95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d9f: Expected Obj, but got Unknown
		//IL_0da5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0daf: Expected Obj, but got Unknown
		//IL_0db4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dbb: Expected Obj, but got Unknown
		//IL_0e23: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2d: Expected Obj, but got Unknown
		//IL_0e2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e34: Expected Obj, but got Unknown
		//IL_0ebd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed6: Expected Obj, but got Unknown
		//IL_0ed7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ee3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef8: Expected Obj, but got Unknown
		//IL_0ef8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f00: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f20: Expected Obj, but got Unknown
		//IL_0f37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f48: Expected Obj, but got Unknown
		base.Text = "Flash小游戏启动器";
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
		base.Width = Math.Min(Dpi.Px(1000), workingArea.Width - Dpi.Px(40));
		base.Height = Math.Min(Dpi.Px(680), workingArea.Height - Dpi.Px(40));
		base.StartPosition = (FormStartPosition)1;
		base.MinimumSize = new Size(Dpi.Px(750), Dpi.Px(480));
		base.Font = new Font("Microsoft YaHei", 9f);
		base.FormBorderStyle = (FormBorderStyle)0;
		base.MinimizeBox = true;
		base.ShowInTaskbar = true;
		gamesDir = Path.Combine(AppContext.BaseDirectory, "games");
		iconMapPath = Path.Combine(AppContext.BaseDirectory, "icon_map.json");
		try
		{
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}
		catch
		{
		}
		iconsDir = Path.Combine(AppContext.BaseDirectory, "icons");
		themePath = Path.Combine(AppContext.BaseDirectory, "theme.txt");
		sidebarPath = Path.Combine(AppContext.BaseDirectory, "sidebar.txt");
		viewPath = Path.Combine(AppContext.BaseDirectory, "view.txt");
		LoadTheme();
		LoadIconMap();
		toolbar = new Panel
		{
			Dock = (DockStyle)1,
			Height = 48
		};
		titleLabel = new Label
		{
			Text = "水星海葵",
			Font = new Font("Microsoft YaHei", 15f, (FontStyle)1),
			Location = new Point(15, 12),
			AutoSize = true
		};
		searchPanel = new Panel
		{
			Location = new Point(140, 12),
			Size = new Size(200, 27),
			BackColor = theme.SearchBg
		};
		((Control)searchPanel).Paint += (object? s, PaintEventArgs e) =>
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected Obj, but got Unknown
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = (SmoothingMode)4;
			int num = ((Control)searchPanel).Width - Dpi.Px(18);
			int num2 = ((Control)searchPanel).Height / 2;
			Pen val8 = new Pen(theme.SearchHint, Dpi.PxF(2.2f));
			try
			{
				val8.StartCap = (LineCap)2;
				val8.EndCap = (LineCap)2;
				graphics.DrawEllipse(val8, num - Dpi.Px(5), num2 - Dpi.Px(5), Dpi.Px(10), Dpi.Px(10));
				graphics.DrawLine(val8, num + Dpi.Px(3), num2 + Dpi.Px(3), num + Dpi.Px(7), num2 + Dpi.Px(7));
			}
			finally
			{
				((IDisposable)val8)?.Dispose();
			}
		};
		searchHint = new Label
		{
			Text = "搜索游戏",
			AutoSize = true,
			Location = new Point(12, 4),
			BackColor = theme.SearchBg,
			ForeColor = theme.SearchHint,
			Font = new Font("Microsoft YaHei", 11f)
		};
		searchBox = new TextBox
		{
			BorderStyle = (BorderStyle)0,
			Location = new Point(12, 3),
			Width = 156,
			TabStop = false,
			BackColor = theme.SearchBg,
			ForeColor = theme.SearchHint,
			Font = new Font("Microsoft YaHei", 11f)
		};
		((Control)searchHint).Click += (object? s, EventArgs e) =>
		{
			((Control)searchBox).Focus();
		};
		((Control)searchBox).TextChanged += (object? s, EventArgs e) =>
		{
			((Control)searchHint).Visible = ((Control)searchBox).Text.Length == 0 && !((Control)searchBox).Focused;
			RefreshGames();
		};
		((Control)searchBox).GotFocus += (object? s, EventArgs e) =>
		{
			((Control)searchHint).Visible = false;
		};
		((Control)searchBox).LostFocus += (object? s, EventArgs e) =>
		{
			((Control)searchHint).Visible = ((Control)searchBox).Text.Length == 0;
		};
		((Control)searchPanel).Controls.Add((Control)(object)searchHint);
		((Control)searchPanel).Controls.Add((Control)(object)searchBox);
		((Control)searchHint).BringToFront();
		SetPillRegion((Control)(object)searchPanel);
		RoundBtn roundBtn = new RoundBtn();
		((Control)roundBtn).Text = "";
		((Control)roundBtn).Location = new Point(390, 14);
		((Control)roundBtn).Size = new Size(34, 24);
		((Control)roundBtn).TabStop = false;
		((ButtonBase)roundBtn).FlatStyle = (FlatStyle)0;
		((Control)roundBtn).Font = new Font("Microsoft YaHei", 9f);
		themeToggleBtn = roundBtn;
		((ButtonBase)themeToggleBtn).FlatAppearance.BorderSize = 0;
		((Control)themeToggleBtn).Click += (object? s, EventArgs e) =>
		{
			ToggleTheme();
		};
		RoundBtn roundBtn2 = new RoundBtn();
		((Control)roundBtn2).Text = "";
		((Control)roundBtn2).Location = new Point(460, 14);
		((Control)roundBtn2).Size = new Size(34, 24);
		((Control)roundBtn2).TabStop = false;
		((ButtonBase)roundBtn2).FlatStyle = (FlatStyle)0;
		((Control)roundBtn2).Font = new Font("Microsoft YaHei", 9f);
		viewToggleBtn = roundBtn2;
		((ButtonBase)viewToggleBtn).FlatAppearance.BorderSize = 0;
		((Control)viewToggleBtn).Click += (object? s, EventArgs e) =>
		{
			ToggleView();
		};
		RoundBtn roundBtn3 = new RoundBtn();
		((Control)roundBtn3).Text = "";
		((Control)roundBtn3).Location = new Point(555, 14);
		((Control)roundBtn3).Size = new Size(34, 24);
		((Control)roundBtn3).TabStop = false;
		((ButtonBase)roundBtn3).FlatStyle = (FlatStyle)0;
		((Control)roundBtn3).Font = new Font("Microsoft YaHei", 9f);
		refreshBtn = roundBtn3;
		((ButtonBase)refreshBtn).FlatAppearance.BorderSize = 0;
		((Control)refreshBtn).Click += (object? s, EventArgs e) =>
		{
			ScanAll();
			BuildIcons();
			RefreshCats();
			RefreshGames();
		};
		RoundButton((Button)(object)themeToggleBtn, 5);
		RoundButton((Button)(object)viewToggleBtn, 5);
		RoundButton((Button)(object)refreshBtn, 5);
		((Control)themeToggleBtn).Text = "";
		((Control)viewToggleBtn).Text = "";
		((Control)refreshBtn).Text = "";
		((Control)themeToggleBtn).Paint += (object? s, PaintEventArgs e) =>
		{
			DrawBtnIcon(e.Graphics, (Control)(object)themeToggleBtn, "sunmoon");
		};
		((Control)viewToggleBtn).Paint += (object? s, PaintEventArgs e) =>
		{
			DrawBtnIcon(e.Graphics, (Control)(object)viewToggleBtn, "view");
		};
		((Control)refreshBtn).Paint += (object? s, PaintEventArgs e) =>
		{
			DrawBtnIcon(e.Graphics, (Control)(object)refreshBtn, "refresh");
		};
		btnTip = new ToolTip();
		btnTip.SetToolTip((Control)(object)themeToggleBtn, "切换浅色 / 深色主题");
		btnTip.SetToolTip((Control)(object)viewToggleBtn, "切换图标 / 列表视图");
		btnTip.SetToolTip((Control)(object)refreshBtn, "刷新");
		NoFocusBtn noFocusBtn = new NoFocusBtn();
		((Control)noFocusBtn).Text = "—";
		((Control)noFocusBtn).Location = new Point(600, 12);
		((Control)noFocusBtn).Size = new Size(38, 28);
		((ButtonBase)noFocusBtn).FlatStyle = (FlatStyle)0;
		((Control)noFocusBtn).Font = new Font("Microsoft YaHei", 9f);
		minBtn = noFocusBtn;
		((ButtonBase)minBtn).FlatAppearance.BorderSize = 0;
		((Control)minBtn).Click += (object? s, EventArgs e) =>
		{
			base.WindowState = (FormWindowState)1;
		};
		NoFocusBtn noFocusBtn2 = new NoFocusBtn();
		((Control)noFocusBtn2).Text = "✕";
		((Control)noFocusBtn2).Location = new Point(640, 12);
		((Control)noFocusBtn2).Size = new Size(38, 28);
		((ButtonBase)noFocusBtn2).FlatStyle = (FlatStyle)0;
		((Control)noFocusBtn2).Font = new Font("Microsoft YaHei", 10f);
		closeBtn = noFocusBtn2;
		((ButtonBase)closeBtn).FlatAppearance.BorderSize = 0;
		((Control)closeBtn).Click += (object? s, EventArgs e) =>
		{
			base.Close();
		};
		((Control)minBtn).MouseEnter += (object? s, EventArgs e) =>
		{
			((Control)minBtn).BackColor = theme.GridAltBg;
		};
		((Control)minBtn).MouseLeave += (object? s, EventArgs e) =>
		{
			((Control)minBtn).BackColor = theme.ToolbarBg;
		};
		((Control)closeBtn).MouseEnter += (object? s, EventArgs e) =>
		{
			((Control)closeBtn).BackColor = Color.FromArgb(232, 17, 35);
			((Control)closeBtn).ForeColor = Color.White;
		};
		((Control)closeBtn).MouseLeave += (object? s, EventArgs e) =>
		{
			((Control)closeBtn).BackColor = theme.ToolbarBg;
			((Control)closeBtn).ForeColor = theme.TextColor;
		};
		((Control)toolbar).Controls.AddRange(new Control[7]
		{
			(Control)titleLabel,
			(Control)searchPanel,
			(Control)themeToggleBtn,
			(Control)viewToggleBtn,
			(Control)refreshBtn,
			(Control)minBtn,
			(Control)closeBtn
		});
		((Control)toolbar).Resize += (object? s, EventArgs e) =>
		{
			LayoutToolbar();
		};
		LayoutToolbar();
		((Control)toolbar).MouseDown += (object? s, MouseEventArgs e) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if ((int)e.Button == 1048576)
			{
				if (e.Clicks == 2)
				{
					ToggleFullscreen();
				}
				else if (!isFull)
				{
					StartDrag();
				}
			}
		};
		((Control)titleLabel).MouseDown += (object? s, MouseEventArgs e) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if ((int)e.Button == 1048576)
			{
				if (e.Clicks == 2)
				{
					ToggleFullscreen();
				}
				else if (!isFull)
				{
					StartDrag();
				}
			}
		};
		((Control)searchPanel).MouseDown += (object? s, MouseEventArgs e) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if ((int)e.Button == 1048576 && !isFull)
			{
				StartDrag();
			}
		};
		WideSplit wideSplit = new WideSplit();
		((SplitContainer)wideSplit).Dock = (DockStyle)5;
		((SplitContainer)wideSplit).SplitterDistance = Dpi.Px(170);
		((SplitContainer)wideSplit).SplitterWidth = Dpi.Px(9);
		((SplitContainer)wideSplit).FixedPanel = (FixedPanel)1;
		split = (SplitContainer)(object)wideSplit;
		split.SplitterMoved += (object? s, SplitterEventArgs e) =>
		{
			SaveSidebar();
			LayoutCatBar();
		};
		catHeader = new Label
		{
			Text = "  分类",
			Dock = (DockStyle)1,
			Height = 32,
			Font = new Font("Microsoft YaHei", 12f),
			TextAlign = (ContentAlignment)16
		};
		catList = new CatList();
		((Control)catList).Dock = (DockStyle)5;
		((Control)catList).Font = new Font("Microsoft YaHei", 13f);
		catList.ItemHeight = 49;
		catFont = new Font("Microsoft YaHei", 13f);
		this.catList.DrawItem += (object? s, DrawItemEventArgs e) =>
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Invalid comparison between Unknown and I4
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected Obj, but got Unknown
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected Obj, but got Unknown
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Expected Obj, but got Unknown
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Expected Obj, but got Unknown
			if (e.Index >= 0)
			{
				e.Graphics.SmoothingMode = (SmoothingMode)4;
				CatInfo catInfo = this.catList.Items[e.Index] as CatInfo;
				bool flag = (((int)e.State) & 1) == 1;
				bool flag2 = e.Index == catHoverIdx && !flag;
				SolidBrush val8 = new SolidBrush(theme.SidebarBg);
				try
				{
					e.Graphics.FillRectangle((Brush)(object)val8, e.Bounds);
				}
				finally
				{
					((IDisposable)val8)?.Dispose();
				}
				if (flag | flag2)
				{
					Rectangle r = new Rectangle(e.Bounds.X + Dpi.Px(4), e.Bounds.Y + Dpi.Px(7), Math.Max(Dpi.Px(4), e.Bounds.Width - Dpi.Px(8)), Math.Max(Dpi.Px(4), e.Bounds.Height - Dpi.Px(15)));
					SolidBrush val9 = new SolidBrush(flag ? theme.SelectedBg : ((theme == AppTheme.Dark) ? Color.FromArgb(110, 44, 62) : Color.FromArgb(167, 224, 196)));
					try
					{
						GraphicsPath val10 = Gfx.RoundRect(r, Dpi.Px(10));
						try
						{
							e.Graphics.FillPath((Brush)(object)val9, val10);
						}
						finally
						{
							((IDisposable)val10)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val9)?.Dispose();
					}
				}
				if (catInfo != null)
				{
					string text = catInfo.ToString();
					SolidBrush val11 = new SolidBrush(flag ? Color.White : ((theme == AppTheme.Dark) ? Color.FromArgb(248, 249, 251) : Color.Black));
					try
					{
						StringFormat val12 = new StringFormat
						{
							Alignment = (StringAlignment)0,
							LineAlignment = (StringAlignment)1,
							Trimming = (StringTrimming)1,
							FormatFlags = (StringFormatFlags)4096
						};
						Rectangle rectangle = new Rectangle(e.Bounds.X + Dpi.Px(20), e.Bounds.Y, Math.Max(Dpi.Px(4), e.Bounds.Width - Dpi.Px(40)), e.Bounds.Height);
						e.Graphics.DrawString(text, catFont, (Brush)(object)val11, (RectangleF)rectangle, val12);
					}
					finally
					{
						((IDisposable)val11)?.Dispose();
					}
				}
			}
		};
		this.catList.SelectedIndexChanged += (object? s, EventArgs e) =>
		{
			if (this.catList.SelectedItem is CatInfo catInfo)
			{
				currentCat = catInfo.Name;
				navDir = null;
				navStack.Clear();
				RefreshGames();
			}
		};
		((Control)this.catList).MouseDown += (object? s, MouseEventArgs e) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if ((int)e.Button == 2097152)
			{
				int num = this.catList.IndexFromPoint(e.Location);
				lastCatRightHit = num >= 0;
				if (num >= 0)
				{
					this.catList.SelectedIndex = num;
				}
			}
		};
		((Control)this.catList).MouseMove += (object? s, MouseEventArgs e) =>
		{
			int num = this.catList.IndexFromPoint(e.Location);
			if (num != catHoverIdx)
			{
				int idx = catHoverIdx;
				catHoverIdx = num;
				InvalidateCatItem(idx);
				InvalidateCatItem(num);
			}
		};
		((Control)this.catList).MouseLeave += (object? s, EventArgs e) =>
		{
			if (catHoverIdx != -1)
			{
				int idx = catHoverIdx;
				catHoverIdx = -1;
				InvalidateCatItem(idx);
			}
		};
		catCm = new ContextMenuStrip();
		ToolStripMenuItem val = new ToolStripMenuItem("更换分类图标");
		ToolStripMenuItem val2 = new ToolStripMenuItem("恢复默认图标");
		((ToolStripItem)val).Click += (object? s, EventArgs e) =>
		{
			ChangeCategoryIcon();
		};
		((ToolStripItem)val2).Click += (object? s, EventArgs e) =>
		{
			ResetCategoryIcon();
		};
		((ToolStrip)catCm).Items.AddRange(new ToolStripItem[2]
		{
			(ToolStripItem)val,
			(ToolStripItem)val2
		});
		((Control)this.catList).ContextMenuStrip = catCm;
		((ToolStripDropDown)catCm).Opening += (object? s, CancelEventArgs e) =>
		{
			if (!lastCatRightHit)
			{
				e.Cancel = true;
			}
		};
		((Control)split.Panel1).Controls.Add((Control)(object)this.catList);
		((Control)split.Panel1).Controls.Add((Control)(object)catHeader);
		InitCatBar((Control)(object)split.Panel1);
		catBottomGap = new Panel
		{
			Dock = (DockStyle)2,
			Height = Dpi.Px(75),
			TabStop = false
		};
		((Control)split.Panel1).Controls.Add((Control)(object)catBottomGap);
		GamePanel gamePanel = new GamePanel();
		((Control)gamePanel).Dock = (DockStyle)5;
		((Panel)gamePanel).BorderStyle = (BorderStyle)0;
		((Control)gamePanel).Font = new Font("Microsoft YaHei", 10f);
		this.gamePanel = gamePanel;
		this.gamePanel.ItemDoubleClicked += (int idx) =>
		{
			GameItem item = this.gamePanel.GetItem(idx);
			if (item != null)
			{
				if (item.IsFolder)
				{
					EnterFolder(item);
				}
				else
				{
					LaunchGame(item.Path);
				}
			}
		};
		NoFocusBtn noFocusBtn3 = new NoFocusBtn();
		((Control)noFocusBtn3).Text = "";
		((Control)noFocusBtn3).Location = new Point(0, 15);
		((Control)noFocusBtn3).Size = new Size(32, 32);
		((ButtonBase)noFocusBtn3).FlatStyle = (FlatStyle)0;
		((Control)noFocusBtn3).Visible = false;
		((Control)noFocusBtn3).Cursor = Cursors.Hand;
		backBtn = noFocusBtn3;
		((ButtonBase)backBtn).FlatAppearance.BorderSize = 0;
		((Control)backBtn).Click += (object? s, EventArgs e) =>
		{
			GoBack();
		};
		((Control)backBtn).Paint += (object? s, PaintEventArgs e) =>
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected Obj, but got Unknown
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = (SmoothingMode)4;
			int num = ((Control)backBtn).Width / 2;
			int num2 = ((Control)backBtn).Height / 2;
			float num3 = (float)((Control)backBtn).Width / 32f;
			Pen val8 = new Pen(((Control)backBtn).ForeColor, 2f * num3);
			try
			{
				val8.StartCap = (LineCap)2;
				val8.EndCap = (LineCap)2;
				val8.LineJoin = (LineJoin)2;
				graphics.DrawLine(val8, (float)num - 6f * num3, (float)num2, (float)num + 6f * num3, (float)num2);
				graphics.DrawLine(val8, (float)num - 6f * num3, (float)num2, (float)num - num3, (float)num2 - 5f * num3);
				graphics.DrawLine(val8, (float)num - 6f * num3, (float)num2, (float)num - num3, (float)num2 + 5f * num3);
			}
			finally
			{
				((IDisposable)val8)?.Dispose();
			}
		};
		GraphicsPath val3 = Gfx.RoundRect(new Rectangle(0, 0, ((Control)backBtn).Width, ((Control)backBtn).Height), ((Control)backBtn).Width / 2);
		try
		{
			((Control)backBtn).Region = new Region(val3);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		((Control)backBtn).BackColor = theme.ButtonBg;
		((Control)backBtn).ForeColor = theme.TextColor;
		((Control)backBtn).MouseEnter += (object? s, EventArgs e) =>
		{
			((Control)backBtn).BackColor = theme.GridAltBg;
		};
		((Control)backBtn).MouseLeave += (object? s, EventArgs e) =>
		{
			((Control)backBtn).BackColor = theme.ButtonBg;
		};
		((Control)this.gamePanel).Controls.Add((Control)(object)backBtn);
		((Control)this.gamePanel).Resize += (object? s, EventArgs e) =>
		{
			PlaceBackBtn();
		};
		cm = new ContextMenuStrip();
		ToolStripMenuItem val4 = new ToolStripMenuItem("打开");
		ToolStripMenuItem val5 = new ToolStripMenuItem("在文件夹中显示");
		ToolStripMenuItem miGameIcon = new ToolStripMenuItem("更换图标");
		ToolStripMenuItem miGameReset = new ToolStripMenuItem("恢复默认图标");
		ToolStripMenuItem val6 = new ToolStripMenuItem("删除文件");
		((ToolStripItem)val4).Click += (object? s, EventArgs e) =>
		{
			GameItem selectedItem = this.gamePanel.GetSelectedItem();
			if (selectedItem != null)
			{
				if (selectedItem.IsFolder)
				{
					EnterFolder(selectedItem);
				}
				else
				{
					LaunchGame(selectedItem.Path);
				}
			}
		};
		((ToolStripItem)val5).Click += (object? s, EventArgs e) =>
		{
			GameItem selectedItem = this.gamePanel.GetSelectedItem();
			if (selectedItem != null)
			{
				ShowInFolder(selectedItem.Path);
			}
		};
		((ToolStripItem)miGameIcon).Click += (object? s, EventArgs e) =>
		{
			ChangeGameIcon();
		};
		((ToolStripItem)miGameReset).Click += (object? s, EventArgs e) =>
		{
			ResetGameIcon();
		};
		((ToolStripItem)val6).Click += (object? s, EventArgs e) =>
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Invalid comparison between Unknown and I4
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			GameItem selectedItem = this.gamePanel.GetSelectedItem();
			if (selectedItem != null && (int)MessageBox.Show("确定删除「" + selectedItem.Name + "」？\n" + selectedItem.Path + "\n\n会移到回收站，需要时可以还原。", "确认删除", (MessageBoxButtons)4, (MessageBoxIcon)48) == 6)
			{
				if (!MoveToRecycleBin(selectedItem.Path))
				{
					MessageBox.Show("移到回收站失败。", "错误");
				}
				ScanAll();
				BuildIcons();
				RefreshCats();
				RefreshGames();
			}
		};
		ToolStripSeparator sepIconGroup = new ToolStripSeparator();
		ToolStripSeparator val7 = new ToolStripSeparator();
		((ToolStrip)cm).Items.AddRange(new ToolStripItem[7]
		{
			(ToolStripItem)val4,
			(ToolStripItem)val5,
			(ToolStripItem)sepIconGroup,
			(ToolStripItem)miGameIcon,
			(ToolStripItem)miGameReset,
			(ToolStripItem)val7,
			(ToolStripItem)val6
		});
		((Control)this.gamePanel).ContextMenuStrip = cm;
		((ToolStripDropDown)cm).Opening += (object? s, CancelEventArgs e) =>
		{
			if (!this.gamePanel.LastRightHit)
			{
				e.Cancel = true;
			}
			else
			{
				bool visible = this.gamePanel.IconView;
				((ToolStripItem)sepIconGroup).Visible = visible;
				((ToolStripItem)miGameIcon).Visible = visible;
				((ToolStripItem)miGameReset).Visible = visible;
			}
		};
		((Control)split.Panel2).Controls.Add((Control)(object)this.gamePanel);
		statusBar = new Panel
		{
			Dock = (DockStyle)2,
			Height = 26
		};
		statusLabel = new Label
		{
			Dock = (DockStyle)5,
			Font = new Font("Microsoft YaHei", 9f),
			TextAlign = (ContentAlignment)16,
			Padding = new Padding(15, 0, 0, 0),
			Text = ""
		};
		((Control)statusBar).Controls.Add((Control)(object)statusLabel);
		rootPanel = new Panel
		{
			Dock = (DockStyle)5
		};
		((Control)rootPanel).Controls.Add((Control)(object)split);
		((Control)rootPanel).Controls.Add((Control)(object)statusBar);
		((Control)rootPanel).Controls.Add((Control)(object)toolbar);
		base.Controls.Add((Control)(object)rootPanel);
		Application.AddMessageFilter((IMessageFilter)(object)new WheelRouter
		{
			Target = this.gamePanel,
			SideArea = (Control)(object)split.Panel1,
			SideScroll = (int d) =>
			{
				SetCatTop(this.catList.TopIndex - d / 120);
			}
		});
		Application.AddMessageFilter((IMessageFilter)(object)new FocusRouter
		{
			Owner = (Form)(object)this,
			SearchPanel = (Control)(object)searchPanel,
			SearchBox = (Control)(object)searchBox
		});
		ScaleLayout();
		ApplyTheme();
		LoadView();
		ScanAll();
		BuildIcons();
		RefreshCats();
		RefreshGames();
	}

	private void LoadTheme()
	{
		try
		{
			if (File.Exists(themePath))
			{
				string text = File.ReadAllText(themePath).Trim();
				theme = ((text == "light") ? AppTheme.Light : AppTheme.Dark);
			}
		}
		catch
		{
		}
	}

	private void SaveTheme()
	{
		try
		{
			File.WriteAllText(themePath, (theme == AppTheme.Dark) ? "dark" : "light");
		}
		catch
		{
		}
	}

	private void ApplyTheme()
	{
		base.BackColor = theme.Background;
		base.ForeColor = theme.TextColor;
		((Control)rootPanel).BackColor = theme.Background;
		((Control)toolbar).BackColor = theme.ToolbarBg;
		((Control)titleLabel).ForeColor = theme.AccentColor;
		((Control)searchPanel).BackColor = theme.SearchBg;
		((Control)searchBox).BackColor = theme.SearchBg;
		((Control)searchBox).ForeColor = theme.SearchHint;
		((Control)searchHint).BackColor = theme.SearchBg;
		((Control)searchHint).ForeColor = theme.SearchHint;
		((Control)searchPanel).Invalidate();
		Color color = ((theme == AppTheme.Dark) ? Color.FromArgb(18, 23, 36) : theme.ButtonBg);
		RoundBtn[] array = new RoundBtn[3] { themeToggleBtn, viewToggleBtn, refreshBtn };
		foreach (RoundBtn obj in array)
		{
			((Control)obj).BackColor = color;
			((Control)obj).ForeColor = theme.TextColor;
			obj.BorderColor = Color.Empty;
			obj.BorderColorHot = Color.Empty;
			((ButtonBase)obj).FlatAppearance.MouseOverBackColor = theme.GridAltBg;
			((ButtonBase)obj).FlatAppearance.MouseDownBackColor = Mix(color, theme.TextColor, 0.24f);
			((Control)obj).Invalidate();
		}
		((Control)backBtn).BackColor = theme.ButtonBg;
		((Control)backBtn).ForeColor = theme.TextColor;
		((Control)backBtn).Invalidate();
		((Control)minBtn).BackColor = theme.ToolbarBg;
		((Control)minBtn).ForeColor = theme.TextColor;
		((Control)closeBtn).BackColor = theme.ToolbarBg;
		((Control)closeBtn).ForeColor = theme.TextColor;
		((Control)catHeader).BackColor = theme.SidebarBg;
		((Control)catHeader).ForeColor = theme.TextSecColor;
		((Control)catList).BackColor = theme.SidebarBg;
		((Control)catList).ForeColor = theme.TextColor;
		((Control)statusBar).BackColor = theme.StatusBarBg;
		((Control)statusLabel).BackColor = theme.StatusBarBg;
		((Control)statusLabel).ForeColor = theme.TextSecColor;
		((ToolStrip)catCm).BackColor = theme.SidebarBg;
		((ToolStrip)catCm).ForeColor = theme.TextColor;
		((ToolStrip)cm).BackColor = theme.SidebarBg;
		((ToolStrip)cm).ForeColor = theme.TextColor;
		gamePanel.SetTheme(theme);
		// 图标与主题无关（分类图标底色由分类名固定算出），切换主题不必重建图标；
		// 重建一次要重新加载全部自定义图标，文件多时会明显卡顿。
		RefreshGames();
		ApplySideScrollbarTheme();
		if (catBar != null)
		{
			((Control)catBar).BackColor = theme.SidebarBg;
			((Control)catBar).Invalidate();
		}
		if (split != null)
		{
			((Control)split).BackColor = theme.SidebarBg;
			((Control)split.Panel1).BackColor = theme.Background;
			if (catBottomGap != null)
			{
				((Control)catBottomGap).BackColor = theme.Background;
			}
		}
	}

	private void ToggleTheme()
	{
		theme = ((theme == AppTheme.Dark) ? AppTheme.Light : AppTheme.Dark);
		SaveTheme();
		ApplyTheme();
	}

	private void LoadIconMap()
	{
		try
		{
			if (File.Exists(iconMapPath))
			{
				string json = File.ReadAllText(iconMapPath);
				iconMap = JsonSerializer.Deserialize<IconMap>(json) ?? new IconMap();
			}
		}
		catch
		{
			iconMap = new IconMap();
		}
	}

	private void SaveIconMap()
	{
		try
		{
			File.WriteAllText(iconMapPath, JsonSerializer.Serialize(iconMap, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		}
		catch
		{
		}
	}

	private Bitmap LoadCustomIcon(string path)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected Obj, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected Obj, but got Unknown
		try
		{
			if (!File.Exists(path))
			{
				return null;
			}
			Image val2;
			if (Path.GetExtension(path).ToLower() == ".ico")
			{
				Icon val = new Icon(path, Dpi.Px(72), Dpi.Px(72));
				try
				{
					val2 = (Image)(object)val.ToBitmap();
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else
			{
				val2 = Image.FromFile(path);
			}
			int num = Dpi.Px(72);
			Bitmap val3 = new Bitmap(num, num, (PixelFormat)2498570);
			Graphics val4 = Graphics.FromImage((Image)(object)val3);
			try
			{
				val4.SmoothingMode = (SmoothingMode)4;
				val4.InterpolationMode = (InterpolationMode)7;
				val4.Clear(Color.Transparent);
				Rectangle rectangle = new Rectangle(Dpi.Px(2), Dpi.Px(2), Dpi.Px(68), Dpi.Px(68));
				int num2 = Dpi.Px(10);
				GraphicsPath val5 = new GraphicsPath();
				val5.AddArc(rectangle.X, rectangle.Y, num2 * 2, num2 * 2, 180f, 90f);
				val5.AddArc(rectangle.Right - num2 * 2, rectangle.Y, num2 * 2, num2 * 2, 270f, 90f);
				val5.AddArc(rectangle.Right - num2 * 2, rectangle.Bottom - num2 * 2, num2 * 2, num2 * 2, 0f, 90f);
				val5.AddArc(rectangle.X, rectangle.Bottom - num2 * 2, num2 * 2, num2 * 2, 90f, 90f);
				val5.CloseFigure();
				val4.SetClip(val5);
				int num3 = Math.Min(val2.Width, val2.Height);
				int x = (val2.Width - num3) / 2;
				int y = (val2.Height - num3) / 2;
				val4.DrawImage(val2, rectangle, new Rectangle(x, y, num3, num3), (GraphicsUnit)2);
				val4.ResetClip();
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			val2.Dispose();
			return val3;
		}
		catch
		{
			return null;
		}
	}

	private void ChangeCategoryIcon()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		if (!(catList.SelectedItem is CatInfo { Name: not "全部" } catInfo))
		{
			return;
		}
		OpenFileDialog val = new OpenFileDialog();
		try
		{
			((FileDialog)val).Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.ico|所有文件|*.*";
			((FileDialog)val).Title = "选择分类「" + catInfo.Name + "」的图标";
			if ((int)((CommonDialog)val).ShowDialog() == 1)
			{
				iconMap.CategoryIcons[catInfo.Name] = ((FileDialog)val).FileName;
				SaveIconMap();
				BuildIcons();
				RefreshGames();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ResetCategoryIcon()
	{
		if (catList.SelectedItem is CatInfo { Name: not "全部" } catInfo && iconMap.CategoryIcons.ContainsKey(catInfo.Name))
		{
			iconMap.CategoryIcons.Remove(catInfo.Name);
			SaveIconMap();
			BuildIcons();
			RefreshGames();
		}
	}

	private void ChangeGameIcon()
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		GameItem selectedItem = gamePanel.GetSelectedItem();
		if (selectedItem == null)
		{
			return;
		}
		OpenFileDialog val = new OpenFileDialog();
		try
		{
			((FileDialog)val).Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.ico;*.gif|所有文件|*.*";
			((FileDialog)val).Title = "选择「" + selectedItem.Name + "」的图标";
			if ((int)((CommonDialog)val).ShowDialog() != 1)
			{
				return;
			}
			try
			{
				if (!Directory.Exists(iconsDir))
				{
					Directory.CreateDirectory(iconsDir);
				}
				string text = Path.GetExtension(((FileDialog)val).FileName);
				if (string.IsNullOrEmpty(text))
				{
					text = ".png";
				}
				string text2 = Path.Combine(iconsDir, selectedItem.Name + text);
				RemoveIconFiles(selectedItem.Name, text2);
				if (!string.Equals(Path.GetFullPath(((FileDialog)val).FileName), Path.GetFullPath(text2), StringComparison.OrdinalIgnoreCase))
				{
					File.Copy(((FileDialog)val).FileName, text2, overwrite: true);
				}
				if (iconMap.GameIcons.ContainsKey(selectedItem.Path))
				{
					iconMap.GameIcons.Remove(selectedItem.Path);
					SaveIconMap();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("设置图标失败: " + ex.Message, "错误");
			}
			BuildIcons();
			RefreshGames();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ResetGameIcon()
	{
		GameItem selectedItem = gamePanel.GetSelectedItem();
		if (selectedItem == null)
		{
			return;
		}
		bool flag = false;
		if (iconMap.GameIcons.ContainsKey(selectedItem.Path))
		{
			iconMap.GameIcons.Remove(selectedItem.Path);
			SaveIconMap();
			flag = true;
		}
		try
		{
			if (Directory.Exists(iconsDir))
			{
				string[] files = Directory.GetFiles(iconsDir);
				foreach (string path in files)
				{
					if (string.Equals(Path.GetFileNameWithoutExtension(path), selectedItem.Name, StringComparison.OrdinalIgnoreCase))
					{
						File.Delete(path);
						flag = true;
					}
				}
			}
		}
		catch
		{
		}
		if (flag)
		{
			BuildIcons();
			RefreshGames();
		}
	}

	private void ScanIconDir()
	{
		nameIcon.Clear();
		try
		{
			if (!Directory.Exists(iconsDir))
			{
				Directory.CreateDirectory(iconsDir);
			}
			string[] files = Directory.GetFiles(iconsDir);
			foreach (string text in files)
			{
				string text2 = Path.GetExtension(text).ToLower();
				if (!(text2 != ".png") || !(text2 != ".jpg") || !(text2 != ".jpeg") || !(text2 != ".bmp") || !(text2 != ".ico") || !(text2 != ".gif"))
				{
					string key = Path.GetFileNameWithoutExtension(text).ToLower();
					if (!nameIcon.ContainsKey(key))
					{
						nameIcon[key] = text;
					}
				}
			}
		}
		catch
		{
		}
	}

	private string IconFileFor(string gameName)
	{
		if (string.IsNullOrEmpty(gameName))
		{
			return null;
		}
		if (!nameIcon.TryGetValue(gameName.ToLower(), out var value))
		{
			return null;
		}
		return value;
	}

	private void RemoveIconFiles(string gameName, string keep)
	{
		try
		{
			if (!Directory.Exists(iconsDir))
			{
				return;
			}
			string[] files = Directory.GetFiles(iconsDir);
			foreach (string path in files)
			{
				if (!string.Equals(Path.GetFullPath(path), Path.GetFullPath(keep), StringComparison.OrdinalIgnoreCase) && string.Equals(Path.GetFileNameWithoutExtension(path), gameName, StringComparison.OrdinalIgnoreCase))
				{
					File.Delete(path);
				}
			}
		}
		catch
		{
		}
	}

	private Color GetCatColor(string cat)
	{
		int num = 0;
		foreach (char c in cat)
		{
			num = num * 31 + c;
		}
		return palette[Math.Abs(num) % palette.Length];
	}

	private void BuildIcons()
	{
		ScanIconDir();
		List<Bitmap> list = new List<Bitmap>();
		catIconIdx.Clear();
		gameIconIdx.Clear();
		foreach (CatInfo allCat in allCats)
		{
			Bitmap val = null;
			if (iconMap.CategoryIcons.ContainsKey(allCat.Name))
			{
				val = LoadCustomIcon(iconMap.CategoryIcons[allCat.Name]);
			}
			if (val == null)
			{
				val = MakeIcon(allCat.Name, GetCatColor(allCat.Name));
			}
			list.Add(val);
			catIconIdx[allCat.Name] = list.Count - 1;
		}
		if (!catIconIdx.ContainsKey("未分类"))
		{
			Bitmap val2 = null;
			if (iconMap.CategoryIcons.ContainsKey("未分类"))
			{
				val2 = LoadCustomIcon(iconMap.CategoryIcons["未分类"]);
			}
			if (val2 == null)
			{
				val2 = MakeIcon("?", Color.FromArgb(107, 114, 128));
			}
			list.Add(val2);
			catIconIdx["未分类"] = list.Count - 1;
		}
		foreach (GameItem allGame in allGames)
		{
			string text = IconFileFor(allGame.Name);
			if (text == null && iconMap.GameIcons.ContainsKey(allGame.Path))
			{
				text = iconMap.GameIcons[allGame.Path];
			}
			if (text != null)
			{
				Bitmap val3 = LoadCustomIcon(text);
				if (val3 != null)
				{
					list.Add(val3);
					gameIconIdx[allGame.Path] = list.Count - 1;
				}
			}
		}
		// 旧的图标位图不再使用，先释放掉，否则每次重建都会泄漏一批 GDI 位图
		List<Bitmap> list2 = gamePanel.Icons;
		gamePanel.Icons = list;
		if (list2 != null)
		{
			foreach (Bitmap bmp in list2)
			{
				try
				{
					bmp.Dispose();
				}
				catch
				{
				}
			}
		}
		((Control)gamePanel).Invalidate();
	}

	private Bitmap MakeIcon(string label, Color color)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected Obj, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected Obj, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected Obj, but got Unknown
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected Obj, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected Obj, but got Unknown
		int num = Dpi.Px(72);
		Bitmap val = new Bitmap(num, num, (PixelFormat)2498570);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		try
		{
			val2.SmoothingMode = (SmoothingMode)4;
			val2.TextRenderingHint = (TextRenderingHint)3;
			Rectangle rectangle = new Rectangle(Dpi.Px(2), Dpi.Px(2), Dpi.Px(68), Dpi.Px(68));
			int num2 = Dpi.Px(10);
			GraphicsPath val3 = new GraphicsPath();
			val3.AddArc(rectangle.X, rectangle.Y, num2 * 2, num2 * 2, 180f, 90f);
			val3.AddArc(rectangle.Right - num2 * 2, rectangle.Y, num2 * 2, num2 * 2, 270f, 90f);
			val3.AddArc(rectangle.Right - num2 * 2, rectangle.Bottom - num2 * 2, num2 * 2, num2 * 2, 0f, 90f);
			val3.AddArc(rectangle.X, rectangle.Bottom - num2 * 2, num2 * 2, num2 * 2, 90f, 90f);
			val3.CloseFigure();
			SolidBrush val4 = new SolidBrush(color);
			try
			{
				val2.FillPath((Brush)(object)val4, val3);
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
				((IDisposable)val3)?.Dispose();
			}
			string text = (string.IsNullOrEmpty(label) ? "?" : label.Substring(0, 1).ToUpper());
			Font val5 = new Font("Microsoft YaHei", 29f, (FontStyle)1);
			try
			{
				SolidBrush val6 = new SolidBrush(Color.White);
				try
				{
					StringFormat val7 = new StringFormat
					{
						Alignment = (StringAlignment)1,
						LineAlignment = (StringAlignment)1
					};
					val2.DrawString(text, val5, (Brush)(object)val6, new RectangleF(0f, 0f, num, num), val7);
					return val;
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void ToggleView()
	{
		iconView = !iconView;
		ApplyView();
		SaveView();
		RefreshGames();
	}

	private void ApplyView()
	{
		gamePanel.SetView(iconView);
		((Control)viewToggleBtn).Text = "";
		((Control)viewToggleBtn).Invalidate();
	}

	private void LoadView()
	{
		try
		{
			if (File.Exists(viewPath))
			{
				iconView = File.ReadAllText(viewPath).Trim() == "icon";
			}
		}
		catch
		{
		}
		ApplyView();
	}

	private void SaveView()
	{
		try
		{
			File.WriteAllText(viewPath, iconView ? "icon" : "list");
		}
		catch
		{
		}
	}

	private void ScaleLayout()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected Obj, but got Unknown
		float s = Dpi.S;
		if (!(Math.Abs(s - 1f) < 0.01f))
		{
			ScaleTree((Control)(object)this, s);
			catList.ItemHeight = Dpi.Px(49);
			if (catBottomGap != null)
			{
				((Control)catBottomGap).Height = Dpi.Px(75);
			}
			((Control)statusLabel).Padding = new Padding(Dpi.Px(15), 0, 0, 0);
			base.MinimumSize = new Size(Dpi.Px(750), Dpi.Px(480));
			RoundButton((Button)(object)themeToggleBtn, Dpi.Px(5));
			RoundButton((Button)(object)viewToggleBtn, Dpi.Px(5));
			RoundButton((Button)(object)refreshBtn, Dpi.Px(5));
			SetPillRegion((Control)(object)searchPanel);
			GraphicsPath val = Gfx.RoundRect(new Rectangle(0, 0, ((Control)backBtn).Width, ((Control)backBtn).Height), ((Control)backBtn).Width / 2);
			try
			{
				((Control)backBtn).Region = new Region(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			LayoutToolbar();
			PlaceBackBtn();
		}
	}

	private static void ScaleTree(Control parent, float s)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected Obj, but got Unknown
		foreach (Control item in (ArrangedElementCollection)parent.Controls)
		{
			Control val = item;
			val.Location = new Point((int)Math.Round((float)val.Left * s), (int)Math.Round((float)val.Top * s));
			val.Size = new Size((int)Math.Round((float)val.Width * s), (int)Math.Round((float)val.Height * s));
			ScaleTree(val, s);
		}
	}

	private void LayoutToolbar()
	{
		((Control)closeBtn).Left = ((Control)toolbar).Width - ((Control)closeBtn).Width - Dpi.Px(8);
		((Control)minBtn).Left = ((Control)closeBtn).Left - ((Control)minBtn).Width - Dpi.Px(2);
		((Control)refreshBtn).Left = ((Control)minBtn).Left - ((Control)refreshBtn).Width - Dpi.Px(20);
		((Control)viewToggleBtn).Left = ((Control)refreshBtn).Left - ((Control)viewToggleBtn).Width - Dpi.Px(12);
		((Control)themeToggleBtn).Left = ((Control)viewToggleBtn).Left - ((Control)themeToggleBtn).Width - Dpi.Px(12);
		int val = ((Control)themeToggleBtn).Left - ((Control)searchPanel).Left - Dpi.Px(16);
		int num = Math.Max(Dpi.Px(150), Math.Min(Dpi.Px(200), val));
		((Control)searchPanel).Width = num;
		((Control)searchBox).Width = Math.Max(Dpi.Px(40), num - Dpi.Px(48));
		// 顶栏里的控件按顶栏实际高度垂直居中
		CenterInToolbar((Control)(object)themeToggleBtn);
		CenterInToolbar((Control)(object)viewToggleBtn);
		CenterInToolbar((Control)(object)refreshBtn);
		CenterInToolbar((Control)(object)minBtn);
		CenterInToolbar((Control)(object)closeBtn);
		CenterInToolbar((Control)(object)searchPanel);
		// 搜索框文字（含占位文字）在搜索框内垂直居中
		int num2 = Math.Max(0, (((Control)searchPanel).Height - ((Control)searchBox).Height) / 2);
		((Control)searchBox).Top = num2;
		((Control)searchHint).Top = num2;
		SetPillRegion((Control)(object)searchPanel);
	}

	private void CenterInToolbar(Control ctl)
	{
		if (ctl != null)
		{
			ctl.Top = Math.Max(0, (((Control)toolbar).Height - ctl.Height) / 2);
		}
	}

	private static bool IsGameFile(string path)
	{
		string text = Path.GetExtension(path).ToLowerInvariant();
		switch (text)
		{
		default:
			return text == ".url";
		case ".swf":
		case ".exe":
		case ".lnk":
			return true;
		}
	}

	private void ScanAll()
	{
		allGames.Clear();
		allCats.Clear();
		if (!Directory.Exists(gamesDir))
		{
			Directory.CreateDirectory(gamesDir);
			string path = Path.Combine(gamesDir, "说明.txt");
			if (!File.Exists(path))
			{
				File.WriteAllText(path, "在这个文件夹里新建子文件夹，文件夹名就是分类名。\n把游戏放到对应文件夹里，工具会自动显示。\n支持：.swf、.exe、快捷方式（.lnk）、子文件夹。\n\n例如：\n  games/\n    动作/\n      game1.swf\n      某游戏.exe\n    射击/\n      game2.swf\n\n增加文件夹 = 增加分类，重命文件夹 = 重命分类。\n", Encoding.UTF8);
			}
			return;
		}
		string[] directories = Directory.GetDirectories(gamesDir);
		foreach (string path2 in directories)
		{
			string fileName = Path.GetFileName(path2);
			int num = 0;
			string[] files = Directory.GetFiles(path2);
			foreach (string path3 in files)
			{
				if (IsGameFile(path3))
				{
					allGames.Add(new GameItem
					{
						Name = Path.GetFileNameWithoutExtension(path3),
						Path = path3,
						Category = fileName
					});
					num++;
				}
			}
			files = Directory.GetDirectories(path2);
			foreach (string path4 in files)
			{
				allGames.Add(new GameItem
				{
					Name = Path.GetFileName(path4),
					Path = path4,
					Category = fileName,
					IsFolder = true
				});
				num++;
			}
			allCats.Add(new CatInfo
			{
				Name = fileName,
				Count = num
			});
		}
		int num2 = 0;
		directories = Directory.GetFiles(gamesDir);
		foreach (string path5 in directories)
		{
			if (IsGameFile(path5))
			{
				allGames.Add(new GameItem
				{
					Name = Path.GetFileNameWithoutExtension(path5),
					Path = path5,
					Category = "未分类"
				});
				num2++;
			}
		}
		if (num2 > 0)
		{
			allCats.Add(new CatInfo
			{
				Name = "未分类",
				Count = num2
			});
		}
	}

	private void InvalidateCatItem(int idx)
	{
		if (idx >= 0 && idx < catList.Items.Count)
		{
			Rectangle itemRectangle = catList.GetItemRectangle(idx);
			if (itemRectangle.Height <= 0)
			{
				((Control)catList).Invalidate();
				return;
			}
			itemRectangle.Inflate(Dpi.Px(2), 0);
			((Control)catList).Invalidate(itemRectangle);
		}
	}

	private void RefreshCats()
	{
		catHoverIdx = -1;
		int selectedIndex = catList.SelectedIndex;
		catList.Items.Clear();
		catList.Items.Add(new CatInfo
		{
			Name = "全部",
			Count = allGames.Count
		});
		foreach (CatInfo allCat in allCats)
		{
			catList.Items.Add(allCat);
		}
		if (selectedIndex >= 0 && selectedIndex < catList.Items.Count)
		{
			catList.SelectedIndex = selectedIndex;
		}
		else
		{
			catList.SelectedIndex = 0;
		}
		LayoutCatBar();
	}

	private void RefreshGames()
	{
		string text = (((Control)searchBox).Text ?? "").ToLower();
		if (catList.SelectedItem is CatInfo catInfo)
		{
			currentCat = catInfo.Name;
		}
		else
		{
			currentCat = "全部";
		}
		if (navDir != null && !Directory.Exists(navDir))
		{
			navDir = null;
			navStack.Clear();
		}
		List<GameItem> list = new List<GameItem>();
		if (navDir != null)
		{
			list.AddRange(ScanFolder(navDir, CategoryOf(navDir)));
		}
		else
		{
			foreach (GameItem allGame in allGames)
			{
				if (!(currentCat != "全部") || !(allGame.Category != currentCat))
				{
					list.Add(new GameItem
					{
						Name = allGame.Name,
						Path = allGame.Path,
						Category = allGame.Category,
						IsFolder = allGame.IsFolder
					});
				}
			}
		}
		List<GameItem> list2 = new List<GameItem>();
		foreach (GameItem item in list)
		{
			if (item.IsFolder)
			{
				list2.Add(item);
			}
		}
		foreach (GameItem item2 in list)
		{
			if (!item2.IsFolder)
			{
				list2.Add(item2);
			}
		}
		list = list2;
		List<GameItem> list3 = new List<GameItem>();
		foreach (GameItem item3 in list)
		{
			if (text.Length > 0 && !item3.Name.ToLower().Contains(text))
			{
				continue;
			}
			if (iconView)
			{
				if (gameIconIdx.ContainsKey(item3.Path))
				{
					item3.IconIndex = gameIconIdx[item3.Path];
				}
				else if (catIconIdx.ContainsKey(item3.Category))
				{
					item3.IconIndex = catIconIdx[item3.Category];
				}
			}
			list3.Add(item3);
		}
		bool flag = navDir != null;
		gamePanel.ReserveTop = flag;
		gamePanel.HeaderText = (flag ? Path.GetFileName(navDir.TrimEnd(new char[2] { '\\', '/' })) : null);
		if (((Control)backBtn).Visible != flag)
		{
			((Control)backBtn).Visible = flag;
			if (flag)
			{
				PlaceBackBtn();
			}
		}
		gamePanel.SetItems(list3);
		string text2 = "共 " + list3.Count + " 个游戏";
		if (flag)
		{
			text2 = text2 + "  ·  " + Path.GetFileName(navDir.TrimEnd(new char[2] { '\\', '/' }));
		}
		else if (currentCat != "全部")
		{
			text2 = text2 + "  ·  " + currentCat;
		}
		if (text.Length > 0)
		{
			text2 = text2 + "  ·  搜索 \"" + text + "\"";
		}
		((Control)statusLabel).Text = text2;
	}

	private string CategoryOf(string dir)
	{
		try
		{
			string[] array = Path.GetRelativePath(gamesDir, dir).Split(new char[2] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 1)
			{
				return array[0];
			}
		}
		catch
		{
		}
		return "未分类";
	}

	private List<GameItem> ScanFolder(string dir, string cat)
	{
		List<GameItem> list = new List<GameItem>();
		try
		{
			string[] files = Directory.GetFiles(dir);
			foreach (string path in files)
			{
				if (IsGameFile(path))
				{
					list.Add(new GameItem
					{
						Name = Path.GetFileNameWithoutExtension(path),
						Path = path,
						Category = cat
					});
				}
			}
			files = Directory.GetDirectories(dir);
			foreach (string path2 in files)
			{
				list.Add(new GameItem
				{
					Name = Path.GetFileName(path2),
					Path = path2,
					Category = cat,
					IsFolder = true
				});
			}
		}
		catch
		{
		}
		return list;
	}

	private void EnterFolder(GameItem item)
	{
		if (navDir != item.Path)
		{
			navStack.Add(navDir);
		}
		navDir = item.Path;
		RefreshGames();
	}

	private void PlaceBackBtn()
	{
		if (backBtn != null && gamePanel != null)
		{
			((Control)backBtn).Left = Math.Max(0, ((Control)gamePanel).ClientSize.Width - ((Control)backBtn).Width - Dpi.Px(15));
			((Control)backBtn).Top = Dpi.Px(15);
			((Control)backBtn).BringToFront();
		}
	}

	private void GoBack()
	{
		if (navDir != null)
		{
			if (navStack.Count > 0)
			{
				navDir = navStack[navStack.Count - 1];
				navStack.RemoveAt(navStack.Count - 1);
			}
			else
			{
				navDir = null;
			}
			RefreshGames();
		}
	}

	private void LaunchGame(string path)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!File.Exists(path) && !Directory.Exists(path))
		{
			MessageBox.Show("文件不存在：\n" + path, "提示");
			return;
		}
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = path,
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			MessageBox.Show("无法打开: " + ex.Message, "错误");
		}
	}

	private static void RoundButton(Button b, int r)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected Obj, but got Unknown
		GraphicsPath val = Gfx.RoundRect(new Rectangle(0, 0, ((Control)b).Width, ((Control)b).Height), r);
		try
		{
			((Control)b).Region = new Region(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (b is RoundBtn roundBtn)
		{
			roundBtn.Radius = r;
		}
	}

	private static void SetPillRegion(Control c)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		GraphicsPath val = Gfx.RoundRect(new Rectangle(0, 0, c.Width, c.Height), Math.Max(2, c.Height / 2));
		try
		{
			c.Region = new Region(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ShowInFolder(string path)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!File.Exists(path) && !Directory.Exists(path))
		{
			MessageBox.Show("文件不存在：\n" + path, "提示");
			return;
		}
		try
		{
			Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"")
			{
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			MessageBox.Show("无法打开文件夹: " + ex.Message, "错误");
		}
	}

	private void DrawBtnIcon(Graphics g, Control btn, string kind)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected Obj, but got Unknown
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Expected Obj, but got Unknown
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Expected Obj, but got Unknown
		if (btn == null || btn.Width <= 0)
		{
			return;
		}
		g.SmoothingMode = (SmoothingMode)4;
		float s = Dpi.S;
		int num = btn.Width / 2;
		int num2 = btn.Height / 2;
		int num3 = Dpi.Px(10);
		Pen val = new Pen(btn.ForeColor, Math.Max(1f, 1.2f * s));
		try
		{
			SolidBrush val2 = new SolidBrush(btn.ForeColor);
			try
			{
				switch (kind)
				{
				case "sunmoon":
				{
					if (theme == AppTheme.Dark)
					{
						int num12 = num3 / 2 - Dpi.Px(2);
						g.DrawEllipse(val, num - num12, num2 - num12, num12 * 2, num12 * 2);
						for (int i = 0; i < 8; i++)
						{
							double num13 = Math.PI * (double)i / 4.0;
							g.DrawLine(val, (float)num + (float)Math.Round(Math.Cos(num13) * (double)(num12 + Dpi.Px(2))), (float)num2 + (float)Math.Round(Math.Sin(num13) * (double)(num12 + Dpi.Px(2))), (float)num + (float)Math.Round(Math.Cos(num13) * (double)(num12 + Dpi.Px(4))), (float)num2 + (float)Math.Round(Math.Sin(num13) * (double)(num12 + Dpi.Px(4))));
						}
						break;
					}
					int num14 = num3 / 2 + Dpi.Px(1);
					int num15 = (int)Math.Round((double)num14 * 0.62);
					int num16 = -(int)Math.Round((double)num14 * 0.34);
					double num17 = Math.Sqrt((double)num15 * (double)num15 + (double)num16 * (double)num16);
					double num18 = Math.Atan2(num16, num15) * 180.0 / Math.PI;
					double num19 = Math.Acos(Math.Min(1.0, num17 / (2.0 * (double)num14))) * 180.0 / Math.PI;
					double num20 = num18 + num19;
					double num21 = 360.0 - 2.0 * num19;
					Rectangle rectangle = new Rectangle(num - num14, num2 - num14, num14 * 2, num14 * 2);
					Rectangle rectangle2 = new Rectangle(num - num14 + num15, num2 - num14 + num16, num14 * 2, num14 * 2);
					double num22 = (double)num + (double)num14 * Math.Cos(num20 * Math.PI / 180.0);
					double num23 = (double)num2 + (double)num14 * Math.Sin(num20 * Math.PI / 180.0);
					float num24 = (float)(Math.Atan2(x: (double)num + (double)num14 * Math.Cos((num18 - num19) * Math.PI / 180.0) - (double)(num + num15), y: (double)num2 + (double)num14 * Math.Sin((num18 - num19) * Math.PI / 180.0) - (double)(num2 + num16)) * 180.0 / Math.PI);
					float num25;
					for (num25 = (float)(Math.Atan2(num23 - (double)(num2 + num16), num22 - (double)(num + num15)) * 180.0 / Math.PI) - num24; num25 > 180f; num25 -= 360f)
					{
					}
					for (; num25 < -180f; num25 += 360f)
					{
					}
					GraphicsPath val4 = new GraphicsPath();
					try
					{
						val4.AddArc(rectangle, (float)num20, (float)num21);
						val4.AddArc(rectangle2, num24, num25);
						val4.CloseFigure();
						g.DrawPath(val, val4);
						break;
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				case "view":
					if (iconView)
					{
						int num26 = Dpi.Px(11);
						int num27 = Math.Max(2, Dpi.Px(2));
						int num28 = Dpi.Px(2);
						for (int j = 0; j < 3; j++)
						{
							int num29 = num2 - Dpi.Px(4) + j * (num27 + num28);
							g.FillRectangle((Brush)(object)val2, num - num26 / 2, num29, num27, num27);
							g.FillRectangle((Brush)(object)val2, num - num26 / 2 + num27 + Dpi.Px(2), num29 + num27 / 2, num26 - num27 - Dpi.Px(2), Math.Max(1, Dpi.Px(1)));
						}
					}
					else
					{
						int num30 = Dpi.Px(5);
						int num31 = Dpi.Px(2);
						int num32 = num - num30 - num31 / 2;
						int num33 = num2 - num30 - num31 / 2;
						g.DrawRectangle(val, num32, num33, num30, num30);
						g.DrawRectangle(val, num32 + num30 + num31, num33, num30, num30);
						g.DrawRectangle(val, num32, num33 + num30 + num31, num30, num30);
						g.DrawRectangle(val, num32 + num30 + num31, num33 + num30 + num31, num30, num30);
					}
					break;
				case "refresh":
				{
					int num4 = num3 / 2;
					Pen val3 = new Pen(btn.ForeColor, Math.Max(1f, 1.2f * s));
					try
					{
						val3.StartCap = (LineCap)2;
						val3.EndCap = (LineCap)2;
						g.DrawArc(val3, num - num4, num2 - num4, num4 * 2, num4 * 2, 35, 285);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
					double num5 = 5.585053606381854;
					float num6 = (float)num + (float)(Math.Cos(num5) * (double)num4);
					float num7 = (float)num2 + (float)(Math.Sin(num5) * (double)num4);
					double num8 = num5 + Math.PI / 2.0;
					int num9 = Dpi.Px(3);
					float num10 = (float)Math.Cos(num8);
					float num11 = (float)Math.Sin(num8);
					g.DrawLine(val, num6 - num10 * (float)num9 - num11 * (float)num9 * 0.7f, num7 - num11 * (float)num9 + num10 * (float)num9 * 0.7f, num6, num7);
					g.DrawLine(val, num6 - num10 * (float)num9 + num11 * (float)num9 * 0.7f, num7 - num11 * (float)num9 - num10 * (float)num9 * 0.7f, num6, num7);
					break;
				}
				}
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

	private void ToggleFullscreen()
	{
		int tickCount = Environment.TickCount;
		if (tickCount - lastFullToggleTick < 350)
		{
			return;
		}
		lastFullToggleTick = tickCount;
		if (!isFull)
		{
			preFullBounds = base.Bounds;
			isFull = true;
			base.WindowState = (FormWindowState)0;
			try
			{
				SetWindowRgn(base.Handle, IntPtr.Zero, bRedraw: true);
			}
			catch
			{
			}
			base.Bounds = Screen.FromControl((Control)(object)this).Bounds;
			SetNoRound();
		}
		else
		{
			isFull = false;
			base.Bounds = preFullBounds;
			ApplyRoundedCorners();
		}
		LayoutToolbar();
		PlaceBackBtn();
		base.Invalidate(true);
	}

	[DllImport("user32.dll")]
	private static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	private static extern nint SendMessage(nint hWnd, int Msg, int wParam, int lParam);

	private void StartDrag()
	{
		ReleaseCapture();
		SendMessage(base.Handle, 161, 2, 0);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 274 && (((IntPtr)(nint)m.WParam).ToInt32() & 0xFFF0) == 61472)
		{
			if (!minimizing)
			{
				minimizing = true;
				base.WindowState = (FormWindowState)1;
				minimizing = false;
			}
		}
		else if (m.Msg == 163 && ((IntPtr)(nint)m.WParam).ToInt32() == 2)
		{
			ToggleFullscreen();
		}
		else if (m.Msg == 132)
		{
			base.WndProc(ref m);
			if (!isFull && ((IntPtr)(nint)m.Result).ToInt32() == 1)
			{
				int num = ((IntPtr)(nint)m.LParam).ToInt32();
				int x = (short)(num & 0xFFFF);
				int y = (short)((num >> 16) & 0xFFFF);
				Point point = base.PointToClient(new Point(x, y));
				int num2 = 6;
				bool flag = point.X <= num2;
				bool flag2 = point.X >= base.ClientSize.Width - num2;
				bool flag3 = point.Y <= num2;
				bool flag4 = point.Y >= base.ClientSize.Height - num2;
				if (flag3 & flag)
				{
					m.Result = (IntPtr)13;
				}
				else if (flag3 & flag2)
				{
					m.Result = (IntPtr)14;
				}
				else if (flag4 & flag)
				{
					m.Result = (IntPtr)16;
				}
				else if (flag4 & flag2)
				{
					m.Result = (IntPtr)17;
				}
				else if (flag)
				{
					m.Result = (IntPtr)10;
				}
				else if (flag2)
				{
					m.Result = (IntPtr)11;
				}
				else if (flag3)
				{
					m.Result = (IntPtr)12;
				}
				else if (flag4)
				{
					m.Result = (IntPtr)15;
				}
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	private static Color Mix(Color a, Color b, float t)
	{
		return Color.FromArgb((int)Math.Round((float)(int)a.R + (float)(b.R - a.R) * t), (int)Math.Round((float)(int)a.G + (float)(b.G - a.G) * t), (int)Math.Round((float)(int)a.B + (float)(b.B - a.B) * t));
	}

	private int SplitterMax()
	{
		if (split == null || ((Control)split).Width <= 0)
		{
			return Dpi.Px(400);
		}
		return Math.Max(Dpi.Px(90), ((Control)split).Width - Dpi.Px(260) - split.SplitterWidth);
	}

	private void LoadSidebar()
	{
		try
		{
			if (split != null && File.Exists(sidebarPath) && int.TryParse(File.ReadAllText(sidebarPath).Trim(), out var result))
			{
				result = Math.Max(Dpi.Px(90), Math.Min(SplitterMax(), result));
				if (result > 0 && result != split.SplitterDistance)
				{
					split.SplitterDistance = result;
				}
				LayoutCatBar();
			}
		}
		catch
		{
		}
	}

	private void SaveSidebar()
	{
		try
		{
			if (split != null && ((Control)split).Width > 0)
			{
				File.WriteAllText(sidebarPath, split.SplitterDistance.ToString());
			}
		}
		catch
		{
		}
	}

	private void InitCatBar(Control host)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected Obj, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected Obj, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected Obj, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected Obj, but got Unknown
		catSideHost = host;
		BufPanel bufPanel = new BufPanel();
		((Control)bufPanel).Visible = false;
		((Panel)bufPanel).TabStop = false;
		catBar = bufPanel;
		((Control)catBar).Paint += (object? s, PaintEventArgs e) =>
		{
			DrawCatBar(e.Graphics);
		};
		((Control)catBar).MouseDown += CatBarDown;
		((Control)catBar).MouseMove += CatBarMove;
		((Control)catBar).MouseUp += (object? s, MouseEventArgs e) =>
		{
			catBarDragY = -1;
			((Control)catBar).Capture = false;
			catBarShowTimer.Start();
			((Control)catBar).Invalidate();
		};
		((Control)catBar).MouseEnter += (object? s, EventArgs e) =>
		{
			((Control)catBar).Invalidate();
		};
		((Control)catBar).MouseLeave += (object? s, EventArgs e) =>
		{
			((Control)catBar).Invalidate();
		};
		catBarShowTimer.Tick += (object? s, EventArgs e) =>
		{
			catBarShowTimer.Stop();
			catBarShow = false;
			if (catBar != null)
			{
				((Control)catBar).Invalidate();
			}
		};
		host.Controls.Add((Control)(object)catBar);
		((Control)catBar).BringToFront();
		host.Resize += (object? s, EventArgs e) =>
		{
			LayoutCatBar();
		};
		// 面板的 Resize 事件早于子控件布局完成，此时读到的分类列表位置还是旧的，
		// 全屏 / 最大化这类大幅改尺寸后滚动条会停在旧位置。这里再跟随分类列表
		// 自身的位置和尺寸变化重算一次，保证滚动条始终贴着列表右缘。
		((Control)catList).Resize += (object? s, EventArgs e) =>
		{
			LayoutCatBar();
		};
		((Control)catList).LocationChanged += (object? s, EventArgs e) =>
		{
			LayoutCatBar();
		};
		catBarTimer.Tick += (object? s, EventArgs e) =>
		{
			if (catBar != null && ((Control)catBar).Visible && catList.TopIndex != catBarLastTop)
			{
				catBarLastTop = catList.TopIndex;
				((Control)catBar).Invalidate();
			}
		};
		catBarTimer.Start();
	}

	private void LayoutCatBar()
	{
		if (catBar == null || catList == null)
		{
			return;
		}
		bool flag = catList.Items.Count * catList.ItemHeight > ((Control)catList).ClientSize.Height;
		if (((Control)catBar).Visible != flag)
		{
			((Control)catBar).Visible = flag;
		}
		if (flag)
		{
			int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
			Rectangle rectangle = new Rectangle(((Control)catList).Right - verticalScrollBarWidth, ((Control)catList).Top, verticalScrollBarWidth, ((Control)catList).Height);
			if (((Control)catBar).Bounds != rectangle)
			{
				((Control)catBar).Bounds = rectangle;
			}
			((Control)catBar).BringToFront();
			((Control)catBar).Invalidate();
		}
	}

	private int MaxCatTop()
	{
		int num = Math.Max(1, ((Control)catList).ClientSize.Height / Math.Max(1, catList.ItemHeight));
		return Math.Max(0, catList.Items.Count - num);
	}

	private void CatBarThumb(out int ty, out int th)
	{
		int num = Math.Max(1, ((Control)catList).ClientSize.Height);
		int num2 = catList.Items.Count * catList.ItemHeight;
		th = Dpi.Px(24);
		if (num2 > num)
		{
			th = Math.Max(th, (int)((long)num * (long)num / num2));
		}
		th = Math.Min(th, num);
		int num3 = MaxCatTop();
		int num4 = Math.Max(0, Math.Min(catList.TopIndex, num3));
		ty = (int)((num3 != 0) ? ((long)Math.Max(0, num - th) * (long)num4 / num3) : 0);
	}

	private void DrawCatBar(Graphics g)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected Obj, but got Unknown
		SolidBrush val = new SolidBrush(theme.SidebarBg);
		try
		{
			g.FillRectangle((Brush)(object)val, ((Control)catBar).ClientRectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (!catBarShow && catBarDragY < 0)
		{
			return;
		}
		CatBarThumb(out var ty, out var th);
		int num = Dpi.Px(6);
		Rectangle r = new Rectangle((((Control)catBar).ClientSize.Width - num) / 2, ty + Dpi.Px(2), num, Math.Max(Dpi.Px(8), th - Dpi.Px(4)));
		g.SmoothingMode = (SmoothingMode)4;
		SolidBrush val2 = new SolidBrush(Color.FromArgb(130, 130, 130));
		try
		{
			GraphicsPath val3 = Gfx.RoundRect(r, Dpi.Px(3));
			try
			{
				g.FillPath((Brush)(object)val2, val3);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void SetCatTop(int top)
	{
		top = Math.Max(0, Math.Min(MaxCatTop(), top));
		catList.TopIndex = top;
		catBarLastTop = top;
		catBarShow = true;
		catBarShowTimer.Stop();
		catBarShowTimer.Start();
		if (catBar != null)
		{
			((Control)catBar).Invalidate();
		}
	}

	private void CatBarDown(object s, MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576 && (catBarShow || catBarDragY >= 0))
		{
			((Control)catBar).Capture = true;
			CatBarThumb(out var ty, out var th);
			if (e.Y >= ty && e.Y <= ty + th)
			{
				catBarDragY = e.Y;
				catBarDragTop = catList.TopIndex;
			}
			else
			{
				int num = Math.Max(1, ((Control)catList).ClientSize.Height / Math.Max(1, catList.ItemHeight));
				SetCatTop(catList.TopIndex + ((e.Y < ty) ? (-num) : num));
			}
		}
	}

	private void CatBarMove(object s, MouseEventArgs e)
	{
		if (catBarDragY >= 0)
		{
			catBarShow = true;
			catBarShowTimer.Stop();
			CatBarThumb(out var _, out var th);
			int num = Math.Max(1, ((Control)catList).ClientSize.Height - th);
			int num2 = MaxCatTop();
			if (num2 != 0)
			{
				SetCatTop(catBarDragTop + (int)((long)(e.Y - catBarDragY) * (long)num2 / num));
			}
		}
	}

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
	private static extern int SetWindowTheme(nint hWnd, string pszSubAppName, string pszSubIdList);

	private void ApplySideScrollbarTheme()
	{
		if (catList == null || !((Control)catList).IsHandleCreated)
		{
			return;
		}
		try
		{
			SetWindowTheme(((Control)catList).Handle, (theme == AppTheme.Dark) ? "DarkMode_Explorer" : "Explorer", null);
		}
		catch
		{
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		ApplySideScrollbarTheme();
		LoadSidebar();
		// 启动时 Windows 会把初始焦点给到搜索框，导致占位文字消失、光标闪烁，
		// 看上去像刚被点过。这里在界面显示后把焦点交还窗体，保持未激活状态。
		BeginInvoke(new Action(delegate
		{
			((ContainerControl)this).ActiveControl = null;
		}));
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

	private static bool MoveToRecycleBin(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT
		{
			hwnd = IntPtr.Zero,
			wFunc = 3u,
			pFrom = path + "\0",
			pTo = null,
			fFlags = 1108
		};
		return SHFileOperation(ref lpFileOp) == 0;
	}

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

	[DllImport("gdi32.dll")]
	private static extern nint CreateRoundRectRgn(int l, int t, int r, int b, int w, int h);

	[DllImport("user32.dll")]
	private static extern int SetWindowRgn(nint hWnd, nint hRgn, bool bRedraw);

	private void SetNoRound()
	{
		try
		{
			int attrValue = 1;
			DwmSetWindowAttribute(base.Handle, 33, ref attrValue, 4);
		}
		catch
		{
		}
	}

	private void ApplyRoundedCorners()
	{
		if (isFull)
		{
			return;
		}
		if (!useRegionFallback)
		{
			try
			{
				int attrValue = 2;
				if (DwmSetWindowAttribute(base.Handle, 33, ref attrValue, 4) == 0)
				{
					int attrValue2 = -2;
					DwmSetWindowAttribute(base.Handle, 34, ref attrValue2, 4);
					return;
				}
			}
			catch
			{
			}
			useRegionFallback = true;
		}
		try
		{
			int num = Dpi.Px(16);
			nint num2 = CreateRoundRectRgn(0, 0, base.Width + 1, base.Height + 1, num, num);
			if (num2 != IntPtr.Zero)
			{
				SetWindowRgn(base.Handle, num2, bRedraw: true);
			}
		}
		catch
		{
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		ApplyRoundedCorners();
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (useRegionFallback && !isFull)
		{
			ApplyRoundedCorners();
		}
	}
}
