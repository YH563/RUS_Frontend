using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace RUS_Frontend.src.Robot
{
    public class Origin
    {
        public Vector3 xyz_ { get; set; } =new Vector3();
        public Vector3 rpy_ { get; set; } = new Vector3();
    }

    public class Inertial
    {
        public Origin Origin { get; set; } = new Origin();
        public double Mass { get; set; }
        public double Ixx { get; set; }
        public double Ixy { get; set; }
        public double Ixz { get; set; }
        public double Iyy { get; set; }
        public double Iyz { get; set; }
        public double Izz { get; set; }
    }


}
