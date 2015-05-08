using Rocket.Logging;
using Rocket.RocketAPI;
using System;
using System.Linq;

namespace ApokPT.RocketPlugins
{
    public class JailTimeCommand : IRocketCommand
    {
        public void Execute(RocketPlayer caller, string command)
        {

            if (!caller.IsAdmin && !JailTime.Instance.Configuration.Enabled) return;

            if (String.IsNullOrEmpty(command.Trim()))
            {
                RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_help"));
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
                            RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_help_add"));
                            break;
                        case "remove":
                            RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_help_remove"));
                            break;
                        case "list":
                            RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_help_list"));
                            break;
                        case "set":
                            RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_help_set"));
                            break;
                        case "unset":
                            RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_help_unset"));
                            break;
                        case "teleport":
                            RocketChatManager.Say(caller, JailTime.Instance.Translate("jailtime_help_teleport"));
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
                                JailTime.Instance.addPlayer(caller, string.Join(" ", param.ToArray()));
                            }
                            else if (param.Length == 2)
                            {
                                // Arrest player in random cell for defined time - /jail add apok 20
                                JailTime.Instance.addPlayer(caller, param[0], "", Convert.ToUInt32(param[1]));
                            }
                            else
                            {
                                // Arrest player in specific cell for defined time - /jail add apok 20 cell 1
                                JailTime.Instance.addPlayer(caller, param[0], string.Join(" ", param.Skip(2).ToArray()), Convert.ToUInt32(param[1]));
                            }
                            break;
                        case "remove":
                            JailTime.Instance.removePlayer(caller, string.Join(" ", param.ToArray()));
                            break;
                        case "list":
                            switch (param[0])
                            {
                                case "players":
                                    JailTime.Instance.listPlayers(caller);
                                    break;
                                case "cells":
                                    JailTime.Instance.listJails(caller);
                                    break;
                            }
                            break;
                        case "set":
                            JailTime.Instance.setJail(caller, string.Join(" ", param.ToArray()), caller.Position);
                            break;
                        case "unset":
                            JailTime.Instance.unsetJail(caller, string.Join(" ", param.ToArray()));
                            break;
                        case "teleport":
                            JailTime.Instance.teleportToCell(caller, string.Join(" ", param.ToArray()));
                            break;
                        default:
                            break;
                    }

                }
            }
        }

        public string Help
        {
            get { return "Send players to jail!"; }
        }

        public string Name
        {
            get { return "jail"; }
        }

        public bool RunFromConsole
        {
            get { return false; }
        }
    }
}
