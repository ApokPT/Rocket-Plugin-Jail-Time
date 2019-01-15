using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

namespace ApokPT.RocketPlugins
{
    public class JailTimeCommand : IRocketCommand
    {
        public string Name
        {
            get { return "jail"; }
        }

        public string Help
        {
            get { return "Send players to jail!"; }
        }  

        public string Syntax
        {
            get { return "<player> [<jail>] [<time>]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>() { "jt" }; }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>()
                {
                    "jail",
                    "jail.add",
                    "jail.remove",
                    "jail.list",
                    "jail.info",
                    "jail.set",
                    "jail.unset",
                    "jail.teleport",
                    "jail.cuff"
                };
            }
        }

        public void Execute(IRocketPlayer caller, string[] cmd)
        {

            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (!player.IsAdmin && !JailTime.Instance.Configuration.Instance.Enabled) return;

            if (cmd.Length == 0 && player.HasPermission("jail"))
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help"));
                return;
            }
            else if (cmd.Length == 1 && player.HasPermission("jail.add") && cmd[0] == "add")
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_add"));
            }
            else if (cmd.Length == 1 && player.HasPermission("jail.remove") && cmd[0] == "remove")
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_remove"));
            }
            else if (cmd.Length == 1 && player.HasPermission("jail.info") && cmd[0] == "info")
            {
                JailTime.Instance.infoPlayer(player);
            }
            else if (cmd.Length == 1 && player.HasPermission("jail.list") && cmd[0] == "list")
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_list"));
            }
            else if (cmd.Length == 1 && player.HasPermission("jail.set") && cmd[0] == "set")
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_set"));
            }
            else if (cmd.Length == 1 && player.HasPermission("jail.unset") && cmd[0] == "unset")
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_unset"));
            }
            else if (cmd.Length == 1 && player.HasPermission("jail.teleport") && cmd[0] == "teleport")
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_teleport"));
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.add") && cmd[0] == "add")
            {
                JailTime.Instance.addPlayer(player, cmd[1].ToString());
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.remove") && cmd[0] == "remove")
            {
                JailTime.Instance.removePlayer(player, cmd[1].ToString());
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.list") && cmd[0] == "list")
            {
                switch (cmd[1])
                {
                    case "players":
                        JailTime.Instance.listPlayers(player);
                        break;
                    case "cells":
                        JailTime.Instance.listJails(player);
                        break;
                }
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.set") && cmd[0] == "set")
            {
                JailTime.Instance.setJail(player, cmd[1].ToString(), player.Position);
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.unset") && cmd[0] == "unset")
            {
                JailTime.Instance.unsetJail(player, cmd[1].ToString());
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.teleport") && cmd[0] == "teleport")
            {
                JailTime.Instance.teleportToCell(player, cmd[1].ToString());
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.cuff") && cmd[0] == "cuff")
            {
                UnturnedPlayer target = UnturnedPlayer.FromName(cmd[1]);
                JailTime.Instance.Cuff(target);
            }
            else if (cmd.Length == 2 && player.HasPermission("jail.uncuff") && cmd[0] == "uncuff")
            {
                UnturnedPlayer target = UnturnedPlayer.FromName(cmd[1]);
                JailTime.Instance.Uncuff(target);
            }
            else if (cmd.Length == 3 && player.HasPermission("jail.add") && cmd[0] == "add")
            {
                JailTime.Instance.addPlayer(player, cmd[1].ToString(), "", Convert.ToUInt32(cmd[2]));
            }
            else if (cmd.Length == 4 && player.HasPermission("jail.add") && cmd[0] == "add")
            {
                JailTime.Instance.addPlayer(player, cmd[1].ToString(), cmd[2].ToString(), Convert.ToUInt32(cmd[3]));
            }

        }

    }
}
