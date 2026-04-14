using System.Text.Json.Serialization;

namespace SRC.Communication
{
    // 基类，用于多态识别
    [JsonDerivedType(typeof(RobotMoveMessage), typeDiscriminator: "robot_move")]
    [JsonDerivedType(typeof(RobotStatusMessage), typeDiscriminator: "robot_status")]
    [JsonDerivedType(typeof(RobotAnglesMessage), typeDiscriminator: "robot_angles")]
    public abstract record RobotMessage
    {
        [JsonPropertyName("type")]
        public string Type => GetType().Name;
    }

    public record RobotAnglesMessage : RobotMessage
    {
        public double[] JointAngels { get; init; }
    }


    /// <summary>
    /// 
    /// </summary>
    public record RobotMoveMessage : RobotMessage
    {
        public double[] Joints { get; init; }
        public double Gripper { get; init; }
    }

    /// <summary>
    /// 
    /// </summary>
    public record RobotStatusMessage : RobotMessage
    {
        public double[] CurrentJoints { get; init; }
        public double CurrentGripper { get; init; }
        public bool IsMoving { get; init; }
    }

}