using Godot;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 格子视图，用于绘制格子
/// </summary>
public partial class BaseGridView : Control, IController
{

	/// <summary>
	/// 默认边框颜色
	/// </summary>
	public static readonly Color DefaultBorderColor = Colors.Gray;
	/// <summary>
	/// 默认空置颜色
	/// </summary>
	public static readonly Color DefaultEmptyColor = Colors.DarkSlateGray;
	/// <summary>
	/// 默认占用颜色
	/// </summary>
	public static readonly Color DefaultTakenColor = Colors.LightSlateGray;
	/// <summary>
	/// 默认冲突颜色
	/// </summary>
	public static readonly Color DefaultConflictColor = Colors.IndianRed;
	/// <summary>
	/// 默认可用颜色
	/// </summary>
	public static readonly Color DefaultAvilableColor = Colors.SteelBlue;

	private GridState _state = GridState.Empty;
	/// <summary>
	/// 当前绘制状态
	/// </summary>
	public GridState State
	{
		get => _state;
		set { _state = value; QueueRedraw(); }
	}

	/// <summary>
	/// 格子ID（格子在当前背包的坐标）
	/// </summary>
	public Vector2I GridId { get; set; } = Vector2I.Zero;
	/// <summary>
	/// 偏移（格子存储物品时的偏移坐标，如：一个2*2的物品，这个格子是它右下角的格子，则 offset = [1,1]）
	/// </summary>
	public Vector2I Offset { get; set; } = Vector2I.Zero;
	/// <summary>
	/// 是否被占用
	/// </summary>
	public bool HasTaken { get; set; } = false;

	/// <summary>
	/// 格子大小
	/// </summary>
	protected int _size = 32;
	/// <summary>
	/// 边框大小
	/// </summary>
	private int _borderSize = 1;
	/// <summary>
	/// 边框颜色
	/// </summary>
	private Color _borderColor = DefaultBorderColor;
	/// <summary>
	/// 空置颜色
	/// </summary>
	private Color _emptyColor = DefaultEmptyColor;
	/// <summary>
	/// 占用颜色
	/// </summary>
	private Color _takenColor = DefaultTakenColor;
	/// <summary>
	/// 冲突颜色
	/// </summary>
	private Color _conflictColor = DefaultConflictColor;
	/// <summary>
	/// 可用颜色
	/// </summary>
	private Color _avilableColor = DefaultAvilableColor;

	/// <summary>
	/// 所属的背包View
	/// </summary>
	protected BaseContainerView _containerView;

	public BaseGridView() { }

	public BaseGridView(BaseContainerView containerView, Vector2I gridId, int size, int borderSize, Color borderColor, Color emptyColor, Color takenColor, Color conflictColor, Color avilableColor)
	{
		_avilableColor = avilableColor;
		_containerView = containerView;
		GridId = gridId;
		_size = size;
		_borderSize = borderSize;
		_borderColor = borderColor;
		_emptyColor = emptyColor;
		_takenColor = takenColor;
		_conflictColor = conflictColor;
		CustomMinimumSize = new Vector2(_size, _size);
	}

	public IArchitecture GetArchitecture()
	{
		return GameArchitecture.Interface;
	}

	/// <summary>
	/// 占用格子
	/// </summary>
	/// <param name="inOffset"></param>
	public void Taken(Vector2I inOffset)
	{
		HasTaken = true;
		Offset = inOffset;
		State = GridState.Taken;
	}

	/// <summary>
	/// 释放格子
	/// </summary>
	public void Release()
	{
		HasTaken = false;
		Offset = Vector2I.Zero;
		State = GridState.Empty;
	}

	public override void _Ready()
	{
		base._Ready();

		MouseFilter = MouseFilterEnum.Pass;
		MouseEntered += () => _containerView?.GridHover(GridId);
		MouseExited += () => _containerView?.GridLoseHover(GridId);
	}

	public override void _Draw()
	{
		base._Draw();

		DrawRect(new Rect2(0, 0, _size, _size), _borderColor);
		int innerSize = _size - _borderSize * 2;
		Color backgroundColor = _state switch
		{
			GridState.Empty => _emptyColor,
			GridState.Taken => _takenColor,
			GridState.Conflict => _conflictColor,
			GridState.Avilable => _avilableColor,
			_ => _emptyColor
		};
		DrawRect(new Rect2(_borderSize, _borderSize, innerSize, innerSize), backgroundColor);
	}
}
