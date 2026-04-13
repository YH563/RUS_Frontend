using Godot;
public partial class TopBar : HBoxContainer
{
	[Export] public Button PatientSelectBtn { get; set; }
	[Export] public Label QueueLabel { get; set; }
	[Export] public Label RobotStatusLabel { get; set; }
	[Export] public Label DoctorLabel { get; set; }

	public override void _Ready()
	{
		PatientSelectBtn.Pressed += OnPatientSelect;
	}

	private void OnPatientSelect() => GD.Print("打开患者选择窗口");
	public void UpdatePatient(string name, string id) => PatientSelectBtn.Text = $"当前患者：{name} (ID：{id})";
	public void UpdateQueue(int count, string next) => QueueLabel.Text = $"队列待诊：{count} 人（下一位：{next}）";
	public void UpdateRobotStatus(string status, bool isSafe)
	{
		RobotStatusLabel.Text = status;
		RobotStatusLabel.Modulate = isSafe ? new Color(0, 0.7f, 0.16f) : new Color(0.9f, 0.2f, 0.2f);
	}
}
