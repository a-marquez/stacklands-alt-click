using System;
using System.Collections.Generic;
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
    
        [HarmonyPatch(typeof(Draggable), nameof(Draggable.Clicked))]
        [HarmonyPostfix]
        public static void Draggable_Clicked_Postfix(Draggable __instance)
        {
            if (__instance is not BuyBoosterBox pack)
            {
                return;
            }
            if (!checkIfAltPressed())
            {
                return;
            }
            int cost = pack.GetCurrentCost();
            if (cost <= 0 )
            {
                return;
            }
            if (pack.BuyWithGold && cost > WorldManager.instance.GetGoldCount(includeInChest: true))
            {
                return;
            }
            if (!pack.BuyWithGold && cost > WorldManager.instance.GetShellCount(includeInChest: true))
            {
                return;
            }

            List<CardData> coins = pack.BuyWithGold
                ? WorldManager.instance.GetCards<Gold>().Select(gold => gold as CardData).ToList()
                : WorldManager.instance.GetCards<Shell>().Select(shell => shell as CardData).ToList(); //this is so stupid

            if (cost != coins.Count)
            {
                coins.Sort((coin1, coin2) => (int)(Vector3.Distance(coin1.MyGameCard.GetRootCard().transform.position, __instance.transform.position) - Vector3.Distance(coin2.MyGameCard.GetRootCard().transform.position, __instance.transform.position)));
            }

            for (var i = 0; i < coins.Count && cost > 0; i++, cost--)
            {
                coins[i].MyGameCard.RemoveFromStack();
                pack.CardDropped(coins[i].MyGameCard);
            }

            if (cost <= 0)
            {
                return;
            }

            List<Chest> chests = WorldManager.instance.GetCards<Chest>();
            chests.Sort((chest1, chest2) => (int)(Vector3.Distance(chest1.MyGameCard.GetRootCard().transform.position, __instance.transform.position) - Vector3.Distance(chest2.MyGameCard.GetRootCard().transform.position, __instance.transform.position)));
            GameCard parent = null;
            GameCard child = null;
            for (var i = 0; i < chests.Count && cost > 0; i++)
            {
                
                if (pack.BuyWithGold != (chests[i].HeldCardId == "gold"))
                {
                    continue;
                }
                cost -= chests[i].CoinCount;
                parent = chests[i].MyGameCard.Parent;
                child = chests[i].MyGameCard.Child;
                pack.CardDropped(chests[i].MyGameCard);
                chests[i].MyGameCard.SetParent(parent);
                chests[i].MyGameCard.SetChild(child);
                chests[i].MyGameCard.Velocity = new Vector3();
                chests[i].MyGameCard.RotWobble(0);
            }
		    AudioManager.me.PlaySound2D(AudioManager.me.CardDestroy, UnityEngine.Random.Range(0.8f, 1.2f), 0.3f);
        }
        
    }
}
