using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using System;

namespace RebalancedMoons
{
    internal static class LobbyCompatibility
    {
        internal static void Init()
        {
            PluginHelper.RegisterPlugin(Plugin.PLUGIN_GUID, Version.Parse(Plugin.PLUGIN_VERSION), CompatibilityLevel.Everyone, VersionStrictness.Minor);
        }
    }
}
