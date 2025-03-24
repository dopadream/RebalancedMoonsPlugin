using BepInEx.Configuration;


namespace RebalancedMoons
{
    public class ModConfig 
    {


        internal static ConfigEntry<bool> configTitanLighting, configIncreasedFog;
        internal static ConfigEntry<bool> configExperimentationScene, configVowScene, configAssuranceScene, configOffenseScene, configAdamanceScene, configMarchScene, configRendScene, configDineScene, configTitanScene, configEmbrionScene, configArtificeScene;
        internal static ConfigEntry<bool> configMarchBridge, configTitanThirdFireExit, configOffenseFirePath, configVowLadder, configShipShrouds;
        internal static ConfigEntry<bool> configMarchDungeons, configDineDungeons, configTitanDungeons;
        internal static ConfigEntry<bool> configWeatherOverrides, configMoonPriceOverrides, configEmbrionBoulders, configEmbrionGambling;
        internal static void Init(ConfigFile cfg)
        {
            // -client settings-

            configTitanLighting = cfg.Bind("Client", "Titan Lighting", true,
                new ConfigDescription("Adds colder interior lighting to Titan."));

            configIncreasedFog = cfg.Bind("Client", "Increased Volumetric Fog Distance", true,
                new ConfigDescription("Increases the distance volumetric fog can be seen globally from 64 to 256. May impact performance on low end hardware."));

            // -server settings-

            configShipShrouds = cfg.Bind("Server", "Sober ship", true,
                new ConfigDescription("Blocks Vain Shrouds near the ship on all moons."));

            configVowLadder = cfg.Bind("Server", "Vow Rope Ladder", true,
                new ConfigDescription("Adds a rope ladder to the dam on Vow. Must be activated from the top of the dam every round."));

            configMarchBridge = cfg.Bind("Server", "March Rickety Bridge", true,
                new ConfigDescription("Adds a rickety bridge to March. Stats are inbetween Adamance and Vow."));

            configOffenseFirePath = cfg.Bind("Server", "Offense Fire Exit Path", true,
                new ConfigDescription("Adds a lengthy route to the fire exit on Offense via new climbable rocks. Marked by a tall silo."));

            configTitanThirdFireExit = cfg.Bind("Server", "Titan Third Fire Exit", true,
                new ConfigDescription("Adds a 3rd fire exit to Titan in the vacant snowy area of the map. Marked by a guide light pole."));

            configExperimentationScene = cfg.Bind("Server", "Experimentation Scene Overrides", true,
                new ConfigDescription("Replaces Experimentation's scene with a new one using LLL."));

            configAssuranceScene = cfg.Bind("Server", "Assurance Scene Overrides", true,
                new ConfigDescription("Replaces Assurance's scene with a new one using LLL."));

            configOffenseScene = cfg.Bind("Server", "Offense Scene Overrides", true,
                new ConfigDescription("Replaces Offense's scene with a new one using LLL."));

            configVowScene = cfg.Bind("Server", "Vow Scene Overrides", true,
                new ConfigDescription("Replaces Vow's scene with a new one using LLL."));

            configMarchScene = cfg.Bind("Server", "March Scene Overrides", true,
                new ConfigDescription("Replaces March's scene with a new one using LLL."));

            configAdamanceScene = cfg.Bind("Server", "Adamance Scene Overrides", true,
                new ConfigDescription("Replaces Adamance's scene with a new one using LLL."));

            configRendScene = cfg.Bind("Server", "Rend Scene Overrides", true,
                new ConfigDescription("Replaces Rend's scene with a new one using LLL."));

            configDineScene = cfg.Bind("Server", "Dine Scene Overrides", true,
                new ConfigDescription("Replaces Dine's scene with a new one using LLL."));

            configTitanScene = cfg.Bind("Server", "Titan Scene Overrides", true,
                new ConfigDescription("Replaces Titan's scene with a new one using LLL."));

            configEmbrionScene = cfg.Bind("Server", "Embrion Scene Overrides", true,
                new ConfigDescription("Replaces Embrion's scene with a new one using LLL."));

            configArtificeScene = cfg.Bind("Server", "Artifice Scene Overrides", true,
                new ConfigDescription("Replaces Artifice's scene with a new one using LLL."));

            configEmbrionBoulders = cfg.Bind("Server", "Embrion Boulders", true,
                new ConfigDescription("Adds randomly spawning boulders to Embrion."));

            configEmbrionGambling = cfg.Bind("Server", "Embrion Gambling", true,
                new ConfigDescription("Randomizes Embrion's scrap pool equally with every one handed item in the game."));

            configMarchDungeons = cfg.Bind("Server", "March Interior Overrides", true,
                new ConfigDescription("REQUIRES RESTART WHEN DISABLING INGAME - Overrides the interior selections on March to include all vanilla interiors."));

            configDineDungeons = cfg.Bind("Server", "Dine Interior Overrides", true,
                new ConfigDescription("REQUIRES RESTART WHEN DISABLING INGAME - Overrides the interior selections on Dine to make Mineshaft less common."));

            configTitanDungeons = cfg.Bind("Server", "Titan Interior Overrides", true,
                new ConfigDescription("REQUIRES RESTART WHEN DISABLING INGAME - Overrides the interior selections on Titan to make Mineshaft significantly more common."));

            configWeatherOverrides = cfg.Bind("Server", "Weather Overrides", true,
                new ConfigDescription("Enables weather overrides on rebalanced moons (Replaces Rainy w/ Foggy on Dine)"));

            configMoonPriceOverrides = cfg.Bind("Server", "Price Overrides", true,
                new ConfigDescription("Enables price overrides on rebalanced moons"));
        }
    }
}
