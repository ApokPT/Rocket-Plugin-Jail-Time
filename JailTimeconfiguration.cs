using Rocket.RocketAPI;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApokPT.RocketPlugins
{
    public class JailTimeConfiguration : RocketConfiguration
    {

        public bool Enabled;
        public bool BanOnReconnect;
        public uint BanOnReconnectTime;
        public uint JailTimeInSeconds;
        public bool KillInsteadOfTeleport;
        public ulong WalkDistance;
        
        [XmlArrayItem(ElementName = "Cell")]
        public List<CellLoc> Cells;

        public RocketConfiguration DefaultConfiguration
        {
            get
            {
                JailTimeConfiguration config = new JailTimeConfiguration();
                config.Cells = new List<CellLoc>() { 
                    new CellLoc("O'Leary 1", -240.5706,34.50486,16.71745),
                    new CellLoc("O'Leary 2", -243.9697,39.25486,-0.4241685),
                    new CellLoc("Charlotte", 71.9969,33.77744,-486.8339),
                    new CellLoc("Stratford", -13.20535,38.89639,665.5002)
                };
                config.KillInsteadOfTeleport = false;
                config.BanOnReconnect = false;
                config.BanOnReconnectTime = 0;
                config.JailTimeInSeconds = 600;
                config.WalkDistance = 5;
                config.Enabled = true;
                return config;
            }
        }
    }
}
