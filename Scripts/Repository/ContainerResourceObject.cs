using Godot;
using Godot.Collections;
using QFramework;

namespace GridBaseInventorySystem;

/// <summary>
/// 背包数据库，管理 ContainerData 的存取
/// </summary>
public partial class ContainerResourceObject : Resource
{

	/// <summary>
	/// 所有背包数据
	/// </summary>
	[Export] public Godot.Collections.Dictionary<string, ContainerData> ContainerDataMap { get; set; } = new();
	/// <summary>
	/// 所有背包的快速移动关系
	/// </summary>
	[Export] public Godot.Collections.Dictionary<string, Array<string>> QuickMoveRelationsMap { get; set; } = new();

}
