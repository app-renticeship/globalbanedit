﻿using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Commands;
using SDG.Unturned;
using System;
using Steamworks;
using System.Collections.Generic;
using Rocket.Core.Steam;

namespace Apprentice.Globalbanedit
{
    public class CommandBan : IRocketCommand
    {
        public string Help
        {
            get { return "Banns a player"; }
        }

        public string Name
        {
            get { return "gban"; }
        }

        public string Syntax
        {
            get { return "<player> [reason] [duration]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "globalban.ban" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            try
            {
                if (command.Length == 0 || command.Length > 3)
                {
                    UnturnedChat.Say(caller, GlobalBan.Instance.Translate("command_generic_invalid_parameter"));
                    return;
                }

                bool isOnline = false;

                CSteamID steamid;
                string charactername = null;


                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
                ulong? otherPlayerID = command.GetCSteamIDParameter(0);
                if (otherPlayer == null || otherPlayer.CSteamID.ToString() == "0" || caller != null && otherPlayer.CSteamID.ToString() == caller.Id)
                {
                    KeyValuePair<CSteamID, string> player = GlobalBan.GetPlayer(command[0]);
                    if (player.Key.ToString() != "0")
                    {
                        steamid = player.Key;
                        charactername = player.Value;
                    }
                    else
                    {
                        if (otherPlayerID != null)
                        {
                            steamid = new CSteamID(otherPlayerID.Value);
                            Profile playerProfile = new Profile(otherPlayerID.Value);
                            charactername = playerProfile.SteamID;
                        }
                        else
                        {
                            UnturnedChat.Say(caller, GlobalBan.Instance.Translate("command_generic_player_not_found"));
                            return;
                        }
                    }
                }
                else
                {
                    isOnline = true;
                    steamid = otherPlayer.CSteamID;
                    charactername = otherPlayer.CharacterName;
                }

                string adminName = "Console";
                if (caller != null) adminName = caller.ToString();

                if (command.Length == 3)
                {
                    int duration = 0;
                    if (int.TryParse(command[2], out duration))
                    {
                        uint i = Convert.ToUInt32(duration); //same behavior as checked
                        Provider.ban(steamid, command[1], i);
                        GlobalBan.Instance.Database.BanPlayer(charactername, steamid.ToString(), adminName, command[1], duration);
                        UnturnedChat.Say(GlobalBan.Instance.Translate("command_ban_public_reason", charactername, command[1]));
                        if (isOnline)
                            Provider.kick(steamid, command[1]);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, GlobalBan.Instance.Translate("command_generic_invalid_parameter"));
                        return;
                    }
                }
                else if (command.Length == 2)
                {
                    Provider.ban(steamid, command[1], 0);
                    GlobalBan.Instance.Database.BanPlayer(charactername, steamid.ToString(), adminName, command[1], 0);
                    UnturnedChat.Say(GlobalBan.Instance.Translate("command_ban_public_reason", charactername, command[1]));
                    if (isOnline)
                        Provider.kick(steamid, command[1]);
                }
                else
                {
                    Provider.ban(steamid, "", 0);
                    GlobalBan.Instance.Database.BanPlayer(charactername, steamid.ToString(), adminName, "", 0);
                    UnturnedChat.Say(GlobalBan.Instance.Translate("command_ban_public", charactername));
                    if (isOnline)
                        Provider.kick(steamid, GlobalBan.Instance.Translate("command_ban_private_default_reason"));
                }

            }
            catch (System.Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}