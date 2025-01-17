﻿using Substrate.Core;
using Substrate.Nbt;

namespace Substrate;

/// <summary>
/// Encompases data to specify game rules.
/// </summary>
public class GameRules : ICopyable<GameRules>
{

    /// <summary>
    /// Gets or sets whether or not actions performed by command blocks are displayed in the chat.
    /// </summary>
    public bool CommandBlockOutput { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to spread or remove fire.
    /// </summary>
    public bool DoFireTick { get; set; } = true;

    /// <summary>
    /// Gets or sets whether mobs should drop loot when killed.
    /// </summary>
    public bool DoMobLoot { get; set; } = true;

    /// <summary>
    /// Gets or sets whether mobs should spawn naturally.
    /// </summary>
    public bool DoMobSpawning { get; set; } = true;

    /// <summary>
    /// Gets or sets whether breaking blocks should drop the block's item drop.
    /// </summary>
    public bool DoTileDrops { get; set; } = true;

    /// <summary>
    /// Gets or sets whether players keep their inventory after they die.
    /// </summary>
    public bool KeepInventory { get; set; } = false;

    /// <summary>
    /// Gets or sets whether mobs can destroy blocks (creeper explosions, zombies breaking doors, etc.).
    /// </summary>
    public bool MobGriefing { get; set; } = true;

    #region ICopyable<GameRules> Members

    /// <inheritdoc />
    public GameRules Copy()
    {
        GameRules gr = new();
        gr.CommandBlockOutput = CommandBlockOutput;
        gr.DoFireTick = DoFireTick;
        gr.DoMobLoot = DoMobLoot;
        gr.DoMobSpawning = DoMobSpawning;
        gr.DoTileDrops = DoTileDrops;
        gr.KeepInventory = KeepInventory;
        gr.MobGriefing = MobGriefing;

        return gr;
    }

    #endregion
}

/// <summary>
/// Specifies the type of gameplay associated with a world.
/// </summary>
public enum GameType
{
    /// <summary>
    /// The world will be played in Survival mode.
    /// </summary>
    SURVIVAL = 0,

    /// <summary>
    /// The world will be played in Creative mode.
    /// </summary>
    CREATIVE = 1,
}

public enum TimeOfDay
{
    Daytime = 0,
    Noon = 6000,
    Sunset = 12000,
    Nighttime = 13800,
    Midnight = 18000,
    Sunrise = 22200,
}

/// <summary>
/// Represents general data and metadata of a single world.
/// </summary>
public class Level : INbtObject<Level>, ICopyable<Level>
{
    private NbtWorld _world;
    private Player _player;

    private int _spawnX;
    private int _spawnY;
    private int _spawnZ;
    private int? _version;
    private string _name;
    private byte? _raining;
    private byte? _thundering;
    private int? _rainTime;
    private int? _thunderTime;

    private int? _gameType;
    private byte? _mapFeatures;
    private byte? _hardcore;

    private int? _generatorVersion;
    private string _generatorOptions;
    private byte? _initialized;
    private byte? _allowCommands;
    private long? _DayTime;

    /// <summary>
    /// Gets or sets the creation time of the world as a long timestamp.
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// Gets or sets the time that the world was last played as a long timestamp.
    /// </summary>
    public long LastPlayed { get; set; }

    /// <summary>
    /// Gets or sets the player for single-player worlds.
    /// </summary>
    public Player Player
    {
        get => _player;
        set
        {
            _player = value;
            _player.World = _name;
        }
    }

    /// <summary>
    /// Gets or sets the world's spawn point.
    /// </summary>
    public SpawnPoint Spawn
    {
        get => new(_spawnX, _spawnY, _spawnZ);
        set
        {
            _spawnX = value.X;
            _spawnY = value.Y;
            _spawnZ = value.Z;
        }
    }

    /// <summary>
    /// Gets the estimated size of the world in bytes.
    /// </summary>
    public long SizeOnDisk { get; private set; }

    /// <summary>
    /// Gets or sets the world's random seed.
    /// </summary>
    public long RandomSeed { get; set; }

    /// <summary>
    /// Gets or sets the world's version number.
    /// </summary>
    public int Version
    {
        get => _version ?? 0;
        set => _version = value;
    }

    /// <summary>
    /// Gets or sets the name of the world.
    /// </summary>
    /// <remarks>If there is a <see cref="Player"/> object attached to this world, the player's world field 
    /// will also be updated.</remarks>
    public string LevelName
    {
        get => _name;
        set
        {
            _name = value;
            if (_player != null)
            {
                _player.World = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the world generator.
    /// </summary>
    /// <remarks>This should be 'default', 'flat', or 'largeBiomes'.</remarks>
    public string GeneratorName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating that it is raining in the world.
    /// </summary>
    public bool IsRaining
    {
        get => (_raining ?? 0) == 1;
        set => _raining = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Gets or sets a value indicating that it is thunderstorming in the world.
    /// </summary>
    public bool IsThundering
    {
        get => (_thundering ?? 0) == 1;
        set => _thundering = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Gets or sets the timer value for controlling rain.
    /// </summary>
    public int RainTime
    {
        get => _rainTime ?? 0;
        set => _rainTime = value;
    }

    /// <summary>
    /// Gets or sets the timer value for controlling thunderstorms.
    /// </summary>
    public int ThunderTime
    {
        get => _thunderTime ?? 0;
        set => _thunderTime = value;
    }

    /// <summary>
    /// Gets or sets the game type associated with this world.
    /// </summary>
    public GameType GameType
    {
        get => (GameType)(_gameType ?? 0);
        set => _gameType = (int)value;
    }

    /// <summary>
    /// Gets or sets a value indicating that structures (dungeons, villages, ...) will be generated.
    /// </summary>
    public bool UseMapFeatures
    {
        get => (_mapFeatures ?? 0) == 1;
        set => _mapFeatures = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the map is hardcore mode
    /// </summary>
    public bool Hardcore
    {
        get => (_hardcore ?? 0) == 1;
        set => _hardcore = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Gets or sets a value indicating the version of the level generator
    /// </summary>
    public int GeneratorVersion
    {
        get => _generatorVersion ?? 0;
        set => _generatorVersion = value;
    }

    /// <summary>
    /// Gets or sets a value indicating controls options for the generator, 
    /// currently only the Superflat generator. The format is a comma separated 
    /// list of block IDs from the bottom of the map up, and each block ID may 
    /// optionally be preceded by the number of layers and an x. 
    /// Damage values are not supported.
    /// </summary>
    public string GeneratorOptions
    {
        get => _generatorOptions ?? "";
        set => _generatorOptions = value;
    }

    /// <summary>
    /// Gets or sets a value, normally true, indicating whether a world has been 
    /// initialized properly after creation. If the initial simulation was canceled 
    /// somehow, this can be false and the world will be re-initialized on next load.
    /// </summary>
    public bool Initialized
    {
        get => (_initialized ?? 0) == 1;
        set => _initialized = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Gets or sets a value indicating if cheats are enabled.
    /// </summary>
    public bool AllowCommands
    {
        get => (_allowCommands ?? 0) == 1;
        set => _allowCommands = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Gets or sets a value indicating the time of day. 
    /// 0 is sunrise, 6000 is midday, 12000 is sunset, 
    /// 18000 is midnight, 24000 is the next day's 0. 
    /// This value keeps counting past 24000 and does not reset to 0
    /// </summary>
    public long DayTime
    {
        get => _DayTime ?? 0;
        set => _DayTime = value;
    }

    /// <summary>
    /// Gets the level's game rules.
    /// </summary>
    public GameRules GameRules { get; private set; }

    /// <summary>
    /// Gets the source <see cref="TagNodeCompound"/> used to create this <see cref="Level"/> if it exists.
    /// </summary>
    public TagNodeCompound Source { get; private set; }

    /// <summary>
    /// Gets a <see cref="SchemaNode"/> representing the schema of a level.
    /// </summary>
    public static SchemaNodeCompound Schema { get; } = new()
    {
        new SchemaNodeCompound("Data")
        {
            new SchemaNodeScaler("Time", TagType.TAG_LONG),
            new SchemaNodeScaler("LastPlayed", TagType.TAG_LONG, SchemaOptions.CREATE_ON_MISSING),
            new SchemaNodeCompound("Player", Player.Schema, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("SpawnX", TagType.TAG_INT),
            new SchemaNodeScaler("SpawnY", TagType.TAG_INT),
            new SchemaNodeScaler("SpawnZ", TagType.TAG_INT),
            new SchemaNodeScaler("SizeOnDisk", TagType.TAG_LONG, SchemaOptions.CREATE_ON_MISSING),
            new SchemaNodeScaler("RandomSeed", TagType.TAG_LONG),
            new SchemaNodeScaler("version", TagType.TAG_INT, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("LevelName", TagType.TAG_STRING, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("generatorName", TagType.TAG_STRING, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("raining", TagType.TAG_BYTE, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("thundering", TagType.TAG_BYTE, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("rainTime", TagType.TAG_INT, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("thunderTime", TagType.TAG_INT, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("GameType", TagType.TAG_INT, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("MapFeatures", TagType.TAG_BYTE, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("hardcore", TagType.TAG_BYTE, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("generatorVersion", TagType.TAG_INT, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("generatorOptions", TagType.TAG_STRING, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("initialized", TagType.TAG_BYTE, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("allowCommands", TagType.TAG_BYTE, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("DayTime", TagType.TAG_LONG, SchemaOptions.OPTIONAL),
            new SchemaNodeCompound("GameRules", SchemaOptions.OPTIONAL)
                {
                    new SchemaNodeScaler("commandBlockOutput", TagType.TAG_STRING),
                    new SchemaNodeScaler("doFireTick", TagType.TAG_STRING),
                    new SchemaNodeScaler("doMobLoot", TagType.TAG_STRING),
                    new SchemaNodeScaler("doMobSpawning", TagType.TAG_STRING),
                    new SchemaNodeScaler("doTileDrops", TagType.TAG_STRING),
                    new SchemaNodeScaler("keepInventory", TagType.TAG_STRING),
                    new SchemaNodeScaler("mobGriefing", TagType.TAG_STRING),
                },
        },
    };

    /// <summary>
    /// Creates a new <see cref="Level"/> object with reasonable defaults tied to the given world.
    /// </summary>
    /// <param name="world">The world that the <see cref="Level"/> should be tied to.</param>
    public Level(NbtWorld world)
    {
        _world = world;

        // Sane defaults
        Time = 0;
        LastPlayed = 0;
        _spawnX = 0;
        _spawnY = 64;
        _spawnZ = 0;
        SizeOnDisk = 0;
        RandomSeed = new Random().Next();
        //_version = 19132;
        _version = 19133;
        _name = "Untitled";
        GeneratorName = "default";
        _hardcore = 0;

        _generatorOptions = "";
        _generatorVersion = 1;
        _initialized = 0;
        _allowCommands = 0;
        _DayTime = 0;
        GameRules = new GameRules();

        GameType = GameType.SURVIVAL;
        UseMapFeatures = true;

        Source = new TagNodeCompound();
    }

    /// <summary>
    /// Creates a copy of an existing <see cref="Level"/> object.
    /// </summary>
    /// <param name="p">The <see cref="Level"/> object to copy.</param>
    protected Level(Level p)
    {
        _world = p._world;

        Time = p.Time;
        LastPlayed = p.LastPlayed;
        _spawnX = p._spawnX;
        _spawnY = p._spawnY;
        _spawnZ = p._spawnZ;
        SizeOnDisk = p.SizeOnDisk;
        RandomSeed = p.RandomSeed;
        _version = p._version;
        _name = p._name;
        GeneratorName = p.GeneratorName;

        _raining = p._raining;
        _thundering = p._thundering;
        _rainTime = p._rainTime;
        _thunderTime = p._thunderTime;

        _gameType = p._gameType;
        _mapFeatures = p._mapFeatures;
        _hardcore = p._hardcore;

        _generatorVersion = p._generatorVersion;
        _generatorOptions = p._generatorOptions;
        _initialized = p._initialized;
        _allowCommands = p._allowCommands;
        _DayTime = p._DayTime;
        GameRules = p.GameRules.Copy();

        if (p._player != null)
        {
            _player = p._player.Copy();
        }

        if (p.Source != null)
        {
            Source = p.Source.Copy() as TagNodeCompound;
        }
    }

    /// <summary>
    /// Creates a default player entry for this world.
    /// </summary>
    public void SetDefaultPlayer()
    {
        _player = new Player();
        _player.World = _name;

        _player.Position.X = _spawnX;
        _player.Position.Y = _spawnY + 1.7;
        _player.Position.Z = _spawnZ;
    }

    /// <summary>
    /// Saves a <see cref="Level"/> object to disk as a standard compressed NBT stream.
    /// </summary>
    /// <returns>True if the level was saved; false otherwise.</returns>
    /// <exception cref="LevelIOException">Thrown when an error is encountered writing out the level.</exception>
    public bool Save()
    {
        if (_world == null)
        {
            return false;
        }

        try
        {
            NBTFile nf = new(Path.Combine(_world.Path, "level.dat"));
            using Stream zipstr = nf.GetDataOutputStream();
            if (zipstr == null)
            {
                NbtIOException nex = new("Failed to initialize compressed NBT stream for output");
                nex.Data["Level"] = this;
                throw nex;
            }

            new NbtTree(BuildTree() as TagNodeCompound).WriteTo(zipstr);

            return true;
        }
        catch (Exception ex)
        {
            LevelIOException lex = new("Could not save level file.", ex);
            lex.Data["Level"] = this;
            throw lex;
        }
    }


    #region INBTObject<Player> Members

    /// <summary>
    /// Attempt to load a Level subtree into the <see cref="Level"/> without validation.
    /// </summary>
    /// <param name="tree">The root node of a Level subtree.</param>
    /// <returns>The <see cref="Level"/> returns itself on success, or null if the tree was unparsable.</returns>
    public virtual Level LoadTree(TagNode tree)
    {
        TagNodeCompound dtree = tree as TagNodeCompound;
        if (dtree == null)
        {
            return null;
        }

        _version = null;
        _raining = null;
        _rainTime = null;
        _thundering = null;
        _thunderTime = null;
        _gameType = null;
        _mapFeatures = null;
        _generatorOptions = null;
        _generatorVersion = null;
        _allowCommands = null;
        _initialized = null;
        _DayTime = null;

        TagNodeCompound ctree = dtree["Data"].ToTagCompound();

        Time = ctree["Time"].ToTagLong();
        LastPlayed = ctree["LastPlayed"].ToTagLong();

        if (ctree.ContainsKey("Player"))
        {
            _player = new Player().LoadTree(ctree["Player"]);
        }

        _spawnX = ctree["SpawnX"].ToTagInt();
        _spawnY = ctree["SpawnY"].ToTagInt();
        _spawnZ = ctree["SpawnZ"].ToTagInt();

        SizeOnDisk = ctree["SizeOnDisk"].ToTagLong();
        RandomSeed = ctree["RandomSeed"].ToTagLong();

        if (ctree.ContainsKey("version"))
        {
            _version = ctree["version"].ToTagInt();
        }
        if (ctree.ContainsKey("LevelName"))
        {
            _name = ctree["LevelName"].ToTagString();
        }

        if (ctree.ContainsKey("generatorName"))
        {
            GeneratorName = ctree["generatorName"].ToTagString();
        }

        if (ctree.ContainsKey("raining"))
        {
            _raining = ctree["raining"].ToTagByte();
        }
        if (ctree.ContainsKey("thundering"))
        {
            _thundering = ctree["thundering"].ToTagByte();
        }
        if (ctree.ContainsKey("rainTime"))
        {
            _rainTime = ctree["rainTime"].ToTagInt();
        }
        if (ctree.ContainsKey("thunderTime"))
        {
            _thunderTime = ctree["thunderTime"].ToTagInt();
        }

        if (ctree.ContainsKey("GameType"))
        {
            _gameType = ctree["GameType"].ToTagInt();
        }
        if (ctree.ContainsKey("MapFeatures"))
        {
            _mapFeatures = ctree["MapFeatures"].ToTagByte();
        }
        if (ctree.ContainsKey("hardcore"))
        {
            _hardcore = ctree["hardcore"].ToTagByte();
        }

        if (ctree.ContainsKey("generatorVersion"))
        {
            _generatorVersion = ctree["generatorVersion"].ToTagInt();
        }
        if (ctree.ContainsKey("generatorOptions"))
        {
            _generatorOptions = ctree["generatorOptions"].ToTagString();
        }
        if (ctree.ContainsKey("allowCommands"))
        {
            _allowCommands = ctree["allowCommands"].ToTagByte();
        }
        if (ctree.ContainsKey("initialized"))
        {
            _initialized = ctree["initialized"].ToTagByte();
        }
        if (ctree.ContainsKey("DayTime"))
        {
            _DayTime = ctree["DayTime"].ToTagLong();
        }
        if (ctree.ContainsKey("GameRules"))
        {
            TagNodeCompound gr = ctree["GameRules"].ToTagCompound();

            GameRules = new GameRules();
            GameRules.CommandBlockOutput = gr["commandBlockOutput"].ToTagString().Data == "true";
            GameRules.DoFireTick = gr["doFireTick"].ToTagString().Data == "true";
            GameRules.DoMobLoot = gr["doMobLoot"].ToTagString().Data == "true";
            GameRules.DoMobSpawning = gr["doMobSpawning"].ToTagString().Data == "true";
            GameRules.DoTileDrops = gr["doTileDrops"].ToTagString().Data == "true";
            GameRules.KeepInventory = gr["keepInventory"].ToTagString().Data == "true";
            GameRules.MobGriefing = gr["mobGriefing"].ToTagString().Data == "true";
        }

        Source = ctree.Copy() as TagNodeCompound;

        return this;
    }

    /// <summary>
    /// Attempt to load a Level subtree into the <see cref="Level"/> with validation.
    /// </summary>
    /// <param name="tree">The root node of a Level subtree.</param>
    /// <returns>The <see cref="Level"/> returns itself on success, or null if the tree failed validation.</returns>
    public virtual Level LoadTreeSafe(TagNode tree)
    {
        return !ValidateTree(tree) ? null : LoadTree(tree);
    }

    /// <summary>
    /// Builds a Level subtree from the current data.
    /// </summary>
    /// <returns>The root node of a Level subtree representing the current data.</returns>
    public virtual TagNode BuildTree()
    {
        TagNodeCompound data = new();
        data["Time"] = new TagNodeLong(Time);
        data["LastPlayed"] = new TagNodeLong(LastPlayed);

        if (_player != null)
        {
            data["Player"] = _player.BuildTree();
        }

        data["SpawnX"] = new TagNodeInt(_spawnX);
        data["SpawnY"] = new TagNodeInt(_spawnY);
        data["SpawnZ"] = new TagNodeInt(_spawnZ);
        data["SizeOnDisk"] = new TagNodeLong(SizeOnDisk);
        data["RandomSeed"] = new TagNodeLong(RandomSeed);

        if (_version != null && _version != 0)
        {
            data["version"] = new TagNodeInt(_version ?? 0);
        }

        if (_name != null)
        {
            data["LevelName"] = new TagNodeString(_name);
        }

        if (GeneratorName != null)
        {
            data["generatorName"] = new TagNodeString(GeneratorName);
        }

        if (_raining != null)
        {
            data["raining"] = new TagNodeByte(_raining ?? 0);
        }
        if (_thundering != null)
        {
            data["thundering"] = new TagNodeByte(_thundering ?? 0);
        }
        if (_rainTime != null)
        {
            data["rainTime"] = new TagNodeInt(_rainTime ?? 0);
        }
        if (_thunderTime != null)
        {
            data["thunderTime"] = new TagNodeInt(_thunderTime ?? 0);
        }

        if (_gameType != null)
        {
            data["GameType"] = new TagNodeInt(_gameType ?? 0);
        }
        if (_mapFeatures != null)
        {
            data["MapFeatures"] = new TagNodeByte(_mapFeatures ?? 0);
        }
        if (_hardcore != null)
        {
            data["hardcore"] = new TagNodeByte(_hardcore ?? 0);
        }

        if (_generatorOptions != null)
        {
            data["generatorOptions"] = new TagNodeString(_generatorOptions);
        }
        if (_generatorVersion != null)
        {
            data["generatorVersion"] = new TagNodeInt(_generatorVersion ?? 0);
        }
        if (_allowCommands != null)
        {
            data["allowCommands"] = new TagNodeByte(_allowCommands ?? 0);
        }
        if (_initialized != null)
        {
            data["initialized"] = new TagNodeByte(_initialized ?? 0);
        }
        if (_DayTime != null)
        {
            data["DayTime"] = new TagNodeLong(_DayTime ?? 0);
        }
        TagNodeCompound gr = new();
        gr["commandBlockOutput"] = new TagNodeString(GameRules.CommandBlockOutput ? "true" : "false");
        gr["doFireTick"] = new TagNodeString(GameRules.DoFireTick ? "true" : "false");
        gr["doMobLoot"] = new TagNodeString(GameRules.DoMobLoot ? "true" : "false");
        gr["doMobSpawning"] = new TagNodeString(GameRules.DoMobSpawning ? "true" : "false");
        gr["doTileDrops"] = new TagNodeString(GameRules.DoTileDrops ? "true" : "false");
        gr["keepInventory"] = new TagNodeString(GameRules.KeepInventory ? "true" : "false");
        gr["mobGriefing"] = new TagNodeString(GameRules.MobGriefing ? "true" : "false");
        data["GameRules"] = gr;

        if (Source != null)
        {
            data.MergeFrom(Source);
        }

        TagNodeCompound tree = new();
        tree.Add("Data", data);

        return tree;
    }

    /// <summary>
    /// Validate a Level subtree against a schema defintion.
    /// </summary>
    /// <param name="tree">The root node of a Level subtree.</param>
    /// <returns>Status indicating whether the tree was valid against the internal schema.</returns>
    public virtual bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, Schema).Verify();
    }

    #endregion


    #region ICopyable<Entity> Members

    /// <summary>
    /// Creates a deep-copy of the <see cref="Level"/>.
    /// </summary>
    /// <returns>A deep-copy of the <see cref="Level"/>, including a copy of the <see cref="Player"/>, if one is attached.</returns>
    public virtual Level Copy()
    {
        return new Level(this);
    }

    #endregion
}
