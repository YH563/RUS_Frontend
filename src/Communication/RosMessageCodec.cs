using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SRC.Communication
{
    public static class RosMessageCodec
    {
        private static readonly JsonSerializerOptions _serializeOptions;
        private static readonly JsonSerializerOptions _deserializeOptions;

        // 话题 → 消息类型的注册表
        private static readonly Dictionary<string, System.Type> _topicTypeMap = new();

        static RosMessageCodec()
        {
            // 序列化选项：保持原样，不缩进，不添加任何多态信息
            _serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // 反序列化选项：使用自定义转换器实现自动 Msg 类型识别
            _deserializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                Converters = { new RosBridgeMessageConverter(_topicTypeMap) }
            };
        }

        /// <summary>
        /// 注册话题对应的消息类型
        /// </summary>
        public static void RegisterTopic<T>(string topic) where T : class
        {
            _topicTypeMap[topic] = typeof(T);
        }

        /// <summary>
        /// 序列化任何消息（包括外层 RosBridgeMessage 或内部具体消息）
        /// </summary>
        public static string Encode<T>(T message) => JsonSerializer.Serialize(message, _serializeOptions);

        /// <summary>
        /// 解码 WebSocket 外层消息（自动将 PublishMessage.Msg 转为强类型）
        /// </summary>
        public static RosBridgeMessage DecodeBridgeMessage(string json)
            => JsonSerializer.Deserialize<RosBridgeMessage>(json, _deserializeOptions);
    }

    /// <summary>
    /// 自定义转换器：根据 op 字段区分 PublishMessage / SubscribeMessage，
    /// 对于 PublishMessage 再根据 topic 动态解析 msg 字段的具体类型。
    /// </summary>
    internal class RosBridgeMessageConverter : JsonConverter<RosBridgeMessage>
    {
        private readonly IReadOnlyDictionary<string, System.Type> _topicTypeMap;

        public RosBridgeMessageConverter(IReadOnlyDictionary<string, System.Type> topicTypeMap)
        {
            _topicTypeMap = topicTypeMap;
        }

        public override RosBridgeMessage Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            // 先解析整个 JSON 到 JsonDocument，以便多次读取
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            // 获取 op 字段
            if (!root.TryGetProperty("op", out JsonElement opElement))
                throw new JsonException("Missing 'op' field");

            string op = opElement.GetString();
            switch (op)
            {
                case OpField.Publish:
                    return ParsePublishMessage(root, options);
                case OpField.Subscribe:
                    return ParseSubscribeMessage(root, options);
                default:
                    throw new JsonException($"Unknown op: {op}");
            }
        }

        private RosBridgeMessage ParsePublishMessage(JsonElement root, JsonSerializerOptions options)
        {
            // 提取 topic
            if (!root.TryGetProperty("topic", out JsonElement topicElement))
                throw new JsonException("Missing 'topic' in publish message");
            string topic = topicElement.GetString();

            // 提取 msg 原始 JSON
            if (!root.TryGetProperty("msg", out JsonElement msgElement))
                throw new JsonException("Missing 'msg' in publish message");

            // 根据 topic 查找目标类型
            System.Type targetType = null;
            if (_topicTypeMap.TryGetValue(topic, out var registeredType))
                targetType = registeredType;
            else
                targetType = typeof(object); // 未知类型则保留为 JsonElement（反序列化 object 默认会变成 JsonElement）

            // 反序列化 msg 为目标类型
            object msgObj;
            if (targetType == typeof(object))
            {
                // 保持为 JsonElement，后续可手动转换
                msgObj = msgElement.Clone();
            }
            else
            {
                // 使用当前 options（但需避免递归，创建新的序列化器来反序列化 msg 部分）
                // 注意：直接使用 options 没问题，因为转换器只注册给 Ros2BridgeMessage，不会影响内部 msg 的反序列化
                msgObj = JsonSerializer.Deserialize(msgElement.GetRawText(), targetType, options);
            }

            // 构造 PublishMessage 对象
            return new PublishMessage
            {
                Topic = topic,
                Msg = msgObj
            };
        }

        private RosBridgeMessage ParseSubscribeMessage(JsonElement root, JsonSerializerOptions options)
        {
            if (!root.TryGetProperty("topic", out JsonElement topicElement))
                throw new JsonException("Missing 'topic' in subscribe message");
            if (!root.TryGetProperty("type", out JsonElement typeElement))
                throw new JsonException("Missing 'type' in subscribe message");

            return new SubscribeMessage
            {
                Topic = topicElement.GetString(),
                Type = typeElement.GetString()
            };
        }

        public override void Write(Utf8JsonWriter writer, RosBridgeMessage value, JsonSerializerOptions options)
        {
            // 序列化直接使用默认方式（因为序列化时不需要特殊处理）
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}