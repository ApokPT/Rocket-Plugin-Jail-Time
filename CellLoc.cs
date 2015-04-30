using System.Xml.Serialization;
using UnityEngine;

namespace ApokPT.RocketPlugins
{
    public class CellLoc
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("x")]
        public double X { get; set; }
        [XmlAttribute("y")]
        public double Y { get; set; }
        [XmlAttribute("z")]
        public double Z { get; set; }

        private CellLoc() { }

        public CellLoc(string name, double x, double y, double z)
        {
            Name = name;
            X = x;
            Y = y;
            Z = z;
        }
    }
}
