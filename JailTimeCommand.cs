using Rocket.API;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;

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
            get { return "<player> <jail> [time]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
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
                    "jail"
                };
            }
        }
        public void Execute(IRocketPlayer caller, string[] cmd)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            string command = String.Join(" ", cmd);

            if (!player.IsAdmin && !JailTime.Instance.Configuration.Instance.Enabled) return;

            if (String.IsNullOrEmpty(command.Trim()))
            {
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help"));
                return;
            }
            else
            {
                string[] oper = command.Split(' ');

                if (oper.Length == 1)
                {
                    switch (oper[0])
                    {
                        case "add":
                            UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_add"));
                            break;
                        case "remove":
                            UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_remove"));
                            break;
                        case "list":
                            UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_list"));
                            break;
                        case "set":
                            UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_set"));
                            break;
                        case "unset":
                            UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_unset"));
                            break;
                        case "teleport":
                            UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_help_teleport"));
                            break;
                        default:
                            break;
                    }
                    return;
                }
                else
                {

                    string[] param = string.Join(" ", oper.Skip(1).ToArray()).Split(' ');

                    switch (oper[0])
                    {
                        case "add":
                            if (param.Length == 1)
                            {
                                // Arrest player in random cell for default time - /jail add apok
                                JailTime.Instance.addPlayer(player, string.Join(" ", param.ToArray()));
                            }
                            else if (param.Length == 2)
                            {
                                // Arrest player in random cell for defined time - /jail add apok 20
                                JailTime.Instance.addPlayer(player, param[0], "", Convert.ToUInt32(param[1]));
                            }
                            else
                            {
                                // Arrest player in specific cell for defined time - /jail add apok 20 cell 1
                                JailTime.Instance.addPlayer(player, param[0], string.Join(" ", param.Skip(2).ToArray()), Convert.ToUInt32(param[1]));
                            }
                            break;
                        case "remove":
                            JailTime.Instance.removePlayer(player, string.Join(" ", param.ToArray()));
                            break;
                        case "list":
                            switch (param[0])
                            {
                                case "players":
                                    JailTime.Instance.listPlayers(player);
                                    break;
                                case "cells":
                                    JailTime.Instance.listJails(player);
                                    break;
                            }
                            break;
                        case "set":
                            JailTime.Instance.setJail(player, string.Join(" ", param.ToArray()), player.Position);
                            break;
                        case "unset":
                            JailTime.Instance.unsetJail(player, string.Join(" ", param.ToArray()));
                            break;
                        case "teleport":
                            JailTime.Instance.teleportToCell(player, string.Join(" ", param.ToArray()));
                            break;
                        default:
                            break;
                    }

                }
            }
        }

    }
}
