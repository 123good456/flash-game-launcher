using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// 分类列表：完全自绘 + 双缓冲的托管控件。
/// 原来的 ListBox 是系统原生控件，DoubleBuffered 与屏蔽 WM_ERASEBKGND 都无法真正开启
/// 双缓冲，鼠标在选项之间移动时局部重绘会直接画到屏幕上，表现为选中项一闪一闪。
/// 这里改成自绘控件（UserPaint + OptimizedDoubleBuffer + AllPaintingInWmPaint），
/// 所有内容先画进后台缓冲、再一次性贴出，从根源上消除闪烁。
/// </summary>
internal class CatList : Control
{
	private sealed class CatItemCollection : Collection<CatInfo>
	{
		private readonly CatList owner;

		public CatItemCollection(CatList owner)
		{
			this.owner = owner;
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			owner.OnItemsChanged();
		}

		protected override void InsertItem(int index, CatInfo item)
		{
			base.InsertItem(index, item);
			owner.OnItemsChanged();
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
			owner.OnItemsChanged();
		}

		protected override void SetItem(int index, CatInfo item)
		{
			base.SetItem(index, item);
			owner.OnItemsChanged();
		}
	}

	private readonly CatItemCollection items;

	private int itemHeight = 49;

	private int selectedIndex = -1;

	private int topIndex;

	public event DrawItemEventHandler DrawItem;

	public event EventHandler SelectedIndexChanged;

	public CatList()
	{
		items = new CatItemCollection(this);
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
		SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		TabStop = true;
		BackColor = Color.White;
		ForeColor = Color.Black;
	}

	public Collection<CatInfo> Items
	{
		get
		{
			return items;
		}
	}

	public int ItemHeight
	{
		get
		{
			return itemHeight;
		}
		set
		{
			int num = Math.Max(1, value);
			if (itemHeight != num)
			{
				itemHeight = num;
				if (topIndex > MaxTop)
				{
					topIndex = MaxTop;
				}
				Invalidate();
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			int num = ((value >= 0 && value < items.Count) ? value : (-1));
			if (selectedIndex != num)
			{
				selectedIndex = num;
				Invalidate();
				SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public CatInfo SelectedItem
	{
		get
		{
			if (selectedIndex < 0 || selectedIndex >= items.Count)
			{
				return null;
			}
			return items[selectedIndex];
		}
	}

	public int TopIndex
	{
		get
		{
			return topIndex;
		}
		set
		{
			int num = Math.Max(0, Math.Min(MaxTop, value));
			if (topIndex != num)
			{
				topIndex = num;
				Invalidate();
			}
		}
	}

	public int MaxTop
	{
		get
		{
			int num = Math.Max(1, ClientSize.Height / Math.Max(1, itemHeight));
			return Math.Max(0, items.Count - num);
		}
	}

	private int ContentWidth
	{
		get
		{
			int num = ClientSize.Width;
			if (items.Count * itemHeight > ClientSize.Height)
			{
				num -= SystemInformation.VerticalScrollBarWidth;
			}
			return Math.Max(0, num);
		}
	}

	private int VisibleRows
	{
		get
		{
			return Math.Max(1, ClientSize.Height / Math.Max(1, itemHeight));
		}
	}

	public Rectangle GetItemRectangle(int index)
	{
		if (index < 0 || index >= items.Count)
		{
			return Rectangle.Empty;
		}
		return new Rectangle(0, (index - topIndex) * itemHeight, ContentWidth, itemHeight);
	}

	public int IndexFromPoint(Point p)
	{
		if (items.Count == 0)
		{
			return -1;
		}
		if (p.X < 0 || p.X >= ClientSize.Width || p.Y < 0 || p.Y >= ClientSize.Height)
		{
			return -1;
		}
		int num = topIndex + p.Y / Math.Max(1, itemHeight);
		if (num < 0 || num >= items.Count)
		{
			return -1;
		}
		return num;
	}

	private void OnItemsChanged()
	{
		bool flag = false;
		if (selectedIndex >= items.Count)
		{
			selectedIndex = -1;
			flag = true;
		}
		if (topIndex > MaxTop)
		{
			topIndex = MaxTop;
		}
		Invalidate();
		if (flag)
		{
			SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			e.Graphics.FillRectangle(brush, ClientRectangle);
		}
		if (items.Count == 0)
		{
			return;
		}
		if (topIndex > MaxTop)
		{
			topIndex = MaxTop;
		}
		if (topIndex < 0)
		{
			topIndex = 0;
		}
		int num = Math.Min(items.Count - 1, topIndex + VisibleRows);
		for (int i = topIndex; i <= num; i++)
		{
			Rectangle itemRectangle = GetItemRectangle(i);
			if (itemRectangle.Bottom > 0 && itemRectangle.Top < ClientSize.Height)
			{
				DrawItemState drawItemState = DrawItemState.None;
				if (i == selectedIndex)
				{
					drawItemState |= DrawItemState.Selected;
				}
				DrawItemEventArgs e2 = new DrawItemEventArgs(e.Graphics, Font, itemRectangle, i, drawItemState);
				this.DrawItem?.Invoke(this, e2);
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
		{
			int num = IndexFromPoint(e.Location);
			if (num >= 0 && num != selectedIndex)
			{
				SelectedIndex = num;
			}
			Focus();
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (topIndex > MaxTop)
		{
			topIndex = MaxTop;
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if (keys == Keys.Up || keys == Keys.Down || keys == Keys.Home || keys == Keys.End || keys == Keys.Next || keys == Keys.Prior)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		int count = items.Count;
		if (count == 0)
		{
			return;
		}
		int num;
		if (e.KeyCode == Keys.Down)
		{
			num = Math.Min(count - 1, (selectedIndex < 0) ? 0 : (selectedIndex + 1));
		}
		else if (e.KeyCode == Keys.Up)
		{
			num = Math.Max(0, (selectedIndex < 0) ? 0 : (selectedIndex - 1));
		}
		else if (e.KeyCode == Keys.Home)
		{
			num = 0;
		}
		else if (e.KeyCode == Keys.End)
		{
			num = count - 1;
		}
		else if (e.KeyCode == Keys.Next)
		{
			num = Math.Min(count - 1, (selectedIndex < 0) ? 0 : (selectedIndex + VisibleRows));
		}
		else if (e.KeyCode == Keys.Prior)
		{
			num = Math.Max(0, (selectedIndex < 0) ? 0 : (selectedIndex - VisibleRows));
		}
		else
		{
			return;
		}
		SelectedIndex = num;
		if (selectedIndex >= 0)
		{
			if (selectedIndex < topIndex)
			{
				TopIndex = selectedIndex;
			}
			else if (selectedIndex >= topIndex + VisibleRows)
			{
				TopIndex = selectedIndex - VisibleRows + 1;
			}
		}
	}
}
