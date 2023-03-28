using System;
using System.Linq;
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
            try {
                Harmony.CreateAndPatchAll(typeof(Plugin));
            } catch(Exception E) {
                L.LogError(E.Message);
            }
        }

        // Reference: https://github.com/benediktwerner/Stacklands-BugFixes-Mod/blob/master/Plugin.cs#L65
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameCard), nameof(GameCard.StartDragging))]
        private static void DontDetachContainersFromGlueAfterAltClicking(GameCard __instance, out GameCard[] __state)
        {

            __state = new GameCard[0];

            if (!InputController.instance.GetKey(Key.LeftAlt) && !InputController.instance.GetKey(Key.RightAlt)) {
                return;
            }

            if (__instance.CardData is not Chest && __instance.CardData is not ResourceChest) {
                return;
            }

            if (__instance.Parent?.CardData is not HeavyFoundation) {
                return;
            }

            __state = new GameCard[2] { __instance, __instance.Parent };

        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.StartDragging))]
        [HarmonyPostfix]
        public static void StartDragging(GameCard __instance, GameCard[] __state)
        {
            if (!InputController.instance.GetKey(Key.LeftAlt) && !InputController.instance.GetKey(Key.RightAlt)) {
                return;
            }

            if (__instance.CardData is not Chest && __instance.CardData is not ResourceChest) {
                return;
            }

            if (__instance.CardData is Chest chest && chest.CoinCount <= 0) {
                return;
            }

            if (__instance.CardData is ResourceChest resourceChest && resourceChest.ResourceCount <= 0) {
                return;
            }

            // Try to fix containers detaching from glue
            if (__state != null && __state.Length > 0) {
                L.LogInfo(__state);
            }

            __instance.Clicked();
            GameCard newCard = WorldManager.instance.AllCards.Last().GetRootCard();

            newCard.DragTag = null;

            WorldManager.instance.DraggingDraggable = newCard;
            WorldManager.instance.DraggingDraggable.DragStartPosition = WorldManager.instance.DraggingDraggable.transform.position;
            WorldManager.instance.grabOffset = WorldManager.instance.mouseWorldPosition - WorldManager.instance.DraggingDraggable.transform.position;
            WorldManager.instance.DraggingDraggable.StartDragging();
        }

    }
}
