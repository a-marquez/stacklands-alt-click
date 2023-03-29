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

        public void Awake()
        {
            L = Logger;
            try
            {
                Harmony.CreateAndPatchAll(typeof(Plugin));
            }
            catch(Exception E)
            {
                L.LogError(E.Message);
            }
        }

        // Reference: https://github.com/benediktwerner/Stacklands-BugFixes-Mod/blob/master/Plugin.cs#L65
        [HarmonyPatch(typeof(GameCard), nameof(GameCard.StartDragging))]
        [HarmonyPrefix]
        public static void GameCard_StartDragging_Prefix(GameCard __instance, out GameCard __state)
        {

            __state = null;

            if (!checkIfAltPressed())
            {
                return;
            }

            if (!checkIfClickableChest(__instance.CardData))
            {
                return;
            }
            __state = __instance.Parent;

        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.StartDragging))]
        [HarmonyPostfix]
        public static void GameCard_StartDragging_Postfix(GameCard __instance, GameCard __state)
        {
            if (!checkIfAltPressed())
            {
                return;
            }
            
            GameCard clickTarget = __instance;

            if (!checkIfClickableChest(clickTarget.CardData))
            {
                clickTarget = clickTarget.GetLeafCard();
                if (!checkIfClickableChest(clickTarget.CardData))
                {
                    return;
                }
            }


            int previousCardCount = WorldManager.instance.AllCards.Count;
            clickTarget.Clicked();
            GameCard newCard;

            if (previousCardCount >= WorldManager.instance.AllCards.Count)
            {
                newCard = __instance;
            }
            else
            {
                newCard = WorldManager.instance.AllCards.Last().GetRootCard();
                newCard.Velocity = new Vector3();
                if (__state)
                {
                    __instance.SetParent(__state);
                }
            }

            newCard.DragTag = null;

            WorldManager.instance.DraggingDraggable = newCard;
            WorldManager.instance.DraggingDraggable.DragStartPosition = WorldManager.instance.DraggingDraggable.transform.position;
            WorldManager.instance.grabOffset = WorldManager.instance.mouseWorldPosition - WorldManager.instance.DraggingDraggable.transform.position;
            //WorldManager.instance.DraggingDraggable.StartDragging();
        }
        public static bool checkIfClickableChest(CardData chestCandidate)
        {
            return (chestCandidate is Chest chest && chest.CoinCount > 0)
                || (chestCandidate is ResourceChest resourceChest && resourceChest.ResourceCount > 0);
        }
        public static bool checkIfAltPressed()
        {
            return InputController.instance.GetKey(Key.LeftAlt) || InputController.instance.GetKey(Key.RightAlt);
        }

        [HarmonyPatch(typeof(Draggable), nameof(Draggable.StartDragging))]
        [HarmonyPrefix]
        public static void Draggable_StartDragging_Prefix(Draggable __instance)
        {
            L.LogInfo(__instance.name);
            if (__instance is not Boosterpack pack)
            {
                return;
            }
            if (!checkIfAltPressed())
            {
                return;
            }

            int previousCardCount = WorldManager.instance.AllCards.Count;
            __instance.Clicked();
            Draggable newDrag;

            if (previousCardCount >= WorldManager.instance.AllCards.Count)
            {
                newDrag = __instance;
            }
            else
            {
                newDrag = WorldManager.instance.AllCards.Last().GetRootCard();
                newDrag.Velocity = new Vector3();
            }

            newDrag.DragTag = null;

            WorldManager.instance.DraggingDraggable = newDrag;
            WorldManager.instance.DraggingDraggable.DragStartPosition = WorldManager.instance.DraggingDraggable.transform.position;
            WorldManager.instance.grabOffset = WorldManager.instance.mouseWorldPosition - WorldManager.instance.DraggingDraggable.transform.position;
            //WorldManager.instance.DraggingDraggable.StartDragging();
        }
    }
}
