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
                        default:
                            break;
                    }
                    return;
                }
                else
                {

                    string[] param = string.Join(" ", oper.Skip(1).ToArray()).Split('/');

                    switch (oper[0])
                    {
                        case "add":
                            if (param.Length == 1)
                            {
                                JailTime.Instance.addPlayer(caller, param[0], "");
                            }
                            else
                            {
                                JailTime.Instance.addPlayer(caller, param[0], param[1]);
                            }
                            break;
                        case "remove":
                            JailTime.Instance.removePlayer(caller, param[0]);
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
                            JailTime.Instance.setJail(caller, param[0], caller.Position);
                            break;
                        case "unset":
                            JailTime.Instance.unsetJail(caller, param[0]);
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
