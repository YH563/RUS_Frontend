using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SRC.Robot
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
        /// 根据名字查找对应的关节
        /// </summary>
        /// <param name="jointName">关节名字</param>
        /// <returns></returns>
        public Joint SearchJoint(string jointName)
        {
            return Joints.FirstOrDefault(j => j.Name == jointName);
        }

        /// <summary>
        /// 根据名字查找对应的连杆
        /// </summary>
        /// <param name="linkName">连杆名称</param>
        /// <returns></returns>
        public Link SearchLink(string linkName)
        {
            return Links.FirstOrDefault(l => l.Name == linkName);
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
            LinkShow();
            JointShow();
        }

        /// <summary>
        /// 从 Xyz，Rpy 转换为 Transform3D 矩阵
        /// </summary>
        /// <param name="xyz">平移量</param>
        /// <param name="rpy">欧拉角旋转量</param>
        /// <returns>Transform3D 矩阵</returns>
        public static Transform3D XyzRpyToTransform(Vector3 xyz, Vector3 rpy)
        {
            Vector3 godotPos = new Vector3(xyz.X, xyz.Z, xyz.Y);
            Basis basis = Basis.FromEuler(rpy);
            return new Transform3D(basis, godotPos);
        }

        /// <summary>
        /// 根据模型文件夹路径与模型文件名称查找模型文件
        /// </summary>
        /// <param name="meshDir">模型文件夹路径</param>
        /// <param name="meshFileName">模型名称</param>
        /// <returns>Node3D 对象</returns>
        public static Node3D FindMeshFile(string meshDir, string meshFileName)
        {
            using var dir = DirAccess.Open(meshDir);
            string findModel = dir?.GetFiles()?
                    .FirstOrDefault(f => !f.StartsWith(".") && Path.GetFileNameWithoutExtension(f) == meshFileName);
            string meshFilePath = string.IsNullOrEmpty(findModel) ? null : Path.Combine(meshDir, findModel).Replace("\\", "/");
            GD.Print(meshFilePath);

            PackedScene modelScene = ResourceLoader.Load<PackedScene>(meshFilePath);
            if (modelScene == null)
            {
                GD.PrintErr($"Failed to load model: {meshFilePath}");
                return null;
            }
            Node3D modelNode = modelScene.Instantiate<Node3D>();
            return modelNode;
        }

        /// <summary>
        /// 结构化打印机械臂 link 信息
        /// </summary>
        public void LinkShow()
        {
            for(int i = 0; i < Links.Count; i++)
            {
                GD.Print($"连杆编号：{i + 1}，连杆名称：{Links[i].Name}");
                GD.Print($"连杆原点变换：x={Links[i].Visual.Origin.XYZ.X} y={Links[i].Visual.Origin.XYZ.Y} z={Links[i].Visual.Origin.XYZ.Z} " +
                    $"r={Links[i].Visual.Origin.RPY.X} p={Links[i].Visual.Origin.RPY.Y} y={Links[i].Visual.Origin.RPY.Z}");
                GD.Print($"连杆网格模型路径：{Links[i].Visual.Geometry.MeshFilename}");
                GD.Print($"连杆颜色：r={Links[i].Visual.Material.Rgba.X} g={Links[i].Visual.Material.Rgba.Y} " +
                    $"b={Links[i].Visual.Material.Rgba.Z} a={Links[i].Visual.Material.Rgba.W}");
                GD.Print();
            }
            GD.Print("========================================");
        }

        /// <summary>
        /// 结构化打印机械臂 joint 信息
        /// </summary>
        public void JointShow()
        {
            for (int i = 0;i < Joints.Count;i++)
            {
                GD.Print($"关节编号：{i + 1}，关节名称：{Joints[i].Name}，关节类型：{Joints[i].Type}");
                GD.Print($"连杆原点变换：x={Joints[i].Origin.XYZ.X} y={Joints[i].Origin.XYZ.Y} z={Joints[i].Origin.XYZ.Z} " +
                    $"r={Joints[i].Origin.RPY.X} p={Joints[i].Origin.RPY.Y} y={Joints[i].Origin.RPY.Z}");
                GD.Print($"父连杆：{Joints[i].Parent}, 子连杆：{Joints[i].Child}");
                GD.Print($"运动轴：{Joints[i].Axis}");
                GD.Print($"关节运动范围为：{Joints[i].Limit.Lower} ~ {Joints[i].Limit.Upper}");
                GD.Print();
            }
            GD.Print("========================================");
        }
    }
}
