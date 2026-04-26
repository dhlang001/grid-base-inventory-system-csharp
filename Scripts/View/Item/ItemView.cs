using Godot;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 物品视图，控制物品的绘制
/// </summary>
public partial class ItemView : Control, IController
{

	/// <summary>
	/// 堆叠数字的字体
	/// </summary>
	public Font StackNumFont { get; set; }
	/// <summary>
	/// 堆叠数字的字体大小
	/// </summary>
	public int StackNumFontSize { get; set; }
	/// <summary>
	/// 堆叠数字的边距
	/// </summary>
	public int StackNumMargin { get; set; } = 4;
	/// <summary>
	/// 堆叠数字的颜色
	/// </summary>
	public Color StackNumColor { get; set; } = Colors.White;

	/// <summary>
	/// 物品数据
	/// </summary>
	public ItemData Data { get; set; }

	private int _baseSize;
	/// <summary>
	/// 绘制基础大小（格子大小）
	/// </summary>
	public int BaseSize
	{
		get => _baseSize;
		set { _baseSize = value; CallDeferred(MethodName.RecalculateSize); }
	}

	/// <summary>
	/// 是否正在移动
	/// </summary>
	private bool _isMoving = false;
	/// <summary>
	/// 移动偏移量（坐标）
	/// </summary>
	private Vector2I _movingOffset = Vector2I.Zero;

	public ItemView() { }

	public ItemView(ItemData data, int baseSize, Font stackNumFont = null, int stackNumFontSize = 16, int stackNumMargin = 2, Color stackNumColor = default)
	{
		Data = data;
		_baseSize = baseSize;
		StackNumFont = stackNumFont ?? GetThemeFont("font");
		StackNumFontSize = stackNumFontSize;
		StackNumMargin = stackNumMargin;
		StackNumColor = stackNumColor == default ? Colors.Wheat : stackNumColor;
		RecalculateSize();
		MouseFilter = MouseFilterEnum.Ignore;
		if (data.Material != null)
		{
			Material = (ShaderMaterial)data.Material.Duplicate();
		}
		else if (this.GetModel<GBIS_Model>().ItemMaterial != null)
		{
			Material = (ShaderMaterial)this.GetModel<GBIS_Model>().ItemMaterial.Duplicate();
		}
		data.SigRefresh += QueueRedraw;
	}

	public IArchitecture GetArchitecture()
	{
		return GameArchitecture.Interface;
	}

	/// <summary>
	/// 重写计算大小
	/// </summary>
	public void RecalculateSize()
	{
		if (Data == null) return;
		Size = new Vector2(Data.Columns * _baseSize, Data.Rows * _baseSize);
		QueueRedraw();
	}

	/// <summary>
	/// 移动
	/// </summary>
	/// <param name="offset"></param>
	public void Move(Vector2I offset)
	{
		_isMoving = true;
		_movingOffset = offset;
	}

	public override void _Draw()
	{
		base._Draw();

		//绘制物品
		if (Data == null) return;
		if (Data.Icon != null)
		{
			DrawTextureRect(Data.Icon, new Rect2(Vector2.Zero, Size), false);
		}
		if (Data is StackableData stackable)
		{
			var font = StackNumFont ?? GetThemeFont("font");
			var text = stackable.CurrentAmount.ToString();
			var textSize = font.GetStringSize(text, HorizontalAlignment.Right, -1, StackNumFontSize);
			var pos = new Vector2(
				Size.X - textSize.X - StackNumMargin,
				Size.Y - font.GetDescent(StackNumFontSize) - StackNumMargin
			);
			DrawString(font, pos, text, HorizontalAlignment.Right, -1, StackNumFontSize, StackNumColor);
		}
		if (Material != null && Data != null)
		{
			foreach (var paramName in Data.ShaderParams.Keys)
			{
				((ShaderMaterial)Material).SetShaderParameter(paramName, Data.ShaderParams[paramName]);
			}
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// 跟随鼠标
		if (_isMoving)
		{
			GlobalPosition = GetGlobalMousePosition() - new Vector2(_baseSize * _movingOffset.X, _baseSize * _movingOffset.Y) - new Vector2(_baseSize / 2f, _baseSize / 2f);
		}
	}
}
