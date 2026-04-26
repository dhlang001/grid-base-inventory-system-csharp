using Godot;
using Godot.Collections;

namespace GridBaseInventorySystem;

/// <summary>
/// 装备槽视图
/// </summary>
[Tool]
public partial class EquipmentSlotView : Control
{
	/// <summary>
	/// 装备槽的绘制状态：正常、可用、不可用
	/// </summary>
	public enum SlotState
	{
		/// <summary>
		/// 正常
		/// </summary>
		Normal,
		/// <summary>
		/// 可用
		/// </summary>
		Avilable,
		/// <summary>
		/// 不可用
		/// </summary>
		Invilable
	}

	/// <summary>
	/// 装备槽名称，如果重复则展示同意来源的数据
	/// </summary>
	[Export] public string SlotName { get; set; } = GBIS_CSharp.DefaultSlotName;

	private int _baseSize = 32;
	/// <summary>
	/// 基础大小（格子大小）
	/// </summary>
	[Export]
	public int BaseSize
	{
		get => _baseSize;
		set { _baseSize = value; RecalculateSize(); }
	}

	private int _columns = 2;
	/// <summary>
	/// 列数（仅显示，与物品大小无关）
	/// </summary>
	[Export]
	public int Columns
	{
		get => _columns;
		set { _columns = value; RecalculateSize(); }
	}

	private int _rows = 2;
	/// <summary>
	/// 行数（仅显示，与物品大小无关）
	/// </summary>
	[Export]
	public int Rows
	{
		get => _rows;
		set { _rows = value; RecalculateSize(); }
	}

	private Texture2D _background;
	/// <summary>
	/// 背景图片
	/// </summary>
	[Export]
	public Texture2D Background
	{
		get => _background;
		set { _background = value; QueueRedraw(); }
	}

	private Color _avilableColor = Colors.Green * 0.3f;
	/// <summary>
	/// 可用时的颜色（推荐半透明）
	/// </summary>
	[Export]
	public Color AvilableColor
	{
		get => _avilableColor;
		set { _avilableColor = value; QueueRedraw(); }
	}

	private Color _invilableColor = Colors.DarkRed * 0.3f;
	/// <summary>
	/// 不可用时的颜色（推荐半透明）
	/// </summary>
	[Export]
	public Color InvilableColor
	{
		get => _invilableColor;
		set { _invilableColor = value; QueueRedraw(); }
	}

	/// <summary>
	/// 可以装备的物品类型，对应 ItemData.type
	/// </summary>
	[Export] public Array<string> AvilableTypes { get; set; } = new() { "ANY" };

	/// <summary>
	/// 物品容器
	/// </summary>
	private Control _itemContainer;
	/// <summary>
	/// 物品视图
	/// </summary>
	private ItemView _itemView;
	/// <summary>
	/// 当前绘制状态
	/// </summary>
	private SlotState _currentState = SlotState.Normal;

	/// <summary>
	/// 是否为空
	/// </summary>
	/// <returns></returns>
	public bool IsEmpty() => _itemView == null;

	/// <summary>
	/// 刷新装备槽显示
	/// </summary>
	public void Refresh()
	{
		ClearSlot();
		var slotData = GBIS_CSharp.Instance.EquipmentSlotService.GetSlot(SlotName);
		if (slotData != null)
		{
			var itemData = slotData.EquippedItem;
			if (itemData != null)
			{
				OnItemEquipped(SlotName, itemData);
			}
		}
	}

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			CallDeferred(MethodName.RecalculateSize);
			return;
		}

		if (string.IsNullOrEmpty(SlotName))
		{
			GD.PushError("Slot must have a name.");
			return;
		}

		var ret = GBIS_CSharp.Instance.EquipmentSlotService.RegistSlot(SlotName, AvilableTypes);
		if (ret == false)
			return;

		if (Visible)
			GBIS_CSharp.Instance.OpenedEquipmentSlots.Add(SlotName);

		MouseFilter = MouseFilterEnum.Pass;
		InitItemContainer();
		GBIS_CSharp.Instance.SigSlotItemEquipped += OnItemEquipped;
		GBIS_CSharp.Instance.SigSlotItemUnequipped += OnItemUnequipped;
		GBIS_CSharp.Instance.SigSlotRefresh += Refresh;
		MouseEntered += OnSlotHover;
		MouseExited += OnSlotLoseHover;

		VisibilityChanged += OnVisibleChangedSlot;

		CallDeferred(MethodName.Refresh);
	}

	private void OnVisibleChangedSlot()
	{
		if (IsVisibleInTree())
			GBIS_CSharp.Instance.OpenedEquipmentSlots.Add(SlotName);
		else
			GBIS_CSharp.Instance.OpenedEquipmentSlots.Remove(SlotName);
	}

	/// <summary>
	/// 高亮
	/// </summary>
	private void OnSlotHover()
	{
		if (GBIS_CSharp.Instance.MovingItemService.MovingItem == null)
		{
			var itemData = GBIS_CSharp.Instance.EquipmentSlotService.GetSlot(SlotName)?.EquippedItem;
			if (itemData != null)
				GBIS_CSharp.Instance.ItemFocusService.FocusItem(itemData, SlotName);
			return;
		}
		if (GBIS_CSharp.Instance.MovingItemService.MovingItem is EquipmentData)
		{
			GBIS_CSharp.Instance.MovingItemService.MovingItemView.BaseSize = _baseSize;
			bool isAvilable = GBIS_CSharp.Instance.EquipmentSlotService.GetSlot(SlotName).IsItemAvilable(GBIS_CSharp.Instance.MovingItemService.MovingItem);
			_currentState = (isAvilable && IsEmpty()) ? SlotState.Avilable : SlotState.Invilable;
		}
		else
		{
			_currentState = SlotState.Invilable;
		}
		QueueRedraw();
	}

	/// <summary>
	/// 失去高亮
	/// </summary>
	private void OnSlotLoseHover()
	{
		_currentState = SlotState.Normal;
		GBIS_CSharp.Instance.ItemFocusService.ItemLoseFocus();
		QueueRedraw();
	}

	/// <summary>
	/// 监听穿装备
	/// </summary>
	/// <param name="slotName"></param>
	/// <param name="itemData"></param>
	private void OnItemEquipped(string slotName, ItemData itemData)
	{
		if (slotName != SlotName)
			return;

		_itemView = DrawItem(itemData);
		_itemContainer.AddChild(_itemView);
		_currentState = SlotState.Normal;
		QueueRedraw();
	}

	/// <summary>
	/// 监听脱装备
	/// </summary>
	/// <param name="slotName"></param>
	/// <param name="itemData"></param>
	private void OnItemUnequipped(string slotName, ItemData itemData)
	{
		if (slotName != SlotName)
			return;

		ClearSlot();
	}

	/// <summary>
	/// 绘制装备
	/// </summary>
	/// <param name="itemData"></param>
	/// <returns></returns>
	private ItemView DrawItem(ItemData itemData)
	{
		var item = new ItemView(itemData, _baseSize);
		var center = Size / 2 - item.Size / 2;
		item.Position = center;
		return item;
	}

	/// <summary>
	/// 清空装备槽显示（仅清空显示，与数据无关）
	/// </summary>
	private void ClearSlot()
	{
		if (_itemView != null)
		{
			_itemView.QueueFree();
			_itemView = null;
		}
	}

	/// <summary>
	/// 初始化物品容器
	/// </summary>
	private void InitItemContainer()
	{
		_itemContainer = new Control();
		AddChild(_itemContainer);
	}

	public override void _Draw()
	{
		base._Draw();

		// 绘制装备槽背景
		if (Background != null)
		{
			DrawTextureRect(Background, new Rect2(0, 0, Columns * _baseSize, Rows * _baseSize), false);
			switch (_currentState)
			{
				case SlotState.Avilable:
					DrawRect(new Rect2(0, 0, Columns * _baseSize, Rows * _baseSize), AvilableColor);
					break;
				case SlotState.Invilable:
					DrawRect(new Rect2(0, 0, Columns * _baseSize, Rows * _baseSize), InvilableColor);
					break;
			}
		}
		else
		{
			DrawRect(new Rect2(0, 0, Columns * _baseSize, Rows * _baseSize), InvilableColor * 10);
		}
	}

	/// <summary>
	/// 重新计算大小
	/// </summary>
	private void RecalculateSize()
	{
		var newSize = new Vector2(Columns * _baseSize, Rows * _baseSize);
		if (Size != newSize)
			Size = newSize;
		QueueRedraw();
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);

		// 点击动作处理
		if (@event.IsActionPressed(GBIS_CSharp.Instance.InputClick))
		{
			GBIS_CSharp.Instance.ItemFocusService.ItemLoseFocus();
			if (GBIS_CSharp.Instance.MovingItemService.MovingItem != null && IsEmpty())
			{
				GBIS_CSharp.Instance.EquipmentSlotService.EquipMovingItem(SlotName);
			}
			else if (GBIS_CSharp.Instance.MovingItemService.MovingItem == null && !IsEmpty())
			{
				GBIS_CSharp.Instance.EquipmentSlotService.MoveItem(SlotName, _baseSize);
				OnSlotHover();
			}
		}
		// 使用动作处理
		else if (@event.IsActionPressed(GBIS_CSharp.Instance.InputUse) && !IsEmpty())
		{
			GBIS_CSharp.Instance.EquipmentSlotService.Unequip(SlotName);
		}
	}
}
