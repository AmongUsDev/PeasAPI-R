﻿using Hazel;
using PeasAPI.Roles;
using Reactor.Networking;
using UnityEngine;

namespace PeasAPI
{
    public static class Extensions
    {
        /// <summary>
        /// Gets a <see cref="PlayerControl"/> from it's id
        /// </summary>
        public static PlayerControl GetPlayer(this byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Gets a <see cref="GameData.PlayerInfo"/> from it's id
        /// </summary>
        public static GameData.PlayerInfo GetPlayerInfo(this byte id)
        {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.PlayerId == id)
                    return player;
            }
            return null;
        }

        /// <summary>
        /// Gets the text color from a <see cref="Color"/>
        /// </summary>
        public static string GetTextColor(this Color color)
        {
            var r = Mathf.RoundToInt(color.r * 255f).ToString("X2");
            
            var g = Mathf.RoundToInt(color.g * 255f).ToString("X2");
            
            var b = Mathf.RoundToInt(color.b * 255f).ToString("X2");
            
            var a = Mathf.RoundToInt(color.a * 255f).ToString("X2");

            return $"<color=#{r}{g}{b}{a}>";
        }

        #region Roles

        /// <summary>
        /// Gets the role of a <see cref="PlayerControl"/>
        /// </summary>
        public static BaseRole GetRole(this PlayerControl player)
        {
            foreach (var _role in RoleManager.Roles)
            {
                if (_role.Members.Contains(player.PlayerId))
                    return _role;
            }

            return null;
        }
        
        /// <summary>
        /// Gets the role of a <see cref="GameData.PlayerInfo"/>
        /// </summary>
        public static BaseRole GetRole(this GameData.PlayerInfo player)
        {
            foreach (var _role in RoleManager.Roles)
            {
                if (_role.Members.Contains(player.PlayerId))
                    return _role;
            }

            return null;
        }

        /// <summary>
        /// Checks if a <see cref="PlayerControl"/> is a certain role
        /// </summary>
        public static bool IsRole(this PlayerControl player, BaseRole role) => player.GetRole() == role;

        #nullable enable
        /// <summary>
        /// Gets the role of a <see cref="PlayerControl"/>
        /// </summary>
        public static T? GetRole<T>(this PlayerControl player) where T : BaseRole
            => player.GetRole() as T;

        /// <summary>
        /// Checks if a <see cref="PlayerControl"/> is a certain role
        /// </summary>
        public static bool IsRole<T>(this PlayerControl player) where T : BaseRole
            => player.GetRole<T>() != null;

        /// <summary>
        /// Sets the role of a <see cref="PlayerControl"/>
        /// </summary>
        public static void SetRole(this PlayerControl player, BaseRole? role)
        {
            foreach (var _role in RoleManager.Roles)
            {
                if (_role != role)
                    _role.Members.Remove(player.PlayerId);
            }

            if (role != null)
            {
                role.Members.Add(player.PlayerId);
            }
        }

        /// <summary>
        /// Sets the role of a <see cref="PlayerControl"/>
        /// </summary>
        public static void RpcSetRole(this PlayerControl player, BaseRole? role)
        {
            Rpc<SetRoleRpc>.Instance.Send(new SetRoleRpc.Data(player, role));

            player.SetRole(role);
        }

        public static bool IsOnSameTeam(this PlayerControl player, PlayerControl otherPlayer)
        {
            var role = player.GetRole();
            var otherRole = otherPlayer.GetRole();

            if (role != null)
            {
                switch (role.Team)
                {
                    case Team.Alone:
                        return false;
                    case Team.Role:
                        return role.Id == otherRole.Id;
                    case Team.Crewmate:
                        if (otherRole != null)
                            return otherRole.Team == Team.Crewmate;
                        else
                            return !otherPlayer.Data.IsImpostor;
                    case Team.Impostor:
                        if (otherRole != null)
                            return otherRole.Team == Team.Impostor;
                        else
                            return otherPlayer.Data.IsImpostor;
                }
            }
            else if (otherRole == null)
            {
                return player.Data.IsImpostor == otherPlayer.Data.IsImpostor;
            }
            else
            {
                switch (otherRole.Team)
                {
                    case Team.Alone:
                        return false;
                    case Team.Role:
                        return false;
                    case Team.Crewmate:
                        return !otherPlayer.Data.IsImpostor;
                    case Team.Impostor:
                        return otherPlayer.Data.IsImpostor;
                }
            }

            return false;
        }

        #endregion Roles
    }
}