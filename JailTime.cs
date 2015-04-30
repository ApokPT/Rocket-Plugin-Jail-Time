using Rocket.Logging;
using Rocket.RocketAPI;
using SDG;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ApokPT.RocketPlugins
{

    class JailTime : RocketPlugin<JailTimeConfiguration>
    {

        private Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
        private Dictionary<string, Sentence> players = new Dictionary<string, Sentence>();

        // Singleton

        public static JailTime Instance;

        protected override void Load()
        {
            Instance = this;
            if (JailTime.Instance.Configuration.Enabled)
            {
                Rocket.RocketAPI.Events.RocketPlayerEvents.OnPlayerRevive += RocketPlayerEvents_OnPlayerRevive;
                Rocket.RocketAPI.Events.RocketServerEvents.OnPlayerConnected += RocketServerEvents_OnPlayerConnected;
            }
            injectConfiCells();
        }

        private void injectConfiCells()
        {
            foreach (CellLoc cell in Configuration.Cells)
            {
                setJail(null, cell.Name.ToLower(), new Vector3(Convert.ToSingle(cell.X), Convert.ToSingle(cell.Y), Convert.ToSingle(cell.Z)));
            }
        }

        // Events

        private void RocketServerEvents_OnPlayerConnected(RocketPlayer player)
        {

            if (player.IsAdmin || player.Permissions.Contains("jail.immune")) return;

            if (players.ContainsKey(player.ToString()))
            {

                if (Configuration.BanOnReconnect)
                {
                    removePlayerFromJail(player, players[player.ToString()]);
                    players.Remove(player.ToString());
                    if (Configuration.BanOnReconnectTime > 0)
                    {
                        player.Ban(JailTime.Instance.Translate("jailtime_ban_time", Configuration.BanOnReconnectTime), Configuration.BanOnReconnectTime);
                    }
                    else
                    {
                        player.Ban(JailTime.Instance.Translate("jailtime_ban"), Configuration.BanOnReconnectTime);
                    }
                }
                else
                {
                    if (!(players[player.ToString()].End <= DateTime.Now))
                    {
                        movePlayerToJail(player, players[player.ToString()].Cell);
                        RocketChatManager.Say(player, JailTime.Instance.Translate("jailtime_player_back_msg"));
                    }
                }
            }
        }

        private void RocketPlayerEvents_OnPlayerRevive(RocketPlayer player, Vector3 position, byte angle)
        {
            if (player.IsAdmin || player.Permissions.Contains("jail.immune")) return;

            if (players.ContainsKey(player.ToString()))
            {
                movePlayerToJail(player, players[player.ToString()].Cell);
                RocketChatManager.Say(player, JailTime.Instance.Translate("jailtime_player_back_msg"));
            }
        }

        // Fixed Update

        public void FixedUpdate()
        {
            if (this.Loaded && players != null && players.Count != 0)
            {
                foreach (KeyValuePair<string, Sentence> pl in players)
                {
                    if (pl.Value.End <= DateTime.Now)
                    {
                        try
                        {
                            removePlayer(null, pl.Key);
                            break;
                        }
                        catch { }
                    }


                    try
                    {
                        RocketPlayer player = RocketPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(pl.Key)));

                        if (Vector3.Distance(player.Position, pl.Value.Cell.Location) > Configuration.WalkDistance)
                        {
                            if (Configuration.KillInsteadOfTeleport)
                            {
                                player.Damage(255, player.Position, EDeathCause.PUNCH, ELimb.SKULL, player.CSteamID);
                            }
                            else
                            {
                                player.Teleport(pl.Value.Cell.Location, player.Rotation);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning(e.ToString());
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

        internal void addPlayer(RocketPlayer caller, string playerName, string jailName = "", uint jailTime = 0)
        {

            Cell jail;
            RocketPlayer target = RocketPlayer.FromName(playerName);

            if (jailTime == 0) jailTime = Configuration.JailTimeInSeconds;

            if (target == null)
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_notfound", jailName));
                return;
            }
            else if (players.ContainsKey(target.ToString()))
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_in_jail", target.CharacterName));
                return;
            }
            else
            {

                if (target.IsAdmin || target.Permissions.Contains("jail.immune"))
                {
                    RocketChatManager.Say(target, JailTime.Instance.Translate("jailtime_player_immune"));
                    return;
                }
                else if (cells.Count == 0)
                {
                    RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notset", jailName));
                    return;
                }
                else if (jailName == "")
                {
                    jail = getRandomCell();
                }
                else
                {
                    jail = getCellbyName(jailName);
                }

                if (jail == null)
                {
                    RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                    return;
                }

                players.Add(target.ToString(), new Sentence(jail, jailTime, target.Position));
                movePlayerToJail(target, jail);
                target.GiveItem(303, 1);
                target.GiveItem(304, 1);

                RocketChatManager.Say(target, JailTime.Instance.Translate("jailtime_player_arrest_msg", jailTime));
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_arrested", target.CharacterName, jail.Name));
            }
        }

        internal void removePlayer(RocketPlayer caller, string playerName)
        {
            RocketPlayer target;
            try
            {
                target = RocketPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(playerName)));
            }
            catch
            {
                target = RocketPlayer.FromName(playerName);
            }

            if (players.ContainsKey(target.ToString()))
            {
                removePlayerFromJail(target, players[target.ToString()]);
                RocketChatManager.Say(target, JailTime.Instance.Translate("jailtime_player_release_msg"));

                if (caller != null) RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_released", target.CharacterName));
                players.Remove(target.ToString());
            }
            else
            {
                if (caller != null) RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_notfound", playerName));
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

                foreach (KeyValuePair<string, Sentence> player in players)
                {
                    try { playersString += RocketPlayer.FromName(player.Key).CharacterName + " (" + player.Value.Cell.Name + "), "; }
                    catch { }
                }

                if (playersString != "") playersString = playersString.Remove(playersString.Length - 2) + ".";

                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_player_list", playersString));
                return;
            }
        }


        // Jail Methods 

        internal void getLocation(RocketPlayer caller)
        {
            RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_location", caller.Position.x, caller.Position.y, caller.Position.z));
        }

        internal void setJail(RocketPlayer caller, string jailName, UnityEngine.Vector3 location)
        {
            if (caller != null)
            {
                if (cells.ContainsKey(jailName.ToLower()))
                {
                    RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_exists", jailName));
                    return;
                }
                else
                {
                    RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_set", jailName));

                }
            }
            cells.Add(jailName.ToLower(), new Cell(jailName, location));
        }

        internal void unsetJail(RocketPlayer caller, string jailName)
        {
            if (!cells.ContainsKey(jailName.ToLower()))
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                return;
            }
            else
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_unset", jailName));
                cells.Remove(jailName.ToLower());
            }
        }


        internal void teleportToCell(RocketPlayer caller, string jailName)
        {
            if (!cells.ContainsKey(jailName.ToLower()))
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                return;
            }
            else
            {
                caller.Teleport(cells[jailName.ToLower()].Location, caller.Rotation);
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

        private void movePlayerToJail(RocketPlayer player, Cell jail)
        {
            player.Inventory.Clear();
            player.Teleport(jail.Location, player.Rotation);
        }

        private void removePlayerFromJail(RocketPlayer player, Sentence sentence)
        {
            player.Inventory.Clear();
            player.Teleport(sentence.Location, player.Rotation);
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
                    {"jailtime_jail_location","Cell location - x:{0} y:{1} z:{2}"},
                    
                    
                    {"jailtime_player_immune","That player cannot be arrested!"},
                    {"jailtime_player_in_jail","Player {0} already in jail!"},
                    {"jailtime_player_arrested","Player {0} was arrested in {1} cell!"},
                    {"jailtime_player_released","Player {0} released from jail!"},
                    
                    {"jailtime_player_list","Players: {0}"},
                    {"jailtime_player_list_clear","Jail cells are getting dusty!"},
                    {"jailtime_player_notfound","No player found named {0}!"},
                    
                    {"jailtime_player_arrest_msg","You have been arrested for {0} seconds!"},
                    {"jailtime_player_release_msg","You have been released!"},
                    {"jailtime_player_back_msg","Get back in your cell!"},

                    {"jailtime_help","/jail commands: add, remove, set, unset, list, location, teleport"},
                    {"jailtime_help_add","use /jail add <player> <time> <cell> - to arrest a player, if no <cell> uses a random one"},
                    {"jailtime_help_remove","use /jail remove <player> - to release a player"},
                    {"jailtime_help_list","use /jail list players or /jail list cells"},
                    {"jailtime_help_set","use /jail set <cell> - to set a new jail cell"},
                    {"jailtime_help_unset","use /jail unset <cell> - to delete a jail cell"},
                    {"jailtime_help_teleport","use /jail teleport <cell> - to teleport to a cell"},
                    {"jailtime_ban","You have been banned for disconnecting while in Jail!"},
                    {"jailtime_ban_time","You have been banned for {0} seconds for disconnecting while in Jail!"}
                };
            }
        }
    }
}
