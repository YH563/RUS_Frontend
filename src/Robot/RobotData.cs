using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace RUS_Frontend.src.Robot
{
    /// <summary>
    /// 原点变换
    /// </summary>
    public class Origin
    {
        public Vector3 XYZ { get; set; } =new Vector3();
        public Vector3 RPY { get; set; } = new Vector3();
    }

    /// <summary>
    /// 网络模型
    /// </summary>
    public class Geometry
    {
        public string MeshFilename { get; set; }
    }

    /// <summary>
    /// 材质
    /// </summary>
    public class Material
    {
        public Vector4 Rgba { get; set; } // [r, g, b, a]
    }

    /// <summary>
    /// 可视化元素
    /// </summary>
    public class Visual
    {
        public Origin Origin { get; set; } = new Origin();
        public Geometry Geometry { get; set; } = new Geometry();
        public Material Material { get; set; } = new Material();
    }

    /// <summary>
    /// 关节运动范围
    /// </summary>
    public class Limit
    {
        public double Lower { get; set; }
        public double Upper { get; set; }
    }

    /// <summary>
    /// 连杆
    /// </summary>
    public class Link
    {
        public string Name { get; set; }
        public Visual Visual { get; set; } = new Visual();
    }

    /// <summary>
    /// 关节
    /// </summary>
    public class Joint
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Origin Origin { get; set; } = new Origin();
        public string Parent { get; set; }
        public string Child { get; set; }
        public Vector3 Axis { get; set; } = new Vector3();
        public Limit Limit { get; set; } = new Limit();
    }

    /// <summary>
    /// 机器人数据类型
    /// </summary>
    public class RobotData
    {
        public string Name { get; set; }
        public List<Link> Links { get; set; } = new List<Link>();
        public List<Joint> Joints { get; set; } = new List<Joint>();

        /// <summary>
        /// 进行坐标变换，以保证模型显示正确
        /// </summary>
        public void OriginTransform()
        {

        }

        /// <summary>
        /// 结构化打印机器人数据信息
        /// </summary>
        public void Show()
        {
            // 打印机器人基础信息
            GD.Print("========================================");
            GD.Print($"机器人名称：{Name ?? "未设置"}");
            GD.Print("========================================");


        }

        /// <summary>
        /// 结构化打印机械臂 link 信息
        /// </summary>
        public void LinkShow()
        {
            
        }

    }
}
