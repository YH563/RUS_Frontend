using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SRC.Robot
{
    /// <summary>
    /// 管理机械臂模型的类
    /// </summary>
    [Tool]  // 可以在编辑器下运行ready
    public partial class RobotClass : Node
    {
        // URDF 文件路径
        [Export(PropertyHint.File, "*.urdf")]
        public string URDFPath { get; set; }

        // 模型保存文件路径
        [Export(PropertyHint.Dir)]
        public string MeshDir { get; set; }

        // URDF 解析结果保存
        private RobotData _robotData;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            LoadModel();
        }

        public override void _PhysicsProcess(double delta)
        {
            
        }

        /// <summary>
        /// 旋转对应的关节
        /// </summary>
        /// <param name="jointName">关节名称</param>
        /// <param name="angle">旋转角度，弧度制</param>
        /// <returns></returns>
        public void RotateJoint(string jointName, float angle)
        {
            var joint = _robotData.SearchJoint(jointName);
            Vector3 axis = new Vector3(joint.Axis.X, joint.Axis.Z, joint.Axis.Y);
            axis.Normalized();
            
            // 查找对应子节点
            var jointNode = FindChild(jointName, recursive: true) as Node3D;
            jointNode.Rotate(axis, angle);
        }

        /// <summary>
        /// 加载模型
        /// </summary>
        private void LoadModel()
        {
            if (!string.IsNullOrEmpty(URDFPath))
            {
                _robotData = RobotParser.Parse(URDFPath);
                _robotData.Show();
                BuildRobot();
            }
            else
            {
                GD.PrintErr("URDF 文件路径为空！");
                return;
            }
        }

        /// <summary>
        /// 创建机械臂模型
        /// </summary>
        private void BuildRobot()
        {
            // 将节点级联起来
            string baseLinkName = "base_link";
            Node3D rootNode = CreateJointAndChild(baseLinkName);
            List<Node3D> nodes = new List<Node3D>();
            nodes.Add(rootNode);
            foreach(var joint in _robotData.Joints)
            {
                nodes.Add(CreateJointAndChild(joint.Name));
            }
            foreach(var (node, index) in nodes.Select((node, index) => (node, index)) )
            {
                if (index == 0)
                    AddChild(node);
                else
                    nodes[index - 1].AddChild(node);
            }

        }

        /// <summary>
        /// 创建关节添加子连杆
        /// </summary>
        /// <param name="jointName">关节名称</param>
        /// <returns>返回值为一个存在模型子节点的节点</returns>
        private Node3D CreateJointAndChild(string jointName)
        {
            if (jointName == null)
            {
                GD.Print("未找到对应的关节！");
                return new Node3D();
            }
            else if (jointName == "base_link")
            {
                var link = _robotData.SearchLink(jointName);
                string meshFileName = Path.GetFileNameWithoutExtension(link.Visual.Geometry.MeshFilename);

                // 加载模型
                var meshNode3D = RobotData.FindMeshFile(MeshDir, meshFileName);

                // 创建根节点
                var baseNode = new Node3D();
                baseNode.Name = jointName;
                baseNode.AddChild(meshNode3D);
                meshNode3D.Name = jointName;
                // 添加变换
                baseNode.Transform = RobotData.XyzRpyToTransform(link.Visual.Origin.XYZ, link.Visual.Origin.RPY);
                meshNode3D.Transform = RobotData.XyzRpyToTransform(link.Visual.Origin.XYZ, link.Visual.Origin.RPY);
                return baseNode;
            }
            else
            {
                var joint = _robotData.SearchJoint(jointName);
                var childLink = _robotData.SearchLink(joint.Child);
                string meshFileName = Path.GetFileNameWithoutExtension(childLink.Visual.Geometry.MeshFilename);

                // 加载模型
                var meshNode3D = RobotData.FindMeshFile(MeshDir, meshFileName);

                // 父节点为关节节点，子节点为子连杆的模型
                var jointNode = new Node3D();
                jointNode.Name = jointName;
                jointNode.AddChild(meshNode3D);
                meshNode3D.Name = childLink.Name;
                // 添加变换
                jointNode.Transform = RobotData.XyzRpyToTransform(joint.Origin.XYZ, joint.Origin.RPY);
                meshNode3D.Transform = RobotData.XyzRpyToTransform(childLink.Visual.Origin.XYZ, childLink.Visual.Origin.RPY);
                return jointNode;
            }
        }

    }
}

