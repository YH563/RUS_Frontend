using Godot;

public partial class UiMain : Control
{
	// 声明子模块引用，不需要[Export]
	public TopBar TopBar;
	public PatientInfoPanel PatientInfoPanel;
	public RobotControlPanel RobotControlPanel;
	public ToolBarPanel ToolBarPanel;
	public UltrasoundPanel UltrasoundPanel;

	public override void _Ready()
	{
		// 👇 完全匹配你当前的场景树路径，自动绑定所有子节点
		// MainCanvas 是父节点，后面跟着子节点的名字，和你场景树完全一致
		TopBar = GetNode<TopBar>("MainCanvas/TopBar");
		PatientInfoPanel = GetNode<PatientInfoPanel>("MainCanvas/LeftPanel/PatientInfoPanel");
		RobotControlPanel = GetNode<RobotControlPanel>("MainCanvas/LeftPanel/RobotContr"); // 注意：和你场景树的节点名完全一致
		ToolBarPanel = GetNode<ToolBarPanel>("MainCanvas/MiddlePanel/TopBar"); // 你MiddlePanel下的TopBar就是ToolBarPanel
		UltrasoundPanel = GetNode<UltrasoundPanel>("MainCanvas/RightPanel/Ultrasound");

		// 初始化界面数据（完全保留你之前的逻辑）
		TopBar.UpdatePatient("张建国", "123456");
		TopBar.UpdateQueue(3, "李四");
		TopBar.UpdateRobotStatus("机器人待命（安全）", true);
	}
}
