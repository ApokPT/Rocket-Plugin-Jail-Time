using Rocket.RocketAPI;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Rocket;
using SDG;
using Steamworks;
using pt.manuelbarbosa.rocketplugin;

namespace pt.manuelbarbosa.rocketplugins
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

    class JailTime : RocketPlugin<JailTimeConfiguration>
    {

        // Singleton

        public static JailTime Instance;

        private Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
        private Dictionary<CSteamID, Cell> players = new Dictionary<CSteamID, Cell>();

        protected override void Load()
        {
            Instance = this;
            if (JailTime.Instance.Configuration.Enabled) {
                Rocket.RocketAPI.Events.RocketPlayerEvents.OnPlayerRevive += RocketPlayerEvents_OnPlayerRevive;
                Rocket.RocketAPI.Events.RocketServerEvents.OnPlayerConnected += RocketServerEvents_OnPlayerConnected;
            }
        }

        private void RocketServerEvents_OnPlayerConnected(RocketPlayer player)
        {

            if (players.ContainsKey(player.CSteamID))
            {

                if (Configuration.BanOnReconnect)
                {
                    players.Remove(player.CSteamID);
                    removePlayerFromJail(player);
                    if (Configuration.BanOnReconnectTime > 0){
                        player.Ban(JailTime.Instance.Translate("jailtime_ban_time", Configuration.BanOnReconnectTime), Configuration.BanOnReconnectTime);
                    }else{
                        player.Ban(JailTime.Instance.Translate("jailtime_ban"), Configuration.BanOnReconnectTime);
                    }
                }
                else
                {
                    movePlayerToJail(player, players[player.CSteamID]);
                    RocketChatManager.Say(player, JailTime.Instance.Translate("jailtime_player_back_msg"));
                }

                
            }
        }

        private void RocketPlayerEvents_OnPlayerRevive(RocketPlayer player, Vector3 position, byte angle)
        {
            if (players.ContainsKey(player.CSteamID))
            {
                movePlayerToJail(player, players[player.CSteamID]);
                RocketChatManager.Say(player, JailTime.Instance.Translate("jailtime_player_back_msg"));
            }
        }

        public void FixedUpdate()
        {
            if (this.Loaded)
            {
                foreach (KeyValuePair<CSteamID, Cell> pl in players)
                {
                    try
                    {
                        if (Vector3.Distance(RocketPlayer.FromCSteamID(pl.Key).Position, pl.Value.Location) > Configuration.WalkDistance)
                        {
                            if (Configuration.KillInsteadOfTeleport)
                            {
                                RocketPlayer.FromCSteamID(pl.Key).Damage(255, RocketPlayer.FromCSteamID(pl.Key).Position, EDeathCause.PUNCH, ELimb.SKULL, RocketPlayer.FromCSteamID(pl.Key).CSteamID);
                            }
                            else
                            {
                                RocketPlayer.FromCSteamID(pl.Key).Teleport(pl.Value.Location, RocketPlayer.FromCSteamID(pl.Key).Rotation);
                            }
                            
                        }
                    }
                    catch
                    {

                    }

                }
            }
        }

        // Private Methods 

        private Cell getCellbyName(string jailName)
        {

            return cells.ContainsKey(jailName) ? cells[jailName] : null;
        }

        private Cell getRandomCell()
        {
            if (cells.Count >= 0)
            {
                List<string> keys = new List<string>(cells.Keys);
                System.Random rand = new System.Random();
                return cells[keys[rand.Next(cells.Count)]];

            }
            return null;
        }


        // Player Methods

        internal void addPlayer(RocketPlayer caller, string playerName, string jailName)
        {
            Cell jail;
            RocketPlayer target = RocketPlayer.FromName(playerName);

            if (target == null)
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_notfound", jailName));
                return;
            }
            else if (players.ContainsKey(target.CSteamID))
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_in_jail", target.CharacterName));
                return;
            }
            else
            {
                if (jailName == "")
                {
                    jail = getRandomCell();
                    if (jail == null)
                    {
                        RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notset", jailName));
                        return;
                    }
                }
                else
                {
                    jail = getCellbyName(jailName);
                    if (jail == null)
                    {
                        RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                        return;
                    }
                }
                players.Add(target.CSteamID, jail);
                target.Damage(255, target.Position, EDeathCause.PUNCH, ELimb.SKULL, target.CSteamID);

                RocketChatManager.Say(target, JailTime.Instance.Translate("jailtime_player_arrest_msg"));
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_arrested", target.CharacterName, jail.Name));
            }
        }

        internal void removePlayer(RocketPlayer caller, string playerName)
        {
            RocketPlayer target = RocketPlayer.FromName(playerName);

            if (target != null && players.ContainsKey(target.CSteamID))
            {
                players.Remove(target.CSteamID);
                removePlayerFromJail(target);
                RocketChatManager.Say(target, JailTime.Instance.Translate("jailtime_player_release_msg"));
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_released", target.CharacterName));
            }
            else
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_notfound", playerName));
                return;
            }
        }

        internal void listPlayers(RocketPlayer caller)
        {
            if (players.Count == 0)
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_list_clear"));
                return;
            }
            else
            {
                string playersString = "";

                foreach (KeyValuePair<CSteamID, Cell> player in players)
                {
                    try 
                    {
                        playersString += RocketPlayer.FromCSteamID(player.Key).CharacterName + " (" + player.Value.Name + "), ";
                    }
                    catch 
                    { 
                    }
                    
                }

                if (playersString != "") playersString = playersString.Remove(playersString.Length - 2) + ".";

                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_list", playersString));
                return;
            }
        }


        // Jail Methods 

        internal void setJail(RocketPlayer caller, string jailName, UnityEngine.Vector3 location)
        {
            if (cells.ContainsKey(jailName))
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_exists", jailName));
                return;
            }
            else
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_set", jailName));
                cells.Add(jailName, new Cell(jailName, location));
            }
        }

        internal void unsetJail(RocketPlayer caller, string jailName)
        {
            if (!cells.ContainsKey(jailName))
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                return;
            }
            else
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_unset", jailName));
                cells.Remove(jailName);
            }
        }

        internal void listJails(RocketPlayer caller)
        {
            if (cells.Count == 0)
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notset"));
                return;
            }
            else
            {
                string jailsString = "";

                foreach (KeyValuePair<string, Cell> jail in cells)
                {
                    jailsString += jail.Value.Name + ", ";
                }

                if (jailsString != "") jailsString = jailsString.Remove(jailsString.Length - 2) + ".";

                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_list", jailsString));
                return;
            }
        }


        // Arrest Methods

        private Timer timer;

        private void movePlayerToJail(RocketPlayer player, Cell jail)
        {
            timer = new System.Threading.Timer(obj =>
            {
                player.Teleport(jail.Location, player.Rotation);
                timer.Dispose();
            }, null, 1000, Timeout.Infinite);
        }

        private void removePlayerFromJail(RocketPlayer player)
        {
            player.Damage(255, player.Position, EDeathCause.PUNCH, ELimb.SKULL, player.CSteamID);
        }

        // Translations

        public override Dictionary<string, string> DefaultTranslations
        {
            get
            {
                return new Dictionary<string, string>(){
                    {"jailtime_jail_notset","No cells set, please use /jail set [name] first!"},
                    {"jailtime_jail_notfound","No cell found named {0}!"},
                    {"jailtime_jail_set","New cell named {0} created where you stand!"},
                    {"jailtime_jail_exists","Cell named {0} already exists!"},
                    {"jailtime_jail_unset","Cell named {0} removed from jail!"},
                    {"jailtime_jail_list","Jail Cells: {0}"},
                    
                    {"jailtime_player_in_jail","Player {0} already in jail!"},
                    {"jailtime_player_arrested","Player {0} was arrested in {1} cell!"},
                    {"jailtime_player_released","Player {0} released from jail!"},
                    
                    {"jailtime_player_list","Players: {0}"},
                    {"jailtime_player_list_clear","Jail cells are getting dusty!"},
                    {"jailtime_player_notfound","No player found named {0}!"},
                    
                    {"jailtime_player_release_msg","You have been arrested!"},
                    {"jailtime_player_arrest_msg","You have been released!"},
                    {"jailtime_player_back_msg","Get back in your cell!"},

                    {"jailtime_help_add","use /jail add <player>/<cell> - to arrest a player, if no <cell> uses a random one"},
                    {"jailtime_help_remove","use /jail remove <player> - to release a player"},
                    {"jailtime_help_list","use /jail list players or /jail list cells"},
                    {"jailtime_help_set","use /jail set <cell> - to set a new jail cell"},
                    {"jailtime_help_unset","use /jail unset <cell> - to delete a jail cell"},
                    {"jailtime_ban","You have been banned for disconnecting while in Jail!"},
                    {"jailtime_ban_time","You have been banned for {0} seconds for disconnecting while in Jail!"}
                };
            }
        }

    }
}
