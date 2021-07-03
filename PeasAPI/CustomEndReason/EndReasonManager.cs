﻿using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PeasAPI;
using UnityEngine;

namespace PeasAPI.CustomEndReason
{
    public class EndReasonManager
    {
        public static GameOverReason CustomGameOverReason = (GameOverReason) (255);

        public static Color Color;

        public static List<GameData.PlayerInfo> Winners;

        public static string Stinger;

        public static void Reset()
        {
            Color = Color.clear;
            Winners = null;
            Stinger = null;
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        private class EndGameManager_SetEverythingUp
        {
            private static readonly Color GhostColor = new(1f, 1f, 1f, 0.5f);

            public static bool Prefix(EndGameManager __instance)
            {
                if (TempData.EndReason != CustomGameOverReason)
                    return true;

                List<WinningPlayerData> _winners = new List<WinningPlayerData>();
                foreach (var winner in Winners)
                {
                    _winners.Add(new WinningPlayerData(winner));
                }

                __instance.DisconnectStinger = Stinger switch
                {
                    "crew" => __instance.CrewStinger,
                    "impostor" => __instance.ImpostorStinger,
                    _ => __instance.DisconnectStinger
                };

                __instance.WinText.text = "Defeat";
                __instance.WinText.color = Palette.ImpostorRed;
                foreach (var winner in Winners)
                {
                    if (winner.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        __instance.WinText.text = "Victory";
                        __instance.WinText.color = Palette.CrewmateBlue;
                    }
                }

                __instance.BackgroundBar.material.color = Color;

                for (int i = 0; i < _winners.Count; i++)
                {
                    var winner = _winners[i];
                    int oddness = (i + 1) / 2;
                    PoolablePlayer player = Object.Instantiate(__instance.PlayerPrefab, __instance.transform);
                    var transform = player.transform;
                    transform.localPosition = new Vector3(
                        0.8f * (i % 2 == 0 ? -1 : 1) * oddness * 1 - oddness * 0.035f,
                        EndGameManager.BaseY - 0.25f + oddness * 0.1f,
                        (i == 0 ? -8 : -1) + oddness * 0.01f
                    ) * 1.25f;
                    float scale = 1f - oddness * 0.075f;
                    var scaleVec = new Vector3(scale, scale, scale) * 1.25f;
                    transform.localScale = scaleVec;
                    if (winner.IsDead)
                    {
                        player.Body.sprite = __instance.GhostSprite;
                        player.SetDeadFlipX(i % 2 == 1);
                        player.HatSlot.color = GhostColor;
                    }
                    else
                    {
                        player.SetFlipX(i % 2 == 0);
                        DestroyableSingleton<HatManager>.Instance.SetSkin(player.SkinSlot, winner.SkinId); // SetSkin
                    }

                    PlayerControl.SetPlayerMaterialColors(winner.ColorId, player.Body);
                    player.HatSlot.SetHat(winner.HatId, winner.ColorId);
                    PlayerControl.SetPetImage(winner.PetId, winner.ColorId, player.PetSlot);
                    player.NameText.text = winner.Name;
                    player.NameText.transform.localScale = global::Extensions.Inv(scaleVec);
                    player.NameText.gameObject.SetActive(false);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
        private class EndGameManager_Start
        {
            public static void Prefix(EndGameManager __instance)
            {
                if (TempData.EndReason != CustomGameOverReason)
                    return;

                __instance.DisconnectStinger = Stinger switch
                {
                    "crew" => __instance.CrewStinger,
                    "impostor" => __instance.ImpostorStinger,
                    _ => __instance.DisconnectStinger
                };

                __instance.WinText.text = "Defeat";
                foreach (var winner in Winners)
                {
                    if (winner.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                        __instance.WinText.text = "Victory";
                }

                __instance.WinText.color = Color;
                __instance.BackgroundBar.material.color = Color;
            }

            public static void Postfix(EndGameManager __instance)
            {
                if (TempData.EndReason != CustomGameOverReason)
                    return;

                Reset();
            }
        }
    }
}