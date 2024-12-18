using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace FairMultiplayerCutsceneExperience.Patches
{
    [HarmonyPatch(typeof(ChatCommands.DefaultHandlers), "Resume")]
    public static class ResumeCommandPatch
    {
        public static void Postfix(string[] command, ChatBox chat)
        {
            if (!Game1.netWorldState.Value.IsPaused)
            {
                Game1.activeClickableMenu = null;
            }
        }
    }
}