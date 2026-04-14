using Godot;
using SRC.Logger;
using System;
using System.Text;

namespace SRC.Communication
{
    /// <summary>
    /// ROS Bridge 客户端
    /// </summary>
    public partial class RosBridgeClient : Node
    {
        // 使用单例模式
        public static RosBridgeClient Instance { get; private set; }
             
        // 配置参数
        [Export]
        public string RosBridgeUrl = "ws://127.0.0.1:9090";  // 服务器地址
        [Export]
        public bool AutoReconnect { get; set; } = true;  // 是否重连

        private WebSocketPeer _webSocketPeer = new WebSocketPeer();
        private double _reconnectDelay = 1.0f;  // 重连延迟
        private double _reconnectTimer = 0.0f;  // 重连的计时器

        /// <summary>
        /// 初始化
        /// </summary>
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;
            ConnectToServer();
        }

        public override void _Process(double delta)
        {
            switch (_webSocketPeer.GetReadyState())
            {
                case WebSocketPeer.State.Connecting:  // 正在连接
                    _webSocketPeer.Poll();
                    break;
                case WebSocketPeer.State.Open:  // 连接完成，正常轮询接收数据
                    _webSocketPeer.Poll();
                    ProcessMessages();  // 处理接受到的消息
                    break;
                case WebSocketPeer.State.Closing:  // 正在关闭
                    _webSocketPeer.Poll();
                    break;
                case WebSocketPeer.State.Closed:  // 连接关闭，处理重连
                    if (AutoReconnect)
                    {
                        _reconnectTimer += delta;
                        if (_reconnectTimer >=  _reconnectDelay)
                        {
                            _reconnectTimer = 0.0f;
                            Logger.Logger.Info("尝试重新连接...", this);
                            ConnectToServer();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void ConnectToServer()
        {
            if (string.IsNullOrEmpty(RosBridgeUrl))
            {
                Logger.Logger.Error("服务器地址为空！请先设置服务器地址！", this);
                return;
            }
            Error err = _webSocketPeer.ConnectToUrl(RosBridgeUrl);
            if (err != Error.Ok)
            {
                Logger.Logger.Error($"连接失败：{err}", this);
                return;
            }
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private void ProcessMessages()
        {
            while(_webSocketPeer.GetAvailablePacketCount() > 0)
            {
                byte[] packet = _webSocketPeer.GetPacket();
                // 简单判断是文本还是二进制
                if (IsLikelyText(packet))
                {
                    string text = Encoding.UTF8.GetString(packet);
                    Logger.Logger.Info($"收到后端数据，为文本信息: {text}", this);
                    // 将处理后的数据进行转发
                    var data = RosMessageCodec.Decode(text);
                    RobotMessageManager.Instance.Send(data);
                }
                else
                {
                    Logger.Logger.Info($"收到二进制数据: {packet.Length} 字节", this);
                }
            }
        }

        /// <summary>
        /// 判断抓包到的数据是字符串还是二进制
        /// </summary>
        /// <param name="data">抓包的数据</param>
        /// <returns>布尔值</returns>
        private bool IsLikelyText(byte[] data)
        {
            // 启发式判断：前256字节中可打印字符占比 > 80%
            int printable = 0;
            int checkLen = Math.Min(data.Length, 256);
            for (int i = 0; i < checkLen; i++)
            {
                byte b = data[i];
                if (b >= 32 || b == 9 || b == 10 || b == 13)
                    printable++;
                else if (b == 0)
                    return false; // 包含空字节，极可能是二进制
            }
            return (double)printable / checkLen > 0.8;
        }

        /// <summary>
        /// 主动关闭连接
        /// </summary>
        public void Disconnect()
        {
            if (_webSocketPeer.GetReadyState() == WebSocketPeer.State.Open)
            {
                _webSocketPeer.Close();
                Logger.Logger.Info("主动关闭连接", this);
            }
        }

        public override void _ExitTree()
        {
            Disconnect();
        }
    }
}