using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Godot.RenderingDevice;
using SRC.Logger;
using SRC.Communication;


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

        // 六个关节的旋转角度
        [Export(PropertyHint.Range, "-270, 270")]
        public float J1Angle { get; set; } = 0;
        [Export(PropertyHint.Range, "-270, 270")]
        public float J2Angle { get; set; } = 0;
        [Export(PropertyHint.Range, "-270, 270")]
        public float J3Angle { get; set; } = 0;
        [Export(PropertyHint.Range, "-270, 270")]
        public float J4Angle { get; set; } = 0;
        [Export(PropertyHint.Range, "-270, 270")]
        public float J5Angle { get; set; } = 0;
        [Export(PropertyHint.Range, "-270, 270")]
        public float J6Angle { get; set; } = 0;

        // 保存六个关节上次旋转的角度
        private float[] _lastJointAngles = {0, 0, 0, 0, 0, 0};

        // URDF 解析结果保存
        private RobotData _robotData;
        private List<Node3D> jointNodes;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            LoadModel();
            Logger.Logger.Info("机械臂模型已创建！", this);
            // 订阅角度信息
            RobotMessageManager.Instance.Subscribe<RobotAnglesMessage>(OnAnglesChanged);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_robotData == null) return;
            
            // 遍历所有关节进行旋转
            for (int i = 0; i < 6; i++)
            {
                // 使用反射获取关节值
                string propertyName = $"J{i+1}Angle";
                PropertyInfo prop = GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                float jointAngle = (float)prop.GetValue(this);
                // 检查是否超出限制
                Limit limit = _robotData.GetLimit(_robotData.Joints[i].Name);
                if (Mathf.DegToRad(jointAngle) < limit.Lower || Mathf.DegToRad(jointAngle) > limit.Upper)
                {
                    jointAngle = Mathf.Clamp(jointAngle, Mathf.RadToDeg((float)limit.Lower), Mathf.RadToDeg((float)limit.Upper));
                    prop.SetValue(this, jointAngle);
                    Logger.Logger.Warn($"关节 j {i + 1} 旋转角度到达限制！", this);
                }

                float angleDelta = jointAngle - _lastJointAngles[i];
                if (angleDelta != 0)
                {
                    RotateJoint(i, Mathf.DegToRad(angleDelta));
                    _lastJointAngles[i] = jointAngle;
                }
            }
        }

        private void OnAnglesChanged(RobotAnglesMessage message)
        {
            // 遍历设置角度
            for (int i = 0; i < 6; i++)
            {
                string propertyName = $"J{i + 1}Angle";
                PropertyInfo prop = GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                prop.SetValue(this, message.JointAngels[i]);
            }
        }

        /// <summary>
        /// 旋转对应的关节
        /// </summary>
        /// <param name="jointIndex">关节序号，第一个为0</param>
        /// <param name="angle">旋转角度差值，弧度制</param>
        /// <returns></returns>
        private void RotateJoint(int jointIndex, float angle)
        {
            // 查找对应关节获取旋转轴
            var joint = _robotData.SearchJoint(jointNodes[jointIndex + 1].Name);
            Vector3 axis = new Vector3(joint.Axis.X, joint.Axis.Z, joint.Axis.Y);
            axis.Normalized();

            // 查找对应子节点
            var jointNode = jointNodes[jointIndex + 1];
            if (jointNode != null)
            {
                jointNode.RotateObjectLocal(axis, angle);
            }
            else
            {
                Logger.Logger.Error($"关节 {jointNodes[jointIndex + 1].Name} 未找到", this);
            }
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
                Logger.Logger.Error("URDF 文件路径为空！", this);
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
            jointNodes = new List<Node3D>();
            jointNodes.Add(rootNode);
            foreach(var joint in _robotData.Joints)
            {
                jointNodes.Add(CreateJointAndChild(joint.Name));
            }
            foreach(var (node, index) in jointNodes.Select((node, index) => (node, index)) )
            {
                if (index == 0)
                    AddChild(node);
                else
                    jointNodes[index - 1].AddChild(node);
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