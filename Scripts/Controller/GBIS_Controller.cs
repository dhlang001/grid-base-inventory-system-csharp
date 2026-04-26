using Godot;
using QFramework;
using System;

namespace GridBaseInventorySystem;

/// <summary>
/// 网格背包系统全局入口，需挂载到全局
/// 可使用已拥有的全局Node替代
/// </summary>
[GlobalClass]
public partial class GBIS_Controller : Node, IController
{
	public IArchitecture GetArchitecture()
	{
		return GameArchitecture.Interface;
	}

	public override void _Ready()
	{
		base._Ready();
		this.GetModel<GBIS_Model>().GBIS_BaseNode = this;
	}
}
