using Godot;
using SRC.Logger;
using System.Collections.Generic;

namespace SRC.UI.InputControl
{
    public partial class InputManager : Node
    {
        // 单例模式
        public static InputManager Instance { get; private set; }

        private List<InputRegion> _regions = new List<InputRegion>();  // 存放已中注册的区域
        private InputRegion _activeRegion;  // 激活区域

        public override void _Ready()
        {
            if (Instance == null)
                Instance = this;
            else
                QueueFree();
        }

        /// <summary>
        /// 注册目标区域
        /// </summary>
        /// <param name="region"></param>
        public void RegisterRegion(InputRegion region)
        {
            if (!_regions.Contains(region))
                _regions.Add(region);
        }

        /// <summary>
        /// 注销目标区域
        /// </summary>
        /// <param name="region"></param>
        public void UnregisterRegion(InputRegion region)
        {
            _regions.Remove(region);
            if (_activeRegion == region)
                SetActiveRegion(null);
        }

        /// <summary>
        /// 设置激活区域
        /// </summary>
        /// <param name="newRegion"></param>
        private void SetActiveRegion(InputRegion region)
        {
            if (_activeRegion == region) return;
            _activeRegion = region;
        }

        public override void _Input(InputEvent e)
        {
            // 只在鼠标事件时更新激活区域（避免每帧遍历）
            if (e is InputEventMouse)
            {
                Vector2 mousePos = GetViewport().GetMousePosition();
                InputRegion newActive = null;
                // 后注册的优先（上层），所以逆序遍历
                for (int i = _regions.Count - 1; i >= 0; i--)
                {
                    if (_regions[i].IsMouseOver(mousePos))
                    {
                        newActive = _regions[i];
                        break;
                    }
                }
                SetActiveRegion(newActive);
            }

            // 转发给当前激活区域
            if (_activeRegion != null && _activeRegion.HandleInput(e))
            {
                GetViewport().SetInputAsHandled();
            }
        }
    }
}