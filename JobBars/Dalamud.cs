using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;

namespace JobBars {
    public class Dalamud {
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static ICondition Condition { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IObjectTable Objects { get; private set; } = null!;
        [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] public static IJobGauges JobGauges { get; private set; } = null;
        [PluginService] public static IPluginLog PluginLog { get; private set; } = null;
        [PluginService] public static IGameInteropProvider Hooks { get; private set; } = null;
        [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null;

        public static void Error( Exception e, string message ) => PluginLog.Error( e, message );

        public static void Error( string message ) => PluginLog.Error( message );

        public static void Log( string messages ) => PluginLog.Info( messages );
    }
}
