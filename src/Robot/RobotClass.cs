using Godot;
using System;

namespace RUS_Frontend.src.Robot
{
    public partial class RobotClass : Node
    {
        // URDF 解析器对象
        private RobotParser parser_;
        [Export(PropertyHint.File, "*.urdf")]
        public string urdfPath_ { get; set; } = string.Empty;

        // URDF 解析结果保存

        public override void _Ready()
        {
            if (!string.IsNullOrEmpty(urdfPath_))
            {
                parser_.urdfPath_ = urdfPath_;
                parser_.UrdfParser(this);
            }
            else
            {
                GD.PrintErr("加载URDF失败！");
            }

        }
    }
}

