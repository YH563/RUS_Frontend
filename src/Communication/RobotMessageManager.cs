using Godot;
using SRC.Logger;
using System;
using System.Collections.Generic;

namespace SRC.Communication
{
    /// <summary>
    /// 消息中心，采用单例模式
    /// </summary>
    public class RobotMessageManager
    {
        // 使用单例模式
        private static readonly RobotMessageManager _instance = new RobotMessageManager();
        public static RobotMessageManager Instance => _instance;

        // 存储接收到消息类型后的多播委托
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        private readonly HashSet<string> _subscribedTopics = new HashSet<string>();  // 已订阅的话题，用HashSet保存，避免重复
        private readonly List<SubscribeMessage> _pending = new List<SubscribeMessage>();  // 暂存订阅的话题
        private bool _connected = false;

        // 私有构造函数
        private RobotMessageManager() { }

        /// <summary>
        /// 注册接收到消息后的回调函数
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="handler">收到消息时的回调函数</param>
        public void Register<T> (Action<T> handler)
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;
            Logger.Logger.Debug($"注册消息类型: {type.Name} ，其回调函数为 {handler.Method.Name}");
        }

        /// <summary>
        /// 取消注册接收到消息后的回调函数
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="handler">要移除的回调函数</param>
        public void Unregister<T>(Action<T> handler)
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                // 从多播委托中移除回调
                var newDel = Delegate.Remove(existing, handler);
                if (newDel == null)
                    _handlers.Remove(type);
                else
                    _handlers[type] = newDel;
                Logger.Logger.Debug($"取消注册消息类型: {type.Name} ，其回调函数为 {handler.Method.Name}");
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息内容</param>
        /// <param name="topicName">话题名称</param>
        public void Send(object msg, string topicName)
        {
            if (msg == null) return;
            Type runtimeType = msg.GetType();
            if (_handlers.TryGetValue(runtimeType, out var del) && _subscribedTopics.Contains(topicName))
            {
                // DynamicInvoke 调用多播委托
                del.DynamicInvoke(msg);
            }
            else
            {
                Logger.Logger.Error($"未注册 {runtimeType.Name}", this);
            }
        }

        /// <summary>
        /// 添加订阅的话题，未建立连接时，进行暂存
        /// </summary>
        /// <param name="msg"></param>
        public void SubscribeTopic(SubscribeMessage msg)
        {
            if (_subscribedTopics.Contains(msg.Topic))
                return;   // 已订阅过，忽略
            _subscribedTopics.Add(msg.Topic);
            if (_connected)
                SendSubscription(msg);
            else
                _pending.Add(msg);
        }

        /// <summary>
        /// 连接建立后调用，发送所有暂存的订阅
        /// </summary>
        public void OnConnectionEstablished()
        {
            if (_connected) return;
            _connected = true;
            foreach (var msg in _pending)
                SendSubscription(msg);
            _pending.Clear();
        }

        /// <summary>
        /// 发布订阅消息
        /// </summary>
        /// <param name="msg"></param>
        private void SendSubscription(SubscribeMessage msg)
        {
            RosBridgeClient.Instance.SendSubscribeMessage(msg);
        }
    }
}