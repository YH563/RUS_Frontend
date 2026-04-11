using Godot;
using System;

namespace SRC.Communication
{
    /// <summary>
    /// ROS Bridge 客户端 - 挂载到场景节点上使用
    /// </summary>
    public partial class RosBridgeClient : Node
    {
        // 配置参数
        [Export]
        public string RosBridgeUrl = "ws://127.0.0.1:9090";

        private WebSocketPeer _webSocketPeer = new WebSocketPeer();
        private bool _isConnected = false;  // 是否已经成功建立连接
        private bool _isConnecting = false;  // 是否在连接过程中
        

    }
}