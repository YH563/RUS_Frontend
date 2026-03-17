using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace RUS_Frontend.src.Robot
{
    /// <summary>
    /// URDF 解析器
    /// </summary>
    public static class RobotParser
    {
        /// <summary>
        /// 解析 URDF 文件，生成结构化的机械臂数据对象
        /// </summary>
        /// <param name="URDFPath"> URDF 文件路径 </param>
        /// <returns></returns>
        public static RobotData Parse(string URDFPath)
        {
            RobotData robotData = new RobotData();
            XDocument URDFData = XDocument.Load(URDFPath);
            XElement robotRoot = URDFData.Root;

            // 获取机器人名称
            robotData.Name = robotRoot.Attribute("name").Value;
            GD.Print(robotData.Name);

            // 加载连杆
            robotData.Links = ParseLinks(robotRoot);
            // 加载关节
            robotData.Joints = ParseJoints(robotRoot);

            return robotData;
        }

        /// <summary>
        /// 解析 link 对象
        /// </summary>
        /// <param name="robotRoot"> URDF 文件解析结果的根节点 </param>
        /// <returns></returns>
        private static List<Link> ParseLinks(XElement robotRoot)
        {
            var linkObjects = robotRoot.Elements("link").ToList();
            var links = new List<Link>();
            foreach (var linkObject in linkObjects)
            {
                var link = new Link();
                link.Name = linkObject.Attribute("name").Value;

                var visualObject = linkObject.Element("visual");
                link.Visual.Origin.XYZ = ParseVector3(visualObject.Element("origin").Attribute("xyz")?.Value);
                link.Visual.Origin.RPY = ParseVector3(visualObject.Element("origin").Attribute("rpy")?.Value);
                link.Visual.Geometry.MeshFilename = visualObject.Element("geometry").Element("mesh").Attribute("filename")?.Value;
                link.Visual.Material.Rgba = ParseColor(visualObject.Element("material").Element("color").Attribute("rgba")?.Value);
                links.Add(link);
            }

            return links;
        }

        /// <summary>
        /// 解析 joint 对象
        /// </summary>
        /// <param name="robotRoot"> URDF 文件解析结果的根节点 </param>
        /// <returns></returns>
        private static List<Joint> ParseJoints(XElement robotRoot)
        {
            var jointObjects = robotRoot.Elements("joint").ToList();
            var joints = new List<Joint>();
            foreach (var jointObject in jointObjects)
            {
                Joint joint = new Joint();
                joint.Name = jointObject.Attribute("name").Value;
                joint.Type = jointObject.Attribute("type").Value;
                joint.Origin.XYZ = ParseVector3(jointObject.Element("origin").Attribute("xyz")?.Value);
                joint.Origin.RPY = ParseVector3(jointObject.Element("origin").Attribute("rpy")?.Value);
                joint.Parent = jointObject.Element("parent").Attribute("link")?.Value;
                joint.Child = jointObject.Element("child").Attribute("link")?.Value;
                joint.Axis = ParseVector3(jointObject.Element("axis").Attribute("xyz").Value);
                joint.Limit.Lower = double.Parse(jointObject.Element("limit").Attribute("lower")?.Value);
                joint.Limit.Upper = double.Parse(jointObject.Element("limit").Attribute("upper")?.Value);
                joints.Add(joint);
            }

            return joints;
        }

        /// <summary>
        /// 将 origin 中的 xyz 或 rpy 转换为 Vector3
        /// </summary>
        /// <param name="xyzObject"> xyz 或 rpy 解析得到的字符串 </param>
        /// <returns></returns>
        private static Vector3 ParseVector3(string xyzObject)
        {
            string pattern = @"^\d+ \d+ \d+$";
            bool isMatch = Regex.IsMatch(xyzObject, pattern);
            if (!isMatch)
            {
                GD.PrintErr("xyz 或 rpy 解析失败！");
                return Vector3.Zero;
            }
            else
            {
                Vector3 xyzData = new Vector3();
                string[] numberParts = xyzObject.Split(' ');
                xyzData.X = float.Parse(numberParts[0]);
                xyzData.Y = float.Parse(numberParts[1]);
                xyzData.Z = float.Parse(numberParts[2]);
                return xyzData;
            }
        }

        /// <summary>
        /// 将 material 中的 color 转换为 Vector4
        /// </summary>
        /// <param name="colorObject"> color 解析得到的字符串 </param>
        /// <returns></returns>
        private static Vector4 ParseColor(string colorObject)
        {
            string pattern = @"^\d+ \d+ \d+ \d$";
            bool isMatch = Regex.IsMatch(colorObject, pattern);
            if (!isMatch)
            {
                GD.PrintErr("color 解析失败！");
                return Vector4.Zero;
            }
            else
            {
                Vector4 colorData = new Vector4();
                string[] numberParts = colorObject.Split(' ');
                colorData.X = float.Parse(numberParts[0]);
                colorData.Y = float.Parse(numberParts[1]);
                colorData.Z = float.Parse(numberParts[2]);
                colorData.W = float.Parse(numberParts[3]);
                return colorData;
            }
        }
    }
}
