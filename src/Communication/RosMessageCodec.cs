using Godot;
using System.Text;
using System.Text.Json;
using System.Linq;
using SRC.Logger;


namespace SRC.Communication
{
    /// <summary>
    /// ROS 消息的通用接口，负责解码传递进来的消息以及编码传递出去的指令
    /// </summary>
    public static class RosMessageCodec
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // 设置小驼峰命名
            WriteIndented = false  // 避免多余空行
        };

        // 消息序列化
        public static string Encode<T>(T message) where T : RobotMessage
            => JsonSerializer.Serialize(message, _options);

        // 消息反序列化
        public static RobotMessage Decode(string json)
            => JsonSerializer.Deserialize<RobotMessage>(json, _options);
    }
}
