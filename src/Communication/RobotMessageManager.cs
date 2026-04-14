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

        // 存储每个消息类型对应的多播委托
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        // 私有构造函数
        private RobotMessageManager() { }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T">消息类型（必须继承自 RobotMessage）</typeparam>
        /// <param name="callback">收到消息时的回调函数</param>
        public void Subscribe<T> (Action<T> callback) where T : RobotMessage
        {
            Type type = typeof(T);
            if (_handlers.ContainsKey(type))
                _handlers[type] = (Action<T>)_handlers[type] + callback;
            else
                _handlers[type] = callback;
            Logger.Logger.Debug($"订阅消息类型: {type.Name}");
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="callback">要移除的回调函数</param>
        public void Unsubscribe<T>(Action<T> callback) where T : RobotMessage
        {
            Type type = typeof(T);
            if (_handlers.TryGetValue(type, out var del))
            {
                // 从多播委托中移除回调
                var newDel = (Action<T>)del - callback;
                if (newDel == null)
                    _handlers.Remove(type);
                else
                    _handlers[type] = newDel;
                Logger.Logger.Debug($"取消订阅消息类型: {type.Name}");
            }
        }

        public void Send<T> (T message) where T : RobotMessage
        {
            Type type = typeof (T);
            if (_handlers.TryGetValue(type, out var del))
            {
                var callback = del as Action<T>;
                callback?.Invoke(message);
            }
        }
    }
}