using Godot;
using System;

namespace RUS_Frontend.src.Robot
{
    public partial class RobotClass : Node
    {
        // URDF 文件路径
        [Export(PropertyHint.File, "*.urdf")]
        public string URDFPath { get; set; }
        // URDF 解析结果保存
        public RobotData RobotData { get; set; } = new RobotData();

        public override void _Ready()
        {
            
        }

        /// <summary>
        /// 加载模型
        /// </summary>
        public void LoadModel()
        {
            if (!string.IsNullOrEmpty(URDFPath))
            {
                RobotData = RobotParser.Parse(URDFPath);
                RobotData.Show();
            }
        }
    }
}

