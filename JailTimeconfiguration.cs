using Rocket.RocketAPI;

namespace ApokPT.RocketPlugins
{
    public class JailTimeConfiguration : RocketConfiguration
    {

        public bool Enabled;
        public bool BanOnReconnect;
        public uint BanOnReconnectTime;
        public bool KillInsteadOfTeleport;
        public ulong WalkDistance;

        public RocketConfiguration DefaultConfiguration
        {
            get
            {

                JailTimeConfiguration config = new JailTimeConfiguration();
                config.KillInsteadOfTeleport = false;
                config.BanOnReconnect = false;
                config.BanOnReconnectTime = 0;
                config.WalkDistance = 5;
                config.Enabled = true;
                return config;
            }
        }
    }
}
