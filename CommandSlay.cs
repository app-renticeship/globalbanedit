﻿using Rocket.API;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Apprentice.Globalbanedit
{
    public class CommandSlay : IRocketCommand
    {
        public string Help
        {
            get { return "Banns a player for a year"; }
        }

        public string Name
        {
            get { return "slay"; }
        }

        public string Syntax
        {
            get { return "<player>"; }
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
                return new List<string>() { "globalban.slay" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            SteamPlayer otherSteamPlayer = null;
            SteamPlayerID steamPlayerID = null;

            if (command.Length == 0 || command.Length > 2)
            {
                UnturnedChat.Say(caller, GlobalBan.Instance.Translate("command_generic_invalid_parameter"));
                return;
            }

            bool isOnline = false;
            CSteamID steamid;
            string charactername = null;
            if (!PlayerTool.tryGetSteamPlayer(command[0], out otherSteamPlayer))
            {
                KeyValuePair<CSteamID, string> player = GlobalBan.GetPlayer(command[0]);
                if (player.Key != null)
                {
                    steamid = player.Key;
                    charactername = player.Value;
                }
                else
                {
                    UnturnedChat.Say(caller, GlobalBan.Instance.Translate("command_generic_player_not_found"));
                    return;
                }
            }
            else
            {
                isOnline = true;
                steamid = otherSteamPlayer.playerID.steamID;
                charactername = otherSteamPlayer.playerID.characterName;
            }

            if (command.Length >= 2)
            {
                Provider.ban(steamid, command[1], 31536000);
                GlobalBan.Instance.Database.BanPlayer(charactername, steamid.ToString(), caller.DisplayName, command[1], 31536000);
                UnturnedChat.Say(GlobalBan.Instance.Translate("command_ban_public_reason", charactername, command[1]));
                if (isOnline)
                    Provider.kick(steamPlayerID.steamID, command[1]);
            }
            else
            {
                Provider.ban(steamid, "", 31536000);
                GlobalBan.Instance.Database.BanPlayer(charactername, steamid.ToString(), caller.DisplayName, "", 31536000);
                UnturnedChat.Say(GlobalBan.Instance.Translate("command_ban_public", charactername));
                if (isOnline)
                    Provider.kick(steamPlayerID.steamID, GlobalBan.Instance.Translate("command_ban_private_default_reason"));
            }
        }
    }
}