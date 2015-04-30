
using UnityEngine;
namespace ApokPT.RocketPlugins
{
    class Cell
    {
        public Vector3 Location { get; private set; }
        public string Name { get; private set; }

        public Cell(string name, Vector3 loc)
        {
            Location = loc;
            Name = name;
        }

    }
}
