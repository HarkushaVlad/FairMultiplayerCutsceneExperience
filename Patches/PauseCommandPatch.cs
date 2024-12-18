using FairMultiplayerCutsceneExperience.Config;
using FairMultiplayerCutsceneExperience.Menus;
using FairMultiplayerCutsceneExperience.Utils;
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
            if (!ModEntry.StaticHelper.ReadConfig<ModConfig>().EnablePauseMenu)
                return;

            if (Game1.netWorldState.Value.IsPaused)
            {
                MenuStack.PushMenu(new PauseMenu());
            }
            else if (Game1.activeClickableMenu is PauseMenu)
            {
                MenuStack.PopMenu();
            }
        }
    }
}