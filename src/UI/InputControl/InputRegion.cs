using Godot;

namespace SRC.UI.InputControl
{
    /// <summary>
    /// 抽象基类，管理窗口区域
    /// </summary>
    public abstract partial class InputRegion : Node
    {
        /// <summary>
        /// 返回屏幕空间矩形（用于鼠标命中测试）
        /// </summary>
        /// <returns></returns>
        public abstract Rect2 GetScreenRect();

        /// <summary>
        /// 默认命中测试（矩形内），子类可重写为更精确的形状
        /// </summary>
        /// <param name="mouseScreenPos"></param>
        /// <returns></returns>
        public virtual bool IsMouseOver(Vector2 mouseScreenPos)
        {
            return GetScreenRect().HasPoint(mouseScreenPos);
        }

        /// <summary>
        /// 处理输入事件，返回 true 表示事件已被消费
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public abstract bool HandleInput(InputEvent e);
    }
}