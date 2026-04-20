using System.Text.Json.Serialization;

namespace SRC.Communication
{
    /// <summary>
    /// 标识消息类型的op字段
    /// </summary>
    public static class OpField
    {
        public const string Publish = "publish";  // 发布话题
        public const string Subscribe = "subscribe";  // 订阅话题
        public const string Unsubscribe = "unsubscribe";  // 取消订阅话题
    }

    /// <summary>
    /// 桥接消息基类
    /// </summary>
    public abstract record RosBridgeMessage
    {
        [JsonPropertyName("op")]
        public abstract string Op { get; }
    }

    /// <summary>
    /// 发布话题
    /// </summary>
    public record PublishMessage : RosBridgeMessage
    {
        public override string Op => OpField.Publish;
        [JsonPropertyName("topic")]
        public required string Topic { get; init; }
        [JsonPropertyName("msg")]
        public required object Msg { get; init; }
    }

    /// <summary>
    /// 订阅话题
    /// </summary>
    public record SubscribeMessage : RosBridgeMessage
    {
        public override string Op => OpField.Subscribe;
        [JsonPropertyName("topic")]
        public required string Topic { get; init; }
        [JsonPropertyName("type")]
        public required string Type { get; init; } 
    }

    /// <summary>
    /// 关节角度消息
    /// </summary>
    public record JointAnglesMsg
    {
        [JsonPropertyName("joints")]
        public double[] Joints { get; init; }

        [JsonPropertyName("stamp")]
        public TimeMsg Stamp { get; init; }
    }

    /// <summary>
    /// 时间戳消息
    /// </summary>
    public record TimeMsg
    {
        [JsonPropertyName("sec")] public int Sec { get; init; }
        [JsonPropertyName("nanosec")] public uint Nanosec { get; init; }
    }

    /// <summary>
    /// 位置信息
    /// </summary>
    public record Position
    {
        [JsonPropertyName("x")] public double X { get; init; }
        [JsonPropertyName("y")] public double Y { get; init; }
        [JsonPropertyName("z")] public double Z { get; init; }
    }

    /// <summary>
    /// 旋转信息，四元数
    /// </summary>
    public record Orientation
    {
        [JsonPropertyName("x")] public double X { get; init; }
        [JsonPropertyName("y")] public double Y { get; init; }
        [JsonPropertyName("z")] public double Z { get; init; }
        [JsonPropertyName("w")] public double W { get; init; }
    }
}