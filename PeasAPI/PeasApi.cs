﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using InnerNet;
using PeasAPI.Components;
using Reactor;
using Random = System.Random;

namespace PeasAPI
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class PeasApi : BasePlugin
    {
        public const string Id = "tk.peasplayer.amongus.api";
        public const string Version = "1.0.0";

        public Harmony Harmony { get; } = new Harmony(Id);
        
        public static readonly Random Random = new Random();
        
        public static ManualLogSource Logger { get; private set; }
        
        public static ConfigFile ConfigFile { get; private set; }

        public static bool EnableRoles => true;
        
        public static bool GameStarted
        {
            get
            {
                return GameData.Instance && ShipStatus.Instance && AmongUsClient.Instance &&
                       (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started ||
                        AmongUsClient.Instance.GameMode == GameModes.FreePlay);
            }
        }

        public override void Load()
        {
            Logger = this.Log;
            ConfigFile = Config;
            
            var useCustomServer = PeasApi.ConfigFile.Bind("CustomServer", "UseCustomServer", false);
            if (useCustomServer.Value)
            {
                CustomServerManager.RegisterServer(PeasApi.ConfigFile.Bind("CustomServer", "Name", "CustomServer").Value, 
                    PeasApi.ConfigFile.Bind("CustomServer", "Ipv4 or Hostname", "au.peasplayer.tk").Value, 
                    PeasApi.ConfigFile.Bind("CustomServer", "Port", (ushort)22023).Value);    
            }
            
            RegisterCustomRoleAttribute.Register(this);
            
            Harmony.PatchAll();
        }
    }
}