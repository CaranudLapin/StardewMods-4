﻿using HarmonyLib;
using ItemExtensions.Additions;
using ItemExtensions.Events;
using ItemExtensions.Models;
using ItemExtensions.Models.Contained;
using ItemExtensions.Models.Items;
using ItemExtensions.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Triggers;

namespace ItemExtensions;

public sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += OnLaunch;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        
        helper.Events.GameLoop.DayStarted += Day.Started;
        helper.Events.GameLoop.DayEnding += Day.Ending;
        
        helper.Events.Content.AssetRequested += Assets.OnRequest;
        helper.Events.Content.AssetsInvalidated += Assets.OnInvalidate;
        
        helper.Events.Input.ButtonPressed += ActionButton.Pressed;
        helper.Events.World.ObjectListChanged += World.ObjectListChanged;
        
        helper.Events.Content.LocaleChanged += LocaleChanged;

        Config = Helper.ReadConfig<ModConfig>();
        
        Mon = Monitor;
        Help = Helper;
        Id = ModManifest.UniqueID;
        CropPatches.HasCropsAnytime = helper.ModRegistry.Get("Pathoschild.CropsAnytimeAnywhere") != null;
        
        // patches
        var harmony = new Harmony(ModManifest.UniqueID);

        ItemPatches.Apply(harmony);
        ObjectPatches.Apply(harmony);

        if (Config.MixedSeeds)
        {
            CropPatches.Apply(harmony);
            HoeDirtPatches.Apply(harmony);
        }

        if (Config.MenuActions)
        {
            InventoryPatches.Apply(harmony);
        }
        
        if (Config.EatingAnimations)
        {
            FarmerPatches.Apply(harmony);
        }
        
        if (Config.Panning)
        {
            PanPatches.Apply(harmony);
        }

        /*
        if (Config.TrainDrops)
        {
            TrainPatches.Apply(harmony);
        }

        if (Config.FishPond)
        {
            FishPondPatches.Apply(harmony);
        }*/

        if (Config.Resources)
        {
            GameLocationPatches.Apply(harmony);
            MineShaftPatches.Apply(harmony);
            ResourceClumpPatches.Apply(harmony);
        }

        if (Config.ShopTrades)
        {
            ShopMenuPatches.Apply(harmony);
        }

        if(helper.ModRegistry.Get("Esca.FarmTypeManager") is not null)
            FarmTypeManagerPatches.Apply(harmony);
        
        if(helper.ModRegistry.Get("mistyspring.dynamicdialogues") is not null)
            NpcPatches.Apply(harmony);
        
        if(helper.ModRegistry.Get("Pathoschild.TractorMod") is not null)
            TractorModPatches.Apply(harmony);
        
        //GSQ
        GameStateQuery.Register($"{Id}_ToolUpgrade", Queries.ToolUpgrade);
        GameStateQuery.Register($"{Id}_InInventory", Queries.InInventory);
        
        // trigger actions
        TriggerActionManager.RegisterTrigger($"{Id}_OnBeingHeld");
        TriggerActionManager.RegisterTrigger($"{Id}_OnStopHolding");
        
        TriggerActionManager.RegisterTrigger($"{Id}_OnPurchased");
        TriggerActionManager.RegisterTrigger($"{Id}_OnItemRemoved");
        TriggerActionManager.RegisterTrigger($"{Id}_OnItemDropped");
        
        TriggerActionManager.RegisterTrigger($"{Id}_OnEquip");
        TriggerActionManager.RegisterTrigger($"{Id}_OnUnequip");
        
        TriggerActionManager.RegisterTrigger($"{Id}_AddedToStack");
        
        #if DEBUG
        helper.ConsoleCommands.Add("ie", "Tests ItemExtension's mod capabilities", Debugging.Tester);
        helper.ConsoleCommands.Add("dump", "Exports ItemExtension's internal data", Debugging.Dump);
        helper.ConsoleCommands.Add("tas", "Tests animated sprite", Debugging.DoTas);
        #endif
        helper.ConsoleCommands.Add("fixclumps", "Fixes any missing clumps, like in the case of removed mod packs. (Usually, this won't be needed unless it's an edge-case)", Debugging.Fix);
    }

    public override object GetApi() =>new Api();

    private void OnLaunch(object sender, GameLaunchedEventArgs e)
    {
        if  (Config.Resources)
        {
            Mon.Log("Getting resources for the first time...");
            var oreData = Help.GameContent.Load<Dictionary<string, ResourceData>>($"Mods/{Id}/Resources");
            Parser.Resources(oreData, true);
        }
        
#if DEBUG
        Assets.WriteTemplates();
#endif
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        
        // register mod
        configMenu?.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );
        
        configMenu?.AddParagraph(
            mod:  ModManifest,
            text: () => Helper.Translation.Get("config.Description")
        );

        //customization
        configMenu?.AddSectionTitle(
            ModManifest,
            text: () => Helper.Translation.Get("config.Customization.title")
        );

        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.EatingAnimations.name"),
            getValue: () => Config.EatingAnimations,
            setValue: value => Config.EatingAnimations = value
        );
         
        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.MenuActions.name"),
            getValue: () => Config.MenuActions,
            setValue: value => Config.MenuActions = value
        );
         
        //vanilla function extension
        configMenu?.AddSectionTitle(
            ModManifest,
            text: () => Helper.Translation.Get("config.VanillaExt.title")
        );

        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.FishPond.name"),
            getValue: () => Config.FishPond,
            setValue: value => Config.FishPond = value
        );
         
        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.MixedSeeds.name"),
            getValue: () => Config.MixedSeeds,
            setValue: value => Config.MixedSeeds = value
        );
         
        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.Resources.name"),
            getValue: () => Config.Resources,
            setValue: value => Config.Resources = value
        );
         
        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.ShopTrades.name"),
            getValue: () => Config.ShopTrades,
            setValue: value => Config.ShopTrades = value
        );
         
        //extra drops
        configMenu?.AddSectionTitle(
            ModManifest,
            text: () => Helper.Translation.Get("config.Drops.title")
        );

        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.Panning.name"),
            getValue: () => Config.Panning,
            setValue: value => Config.Panning = value
        );

        configMenu?.AddBoolOption(
            mod: ModManifest,
            name: () => Help.Translation.Get("config.TrainDrops.name"),
            getValue: () => Config.TrainDrops,
            setValue: value => Config.TrainDrops = value
        );
    }

    /// <summary>
    /// At this point, the mod loads its files and adds contentpacks' changes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        //get obj data
        var objData = Help.GameContent.Load<Dictionary<string, ItemData>>($"Mods/{Id}/Data");
        Parser.ObjectData(objData);
        Monitor.Log($"Loaded {Data?.Count ?? 0} item data.", LogLevel.Debug);
        
        if (Config.EatingAnimations)
        {
            //get custom animations
            var animations = Help.GameContent.Load<Dictionary<string, FarmerAnimation>>($"Mods/{Id}/EatingAnimations");
            Parser.EatingAnimations(animations);
            Monitor.Log($"Loaded {EatingAnimations?.Count ?? 0} eating animations.", LogLevel.Debug);
        }
        
        if (Config.MenuActions)
        {
            //get item actions
            var menuActions = Help.GameContent.Load<Dictionary<string, List<MenuBehavior>>>($"Mods/{Id}/MenuActions");
            Parser.ItemActions(menuActions);
            Monitor.Log($"Loaded {MenuActions?.Count ?? 0} menu actions.", LogLevel.Debug);
        }
        
        if (Config.Resources)
        {
            //get item actions
            var trees = Help.GameContent.Load<Dictionary<string, TerrainSpawnData>>($"Mods/{Id}/Mines/Terrain");
            Parser.Terrain(trees);
            Monitor.Log($"Loaded {MineFeatures?.Count ?? 0} mineshaft terrain features.", LogLevel.Debug);
        }
        
        if (Config.MixedSeeds)
        {
            //get mixed seeds
            var seedData = Help.GameContent.Load<Dictionary<string, List<MixedSeedData>>>($"Mods/{Id}/MixedSeeds");
            Parser.MixedSeeds(seedData);
            Monitor.Log($"Loaded {Seeds?.Count ?? 0} mixed seeds data.", LogLevel.Debug);
        }
        
        if (Config.Panning)
        {
            //get panning
            var panData = Help.GameContent.Load<Dictionary<string, PanningData>>($"Mods/{Id}/Panning");
            Parser.Panning(panData);
            Monitor.Log($"Loaded {Panning?.Count ?? 0} mixed seeds data.", LogLevel.Debug);
            /*
            //train stuff
            var trainData = Help.GameContent.Load<Dictionary<string, ExtraSpawn>>($"Mods/{Id}/Train");
            Parser.Train(trainData);
            Monitor.Log($"Loaded {TrainDrops?.Count ?? 0} custom train drops.", LogLevel.Debug);
            */
        }
        
        //ACTION BUTTON LIST
        var temp = new List<SButton>();
        foreach (var b in Game1.options.actionButton)
        {
            temp.Add(b.ToSButton());
            Monitor.Log("Button: " + b);
        }
        Monitor.Log($"Total {Game1.options.actionButton?.Length ?? 0}");

        ActionButtons = temp;
    }

    private static void LocaleChanged(object sender, LocaleChangedEventArgs e)
    {
        Comma = e.NewLanguage switch
        {
            LocalizedContentManager.LanguageCode.ja => "、",
            LocalizedContentManager.LanguageCode.zh => "，",
            _ => ", "
        };
    }

    /// <summary>Buttons used for custom item actions</summary>
    internal static List<SButton> ActionButtons { get; private set; } = new();

    public static string Id { get; set; }
    internal static string Comma { get; private set; } = ", ";

    internal static IModHelper Help { get; set; }
    internal static IMonitor Mon { get; set; }
#if DEBUG
    private const LogLevel Level = LogLevel.Debug;
#else
    private const LogLevel Level =  LogLevel.Trace;
#endif

    internal static bool Holding { get; set; }
    internal static ModConfig Config { get; private set; }
    public static Dictionary<string, ResourceData> BigClumps { get; internal set; } = new();
    public static Dictionary<string, ItemData> Data { get; internal set; } = new();
    internal static Dictionary<string, FarmerAnimation> EatingAnimations { get; set; } = new();
    internal static Dictionary<string, List<MenuBehavior>> MenuActions { get; set; } = new();
    internal static Dictionary<string, TerrainSpawnData> MineFeatures { get; set; } = new();
    public static Dictionary<string, ResourceData> Ores { get; internal set; } = new();
    public static List<PanningData> Panning { get; internal set; } = new();
    internal static Dictionary<string, List<MixedSeedData>> Seeds { get; set; } = new();
}
