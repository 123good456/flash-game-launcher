using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

internal class GamePanel : Panel
{
	public List<Bitmap> Icons;

	public AppTheme Theme;

	private int hoverIdx = -1;

	private int scrollY;

	private int contentH;

	private bool sbDrag;

	private int sbDragY;

	private int sbDragScroll;

	private bool sbShow;

	private Timer sbTimer;

	private Font nameFontList;

	private Font nameFontIcon;

	private Font charWFont;

	private int charWCache = 1;

	private Font lineHCacheFont;

	private int lineHCache;

	private int[] rowTopCache;

	private int[] rowHCache;

	private int rowCacheRows = -1;

	private readonly Dictionary<string, bool> existsCache = new Dictionary<string, bool>();

	private const int CharsPerLine = 8;

	public bool ReserveTop;

	public string HeaderText;

	public bool LastRightHit;

	private readonly Dictionary<string, List<string>> wrapCache = new Dictionary<string, List<string>>();

	public List<GameItem> Items { get; private set; } = new List<GameItem>();

	public int SelectedIndex { get; private set; } = -1;

	public bool IconView { get; private set; }

	private int SBW => Dpi.Px(12);

	public Font NameFont
	{
		get
		{
			if (!IconView)
			{
				return nameFontList;
			}
			return nameFontIcon;
		}
	}

	private int LineH
	{
		get
		{
			// GetHeight() 是 GDI 调用，热路径里会被调用成千上万次，缓存起来
			if (lineHCacheFont != nameFontList)
			{
				lineHCacheFont = nameFontList;
				lineHCache = (int)Math.Ceiling(nameFontList.GetHeight() * 1.15f);
			}
			return lineHCache;
		}
	}

	private int IconNameH => 2 * LineH;

	private int Cols
	{
		get
		{
			if (IconView)
			{
				return 5;
			}
			int num = Math.Max(1, base.ClientSize.Width - MarginX * 2);
			int val = 8 * CharWidth() + Dpi.Px(42) + Dpi.Px(18) + Dpi.Px(6);
			int val2 = num / Math.Max(1, val);
			return Math.Max(2, Math.Min(4, val2));
		}
	}

	private int HeaderH => Dpi.Px(40);

	private int StartY
	{
		get
		{
			if (!ReserveTop)
			{
				return Dpi.Px(IconView ? 25 : 30);
			}
			return Dpi.Px(15) + HeaderH + 2 * LineH;
		}
	}

	private int MarginX => Dpi.Px(IconView ? 20 : 15);

	private int RowGap => Dpi.Px(12) + LineH;

	private int IconRowGap => Math.Max(Dpi.Px(10), Dpi.Px(150) - (Dpi.Px(72) + Dpi.Px(10) + LineH));

	private int CellW
	{
		get
		{
			int num = base.ClientSize.Width - MarginX * 2 - (SbVisible ? SBW : 0);
			if (num < Dpi.Px(100))
			{
				num = Dpi.Px(100);
			}
			return num / Cols;
		}
	}

	private bool SbVisible => contentH > base.ClientSize.Height;

	public event Action<int> ItemDoubleClicked;

	public event Action<int> SelectionChanged;

	public GamePanel()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected Obj, but got Unknown
		nameFontList = new Font("Microsoft YaHei", 12f);
		nameFontIcon = new Font("Microsoft YaHei", 12f);
		base.DoubleBuffered = true;
		base.AutoScroll = false;
		base.SetStyle((ControlStyles)143634, true);
		sbTimer = new Timer
		{
			Interval = 1200
		};
		sbTimer.Tick += (object? s, EventArgs e) =>
		{
			sbTimer.Stop();
			sbShow = false;
			base.Invalidate();
		};
	}

	public void SetItems(List<GameItem> items)
	{
		Items = items;
		SelectedIndex = -1;
		hoverIdx = -1;
		scrollY = 0;
		// 换行结果按"文字+宽度"缓存，跨分类切换复用，不再每次清空重算；
		// 只在条目过多时清一次，防止无限增长。
		if (wrapCache.Count > 5000)
		{
			wrapCache.Clear();
		}
		existsCache.Clear();
		UpdateScroll();
		base.Invalidate();
	}

	public GameItem GetItem(int idx)
	{
		if (idx >= 0 && idx < Items.Count)
		{
			return Items[idx];
		}
		return null;
	}

	public void SetView(bool iconView)
	{
		IconView = iconView;
		UpdateScroll();
		base.Invalidate();
	}

	public void SetTheme(AppTheme t)
	{
		Theme = t;
		base.BackColor = t.GridBg;
		base.Invalidate();
	}

	public GameItem GetSelectedItem()
	{
		if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
		{
			return Items[SelectedIndex];
		}
		return null;
	}

	private bool ExistsCached(string path)
	{
		// 每次重绘都对可见项查一次磁盘，鼠标一动就重绘，缓存起来避免反复 IO
		if (existsCache.TryGetValue(path, out var value))
		{
			return value;
		}
		value = File.Exists(path);
		existsCache[path] = value;
		return value;
	}

	private int CharWidth()
	{
		if (charWFont != NameFont)
		{
			charWFont = NameFont;
			charWCache = Math.Max(1, TextRenderer.MeasureText("游游游游游游游游", NameFont).Width / 8);
		}
		return charWCache;
	}

	private static List<string> WrapChars(string s, int per)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(s))
		{
			list.Add("");
			return list;
		}
		for (int i = 0; i < s.Length; i += per)
		{
			list.Add(s.Substring(i, Math.Min(per, s.Length - i)));
		}
		return list;
	}

	private int RowLines(int row)
	{
		int num = 1;
		int num2 = row * Cols;
		for (int i = num2; i < Math.Min(num2 + Cols, Items.Count); i++)
		{
			num = Math.Max(num, WrapChars(Items[i].Name, 8).Count);
		}
		return num;
	}

	private int RowContentH(int row)
	{
		return Dpi.Px(12) + RowLines(row) * LineH;
	}

	private int IconContentH(int row)
	{
		return Dpi.Px(72) + Dpi.Px(10) + IconLines(row) * LineH;
	}

	private int RowTop(int row)
	{
		if (rowCacheRows < 0)
		{
			UpdateScroll();
		}
		if (row < 0)
		{
			row = 0;
		}
		if (row > rowCacheRows)
		{
			row = rowCacheRows;
		}
		return rowTopCache[row];
	}

	private int IconLines(int row)
	{
		int num = 1;
		int num2 = row * Cols;
		int maxW = Math.Max(Dpi.Px(24), CellW - Dpi.Px(12));
		for (int i = num2; i < Math.Min(num2 + Cols, Items.Count); i++)
		{
			num = Math.Max(num, Math.Min(2, WrapByWidth(Items[i].Name, maxW).Count));
		}
		return num;
	}

	private List<string> WrapByWidth(string s, int maxW)
	{
		string key = maxW + "|" + s;
		if (wrapCache.TryGetValue(key, out var value))
		{
			return value;
		}
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(s))
		{
			list.Add("");
		}
		else
		{
			string text = "";
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				string text2 = text + c;
				if (text.Length > 0 && TextRenderer.MeasureText(text2, NameFont).Width > maxW)
				{
					list.Add(text);
					text = c.ToString();
				}
				else
				{
					text = text2;
				}
			}
			if (text.Length > 0)
			{
				list.Add(text);
			}
		}
		wrapCache[key] = list;
		return list;
	}

	private void ClampScroll()
	{
		int num = Math.Max(0, contentH - base.ClientSize.Height);
		if (scrollY < 0)
		{
			scrollY = 0;
		}
		if (scrollY > num)
		{
			scrollY = num;
		}
	}

	private void UpdateScroll()
	{
		// 一次性把所有行的顶部位置和行高算好缓存起来。
		// 原来 RowTop 每调用一次都要从第 0 行累加一遍，而 GetItemBounds 会被整屏逐项调用，
		// 文件一多就是 O(n²)，每帧都要重算，这是卡顿的主因。
		int cols = Cols;
		int rows = (Items.Count + cols - 1) / cols;
		if (rows < 0)
		{
			rows = 0;
		}
		rowTopCache = new int[rows + 1];
		rowHCache = new int[rows];
		int y = StartY;
		for (int r = 0; r < rows; r++)
		{
			rowTopCache[r] = y;
			int h = (IconView ? IconContentH(r) : RowContentH(r));
			rowHCache[r] = h;
			y += h + (IconView ? IconRowGap : RowGap);
		}
		rowTopCache[rows] = y;
		rowCacheRows = rows;
		contentH = y + Dpi.Px(20);
		ClampScroll();
	}

	private Rectangle GetItemBounds(int index)
	{
		if (index < 0 || index >= Items.Count)
		{
			return Rectangle.Empty;
		}
		int cols = Cols;
		int marginX = MarginX;
		int num = base.ClientSize.Width - marginX * 2 - (SbVisible ? SBW : 0);
		if (num < Dpi.Px(100))
		{
			num = Dpi.Px(100);
		}
		int num2 = num / cols;
		int row = index / cols;
		int num3 = index % cols;
		int x = marginX + num3 * num2;
		int y = RowTop(row) - scrollY;
		int height = ((row >= 0 && row < rowCacheRows) ? rowHCache[row] : 0);
		return new Rectangle(x, y, num2, height);
	}

	private int GetItemIndexAt(Point pt)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (GetItemBounds(i).Contains(pt))
			{
				return i;
			}
		}
		return -1;
	}

	private int GetHitIndex(Point p)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			if (HitItem(i, p))
			{
				return i;
			}
		}
		return -1;
	}

	private bool HitItem(int idx, Point p)
	{
		if (idx < 0 || idx >= Items.Count)
		{
			return false;
		}
		Rectangle itemBounds = GetItemBounds(idx);
		if (!itemBounds.Contains(p))
		{
			return false;
		}
		if (!IconView)
		{
			return true;
		}
		GameItem gameItem = Items[idx];
		int num = Dpi.Px(72);
		if (new Rectangle(itemBounds.X + (itemBounds.Width - num) / 2, itemBounds.Y, num, num).Contains(p))
		{
			return true;
		}
		int num2 = Math.Max(Dpi.Px(24), itemBounds.Width - Dpi.Px(12));
		int num3 = Math.Min(TextRenderer.MeasureText(gameItem.Name, NameFont).Width, num2);
		return new Rectangle(itemBounds.X + Dpi.Px(6) + (num2 - num3) / 2, itemBounds.Y + num + Dpi.Px(10), num3, IconNameH).Contains(p);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		base.OnPaint(e);
		if (Theme == null)
		{
			return;
		}
		SolidBrush val = new SolidBrush(Theme.GridBg);
		try
		{
			e.Graphics.FillRectangle((Brush)(object)val, e.ClipRectangle);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		for (int i = 0; i < Items.Count; i++)
		{
			Rectangle itemBounds = GetItemBounds(i);
			if (e.ClipRectangle.IntersectsWith(itemBounds))
			{
				bool isSel = i == SelectedIndex;
				GameItem gameItem = Items[i];
				bool fileExists = ExistsCached(gameItem.Path);
				if (IconView)
				{
					DrawIconItem(e.Graphics, itemBounds, gameItem, i, isSel, fileExists);
				}
				else
				{
					DrawListItem(e.Graphics, itemBounds, gameItem, i, isSel, fileExists);
				}
			}
		}
		DrawScrollbar(e.Graphics);
		DrawHeader(e.Graphics);
	}

	private void DrawHeader(Graphics g)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected Obj, but got Unknown
		if (!ReserveTop || string.IsNullOrEmpty(HeaderText))
		{
			return;
		}
		g.SmoothingMode = (SmoothingMode)4;
		Rectangle rectangle = new Rectangle(0, Dpi.Px(15), base.ClientSize.Width, HeaderH);
		Font val = new Font(base.Font.FontFamily, 15f, (FontStyle)1);
		try
		{
			TextRenderer.DrawText((IDeviceContext)(object)g, HeaderText, val, rectangle, Theme.TextColor, (TextFormatFlags)32773);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ThumbRect(out int ty, out int th)
	{
		int num = Math.Max(1, base.ClientSize.Height);
		th = Math.Max(Dpi.Px(30), (int)((long)num * (long)num / Math.Max(1, contentH)));
		int num2 = Math.Max(0, contentH - num);
		ty = (int)((num2 > 0) ? ((long)scrollY * (long)(num - th) / num2) : 0);
	}

	private void ShowSb()
	{
		if (SbVisible)
		{
			sbShow = true;
			sbTimer.Stop();
			sbTimer.Start();
		}
	}

	private void DrawScrollbar(Graphics g)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected Obj, but got Unknown
		if (!SbVisible || !sbShow)
		{
			return;
		}
		ThumbRect(out var ty, out var th);
		Rectangle r = new Rectangle(base.ClientSize.Width - SBW + Dpi.Px(3), ty + Dpi.Px(2), SBW - Dpi.Px(6), Math.Max(Dpi.Px(8), th - Dpi.Px(4)));
		SolidBrush val = new SolidBrush(Color.FromArgb(130, 130, 130));
		try
		{
			GraphicsPath val2 = Gfx.RoundRect(r, Dpi.Px(3));
			try
			{
				g.FillPath((Brush)(object)val, val2);
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

	private bool OnScrollbar(Point p)
	{
		if (SbVisible && sbShow)
		{
			return p.X >= base.ClientSize.Width - SBW;
		}
		return false;
	}

	public void ScrollBy(int delta)
	{
		if (SbVisible)
		{
			scrollY -= (int)((double)delta * 60.0 / 120.0);
			ClampScroll();
			ShowSb();
			base.Invalidate();
		}
	}

	private void DrawListItem(Graphics g, Rectangle b, GameItem item, int idx, bool isSel, bool fileExists)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected Obj, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected Obj, but got Unknown
		int num = Dpi.Px(42);
		int num2 = Dpi.Px(10);
		int num3 = b.Y + Dpi.Px(6) + Math.Max(0, (NameFont.Height - num2) / 2);
		PointF[] array = new PointF[3]
		{
			new PointF(b.X + num, num3),
			new PointF(b.X + num, num3 + num2),
			new PointF(b.X + num + Dpi.Px(9), num3 + num2 / 2)
		};
		SolidBrush val = new SolidBrush(Color.FromArgb(59, 130, 246));
		try
		{
			g.FillPolygon((Brush)(object)val, array);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		Color color = (item.IsFolder ? Theme.FolderText : (fileExists ? Theme.TextColor : Color.Gray));
		List<string> list = WrapChars(item.Name, 8);
		int x = b.X + num + Dpi.Px(18);
		int num4 = b.Y + Dpi.Px(6);
		foreach (string item2 in list)
		{
			if (isSel)
			{
				// 文件夹只加粗，游戏文件保持加粗+下划线
				Font val2 = new Font(NameFont, item.IsFolder ? FontStyle.Bold : (FontStyle)5);
				try
				{
					TextRenderer.DrawText((IDeviceContext)(object)g, item2, val2, new Point(x, num4), color);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			else
			{
				TextRenderer.DrawText((IDeviceContext)(object)g, item2, NameFont, new Point(x, num4), color);
			}
			num4 += LineH;
		}
	}

	private void DrawIconItem(Graphics g, Rectangle b, GameItem item, int idx, bool isSel, bool fileExists)
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		int num = Dpi.Px(72);
		int num2 = b.X + (b.Width - num) / 2;
		int y = b.Y;
		int num3 = Math.Max(Dpi.Px(24), b.Width - Dpi.Px(12));
		if (item.IconIndex >= 0 && Icons != null && item.IconIndex < Icons.Count)
		{
			g.DrawImage((Image)(object)Icons[item.IconIndex], num2, y);
		}
		Color color = (item.IsFolder ? Theme.FolderText : (fileExists ? Theme.TextColor : Color.Gray));
		Rectangle rectangle = new Rectangle(b.X + Dpi.Px(6), y + num + Dpi.Px(10), num3, IconNameH);
		bool flag = isSel;
		List<string> list = WrapByWidth(item.Name, num3);
		if (list.Count > 2)
		{
			list = list.GetRange(0, 2);
		}
		int num4 = rectangle.Y;
		Font val = ((!flag) ? ((Font)null) : new Font(NameFont, item.IsFolder ? FontStyle.Bold : (FontStyle)5));
		try
		{
			Font val2 = (flag ? val : NameFont);
			foreach (string item2 in list)
			{
				int width = TextRenderer.MeasureText(item2, val2).Width;
				int x = rectangle.X + Math.Max(0, (rectangle.Width - width) / 2);
				TextRenderer.DrawText((IDeviceContext)(object)g, item2, val2, new Point(x, num4), color);
				num4 += LineH;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (sbDrag)
		{
			int num = Math.Max(1, base.ClientSize.Height);
			ThumbRect(out var _, out var th);
			int num2 = Math.Max(0, contentH - num);
			int num3 = Math.Max(1, num - th);
			scrollY = sbDragScroll + (int)((long)(e.Y - sbDragY) * (long)num2 / num3);
			ClampScroll();
			sbShow = true;
			sbTimer.Stop();
			base.Invalidate();
		}
		else
		{
			// 游戏区没有悬停高亮效果，只记录位置即可。
			// 原来每次鼠标移动都要整屏重绘，文件多的时候非常卡。
			hoverIdx = GetItemIndexAt(e.Location);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (sbDrag)
		{
			sbDrag = false;
			sbTimer.Start();
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		ScrollBy(e.Delta);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		hoverIdx = -1;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (!OnScrollbar(e.Location))
		{
			int hitIndex = GetHitIndex(e.Location);
			if (hitIndex >= 0)
			{
				SelectedIndex = hitIndex;
				base.Invalidate();
				SelectionChanged?.Invoke(hitIndex);
			}
		}
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		base.OnMouseDoubleClick(e);
		if (!OnScrollbar(e.Location))
		{
			int hitIndex = GetHitIndex(e.Location);
			if (hitIndex >= 0)
			{
				SelectedIndex = hitIndex;
				base.Invalidate();
				ItemDoubleClicked?.Invoke(hitIndex);
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		base.OnMouseDown(e);
		if (OnScrollbar(e.Location))
		{
			if ((int)e.Button == 1048576)
			{
				ThumbRect(out var ty, out var th);
				if (e.Y >= ty && e.Y <= ty + th)
				{
					sbDrag = true;
					sbDragY = e.Y;
					sbDragScroll = scrollY;
				}
				else
				{
					int num = Math.Max(1, base.ClientSize.Height);
					scrollY += ((e.Y < ty) ? (-num) : num);
					ClampScroll();
					ShowSb();
					base.Invalidate();
				}
			}
		}
		else if ((int)e.Button == 2097152)
		{
			int hitIndex = GetHitIndex(e.Location);
			LastRightHit = hitIndex >= 0;
			if (hitIndex >= 0)
			{
				SelectedIndex = hitIndex;
				base.Invalidate();
				SelectionChanged?.Invoke(hitIndex);
			}
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		UpdateScroll();
		base.Invalidate();
	}
}
