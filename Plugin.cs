using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StacklandsAltClick
{
    [BepInPlugin("com.a-marquez.stacklands.altclick", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        static ManualLogSource L;

        private void Awake()
        {
            L = Logger;
            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        public void Update()
        {
            L.LogInfo("Hello from Stacklandsaltclick");
        }

    }
}
