using HarmonyLib;
using ResoniteModLoader;
using System;
using FrooxEngine;

namespace DisableLegacyInventory
{
    public class DisableLegacyInventory : ResoniteMod
    {
        public override string Name => "DisableLegacyInventory";
        public override string Author => "art0007i";
        public override string Version => "2.0.0";
        public override string Link => "https://github.com/art0007i/DisableLegacyInventory/";

        [AutoRegisterConfigKey]
        public ModConfigurationKey<bool> KEY_ENABLE = new("enable", "If true you will not be able to use the legacy inventory gesture", () => true);

        public static Action<InteractionHandler> dashDelegate;
        public static bool fail = false;
        public override void OnEngineInit()
        {
            // This mod does not require harmony at all. Yay

            // Still need this next line, even as a comment because my stupid manifest generator tool looks for this string as the mod id
            //Harmony harmony = new Harmony("me.art0007i.DisableLegacyInventory");

            GetConfiguration().OnThisConfigurationChanged += ConfigHandler;


            var userspaceToggle = AccessTools.Field(typeof(InteractionHandler), "_userspaceToggle");
            Engine.Current.OnReady += () => {
                var tool = Userspace.UserspaceWorld.LocalUser.Root.Slot.GetComponentInChildren<InteractionHandler>((tool) => tool.Side == Chirality.Left);
                if(tool == null)
                {
                    Error("Failed to locate common tool. This means the mod will not work at all.");
                    fail = true; // doing this cause if we cause a null reference exception here, the game will fully crash
                    return;
                }
                dashDelegate = userspaceToggle.GetValue(tool) as Action<InteractionHandler>;
                Msg(dashDelegate);
                var dash = Userspace.UserspaceWorld.RootSlot.GetComponentInChildren<UserspaceRadiantDash>();
                if (GetConfiguration().GetValue(KEY_ENABLE))
                {
                    userspaceToggle.SetValue(tool, (InteractionHandler ct) => { dash.Open = !dash.Open; });
                }
            };
        }

        private void ConfigHandler(ConfigurationChangedEvent configurationChangedEvent)
        {
            if (fail) return;
            if(configurationChangedEvent.Key == KEY_ENABLE)// sanity check
            { 
                var userspaceToggle = AccessTools.Field(typeof(InteractionHandler), "_userspaceToggle");
                var tool = Userspace.UserspaceWorld.LocalUser.Root.Slot.GetComponentInChildren<InteractionHandler>((tool) => tool.Side == Chirality.Left);
                if (configurationChangedEvent.Config.GetValue(KEY_ENABLE))
                {
                    var dash = Userspace.UserspaceWorld.RootSlot.GetComponentInChildren<UserspaceRadiantDash>();
                    userspaceToggle.SetValue(tool, (InteractionHandler ct) => { dash.Open = !dash.Open;});
                }
                else
                {
                    userspaceToggle.SetValue(tool, dashDelegate);
                }
            }
        }
    }
}