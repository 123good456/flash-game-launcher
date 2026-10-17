using System.Windows.Forms;

/// <summary>
/// 分栏容器：去掉分隔条上的焦点虚线框。
/// 分隔条默认是可以获得焦点的（SplitContainer.OnMouseDown 里就会把 ActiveControl 设成自己），
/// 窗口打开时它也可能抢到初始焦点，此时基类 OnPaint 会在分隔条矩形上画一圈焦点虚线框；
/// 深色主题下 ForeColor 是白色，看上去就是分隔条两侧的两条白色细竖线。
/// 这里让分隔条不再参与焦点，并跳过基类的焦点框绘制（背景仍正常绘制），
/// 线条消失，用鼠标拖动分栏不受影响。
/// </summary>
internal class WideSplit : SplitContainer
{
	public WideSplit()
	{
		SetStyle(ControlStyles.Selectable, false);
		TabStop = false;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		// 只画背景，不调用基类实现（基类会额外画焦点虚线框）
		base.OnPaintBackground(e);
	}
}
