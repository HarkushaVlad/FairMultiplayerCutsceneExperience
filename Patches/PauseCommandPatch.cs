using FairMultiplayerCutsceneExperience.Menus;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace FairMultiplayerCutsceneExperience.Patches
{
    [HarmonyPatch(typeof(ChatCommands.DefaultHandlers), "Pause")]
    public static class PauseCommandPatch
    {
        public static void Postfix(string[] command, ChatBox chat)
        {
            if (Game1.netWorldState.Value.IsPaused)
            {
                Game1.activeClickableMenu = new PauseMenu();
            }
        }
    }
}