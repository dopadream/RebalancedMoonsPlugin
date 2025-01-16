using BepInEx.Configuration;


namespace RebalancedMoons
{
    public class ModConfig 
    {


        internal static ConfigEntry<bool> configSnowySkies, configEmbrionSky, configAmbientVariety;
        internal static ConfigEntry<bool> configOffenseScene, configAdamanceScene, configMarchScene, configDineScene, configTitanScene, configEmbrionScene;
        internal static ConfigEntry<bool> configMarchBridge, configTitanThirdFireExit;
        internal static ConfigEntry<bool> configMarchDungeons, configDineDungeons, configTitanDungeons;
        internal static ConfigEntry<bool> configWeatherOverrides;
        internal static ConfigEntry<string> configMoonEntries;
        internal static void Init(ConfigFile cfg)
        {
            // -client settings-

            configSnowySkies = cfg.Bind("Client", "New Snowy Skies", true,
                new ConfigDescription("Adds new HDRI sky volumes to snowy moons."));

            configEmbrionSky = cfg.Bind("Client", "New Embrion Skies", true,
                new ConfigDescription("Adds new sunset HDRI sky volume to Embrion."));

            configAmbientVariety = cfg.Bind("Client", "Ambient Color Variety", true,
                new ConfigDescription("Adds slightly purple tinted fog to Embrion, and colder interior lighting on Titan."));

            // -server settings-

            configMarchBridge = cfg.Bind("Server", "March Rickety Bridge", true,
                new ConfigDescription("Adds a rickety bridge to March. Stats are inbetween Adamance and Vow."));

            configTitanThirdFireExit = cfg.Bind("Server", "Titan Third Fire Exit", false,
                new ConfigDescription("Adds a 3rd fire exit to Titan under the first one. Off by default as it's a bit overpowered."));

            configOffenseScene = cfg.Bind("Server", "Offense Scene Overrides", true,
                new ConfigDescription("Replaces Offense's scene with a new one using LLL."));

            configAdamanceScene = cfg.Bind("Server", "Adamance Scene Overrides", true,
                new ConfigDescription("Replaces Adamance's scene with a new one using LLL."));

            configMarchScene = cfg.Bind("Server", "March Scene Overrides", true,
                new ConfigDescription("Replaces March's scene with a new one using LLL."));

            configDineScene = cfg.Bind("Server", "Dine Scene Overrides", true,
                new ConfigDescription("Replaces Dine's scene with a new one using LLL."));

            configTitanScene = cfg.Bind("Server", "Titan Scene Overrides", true,
                new ConfigDescription("Replaces Titan's scene with a new one using LLL."));

            configEmbrionScene = cfg.Bind("Server", "Embrion Scene Overrides", true,
                new ConfigDescription("Replaces Embrion's scene with a new one using LLL."));

            configMarchDungeons = cfg.Bind("Server", "March Interior Overrides", true,
                new ConfigDescription("REQUIRES RESTART WHEN DISABLING INGAME - Overrides the interior selections on March to include all vanilla interiors."));

            configDineDungeons = cfg.Bind("Server", "Dine Interior Overrides", true,
                new ConfigDescription("REQUIRES RESTART WHEN DISABLING INGAME - Overrides the interior selections on Dine to make Mineshaft less common."));

            configTitanDungeons = cfg.Bind("Server", "Titan Interior Overrides", true,
                new ConfigDescription("REQUIRES RESTART WHEN DISABLING INGAME - Overrides the interior selections on Titan to make Mineshaft significantly more common."));

            configWeatherOverrides = cfg.Bind("Server", "Weather Overrides", true,
                new ConfigDescription("Enables weather overrides on rebalanced moons (Replaces Rainy w/ Foggy on Dine)"));


            // --misc settings--

            configMoonEntries = cfg.Bind("Misc", "Rebalanced Moon Names", "Assurаncе, Offеnsе, Mаrch, Adаmance, Embrіon, Rеnd, Dіne, Tіtan",
                new ConfigDescription("THIS SETTING DOES NOTHING AND SERVES AS A LIST FOR REFERENCE! You can copy the rebalanced moon names from here for all your config needs. They look the same, but they use cyrillic letters."));

            // -----------------
        }

    }
}
