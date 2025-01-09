using Substrate.Core;
using Substrate.Nbt;

namespace Substrate;

public class AnvilChunk : IChunk, INbtObject<AnvilChunk>, ICopyable<AnvilChunk>
{
    public static SchemaNodeCompound LevelSchema = new()
    {
        new SchemaNodeCompound("Level")
        {
            new SchemaNodeList("Sections", TagType.TAG_COMPOUND, new SchemaNodeCompound() {
                new SchemaNodeArray("Blocks", 4096),
                new SchemaNodeArray("Data", 2048),
                new SchemaNodeArray("SkyLight", 2048),
                new SchemaNodeArray("BlockLight", 2048),
                new SchemaNodeScaler("Y", TagType.TAG_BYTE),
                new SchemaNodeArray("Add", 2048, SchemaOptions.OPTIONAL),
            }),
            new SchemaNodeArray("Biomes", 256, SchemaOptions.OPTIONAL),
            new SchemaNodeIntArray("HeightMap", 256),
            new SchemaNodeList("Entities", TagType.TAG_COMPOUND, SchemaOptions.CREATE_ON_MISSING),
            new SchemaNodeList("TileEntities", TagType.TAG_COMPOUND, TileEntity.Schema, SchemaOptions.CREATE_ON_MISSING),
            new SchemaNodeList("TileTicks", TagType.TAG_COMPOUND, TileTick.Schema, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("LastUpdate", TagType.TAG_LONG, SchemaOptions.CREATE_ON_MISSING),
            new SchemaNodeScaler("xPos", TagType.TAG_INT),
            new SchemaNodeScaler("zPos", TagType.TAG_INT),
            new SchemaNodeScaler("TerrainPopulated", TagType.TAG_BYTE, SchemaOptions.CREATE_ON_MISSING),
        },
    };

    private const int XDIM = 16;
    private const int YDIM = 256;
    private const int ZDIM = 16;
    private IDataArray3 _blocks;
    private IDataArray3 _data;
    private IDataArray3 _blockLight;
    private IDataArray3 _skyLight;

    private ZXIntArray _heightMap;
    private ZXByteArray _biomes;

    private TagNodeList _entities;
    private TagNodeList _tileEntities;
    private TagNodeList _tileTicks;

    private AnvilChunk()
    {
        Sections = new AnvilSection[16];
    }

    public int X { get; private set; }

    public int Z { get; private set; }

    public AnvilSection[] Sections { get; private set; }

    public AlphaBlockCollection Blocks { get; private set; }

    public AnvilBiomeCollection Biomes { get; private set; }

    public EntityCollection Entities { get; private set; }

    public NbtTree Tree { get; private set; }

    public bool IsTerrainPopulated
    {
        get => Tree.Root["Level"].ToTagCompound()["TerrainPopulated"].ToTagByte() == 1;
        set => Tree.Root["Level"].ToTagCompound()["TerrainPopulated"].ToTagByte().Data = (byte)(value ? 1 : 0);
    }

    public static AnvilChunk Create(int x, int z)
    {
        AnvilChunk c = new();

        c.X = x;
        c.Z = z;

        c.BuildNBTTree();
        return c;
    }

    public static AnvilChunk Create(NbtTree tree)
    {
        AnvilChunk c = new();

        return c.LoadTree(tree.Root);
    }

    public static AnvilChunk CreateVerified(NbtTree tree)
    {
        AnvilChunk c = new();

        return c.LoadTreeSafe(tree.Root);
    }

    /// <summary>
    /// Updates the chunk's global world coordinates.
    /// </summary>
    /// <param name="x">Global X-coordinate.</param>
    /// <param name="z">Global Z-coordinate.</param>
    public virtual void SetLocation(int x, int z)
    {
        int diffx = (x - X) * XDIM;
        int diffz = (z - Z) * ZDIM;

        // Update chunk position

        X = x;
        Z = z;

        Tree.Root["Level"].ToTagCompound()["xPos"].ToTagInt().Data = x;
        Tree.Root["Level"].ToTagCompound()["zPos"].ToTagInt().Data = z;

        // Update tile entity coordinates

        List<TileEntity> tileEntites = new();
        foreach (TagNodeCompound tag in _tileEntities)
        {
            TileEntity te = TileEntityFactory.Create(tag);
            te ??= TileEntity.FromTreeSafe(tag);

            if (te != null)
            {
                te.MoveBy(diffx, 0, diffz);
                tileEntites.Add(te);
            }
        }

        _tileEntities.Clear();
        foreach (TileEntity te in tileEntites)
        {
            _tileEntities.Add(te.BuildTree());
        }

        // Update tile tick coordinates

        if (_tileTicks != null)
        {
            List<TileTick> tileTicks = new();
            foreach (TagNodeCompound tag in _tileTicks)
            {
                TileTick tt = TileTick.FromTreeSafe(tag);

                if (tt != null)
                {
                    tt.MoveBy(diffx, 0, diffz);
                    tileTicks.Add(tt);
                }
            }

            _tileTicks.Clear();
            foreach (TileTick tt in tileTicks)
            {
                _tileTicks.Add(tt.BuildTree());
            }
        }

        // Update entity coordinates

        List<TypedEntity> entities = new();
        foreach (TypedEntity entity in Entities)
        {
            entity.MoveBy(diffx, 0, diffz);
            entities.Add(entity);
        }

        _entities.Clear();
        foreach (TypedEntity entity in entities)
        {
            Entities.Add(entity);
        }
    }

    public bool Save(Stream outStream)
    {
        if (outStream == null || !outStream.CanWrite)
        {
            return false;
        }

        BuildConditional();

        NbtTree tree = new();
        tree.Root["Level"] = BuildTree();

        tree.WriteTo(outStream);

        return true;
    }

    #region INbtObject<AnvilChunk> Members

    public AnvilChunk LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null)
        {
            return null;
        }

        Tree = new NbtTree(ctree);

        TagNodeCompound level = Tree.Root["Level"] as TagNodeCompound;

        TagNodeList sections = level["Sections"] as TagNodeList;
        foreach (TagNodeCompound section in sections)
        {
            AnvilSection anvilSection = new(section);
            if (anvilSection.Y < 0 || anvilSection.Y >= Sections.Length)
                continue;
            Sections[anvilSection.Y] = anvilSection;
        }

        FusedDataArray3[] blocksBA = new FusedDataArray3[Sections.Length];
        YZXNibbleArray[] dataBA = new YZXNibbleArray[Sections.Length];
        YZXNibbleArray[] skyLightBA = new YZXNibbleArray[Sections.Length];
        YZXNibbleArray[] blockLightBA = new YZXNibbleArray[Sections.Length];

        for (int i = 0; i < Sections.Length; i++)
        {
            if (Sections[i] == null)
                Sections[i] = new AnvilSection(i);

            blocksBA[i] = new FusedDataArray3(Sections[i].AddBlocks, Sections[i].Blocks);
            dataBA[i] = Sections[i].Data;
            skyLightBA[i] = Sections[i].SkyLight;
            blockLightBA[i] = Sections[i].BlockLight;
        }

        _blocks = new CompositeDataArray3(blocksBA);
        _data = new CompositeDataArray3(dataBA);
        _skyLight = new CompositeDataArray3(skyLightBA);
        _blockLight = new CompositeDataArray3(blockLightBA);

        _heightMap = new ZXIntArray(XDIM, ZDIM, level["HeightMap"] as TagNodeIntArray);

        if (level.ContainsKey("Biomes"))
            _biomes = new ZXByteArray(XDIM, ZDIM, level["Biomes"] as TagNodeByteArray);
        else
        {
            level["Biomes"] = new TagNodeByteArray(new byte[256]);
            _biomes = new ZXByteArray(XDIM, ZDIM, level["Biomes"] as TagNodeByteArray);
            for (int x = 0; x < XDIM; x++)
                for (int z = 0; z < ZDIM; z++)
                    _biomes[x, z] = BiomeType.Default;
        }

        _entities = level["Entities"] as TagNodeList;
        _tileEntities = level["TileEntities"] as TagNodeList;

        _tileTicks = level.ContainsKey("TileTicks") ? level["TileTicks"] as TagNodeList : new TagNodeList(TagType.TAG_COMPOUND);

        // List-type patch up
        if (_entities.Count == 0)
        {
            level["Entities"] = new TagNodeList(TagType.TAG_COMPOUND);
            _entities = level["Entities"] as TagNodeList;
        }

        if (_tileEntities.Count == 0)
        {
            level["TileEntities"] = new TagNodeList(TagType.TAG_COMPOUND);
            _tileEntities = level["TileEntities"] as TagNodeList;
        }

        if (_tileTicks.Count == 0)
        {
            level["TileTicks"] = new TagNodeList(TagType.TAG_COMPOUND);
            _tileTicks = level["TileTicks"] as TagNodeList;
        }

        X = level["xPos"].ToTagInt();
        Z = level["zPos"].ToTagInt();

        Blocks = new AlphaBlockCollection(_blocks, _data, _blockLight, _skyLight, _heightMap, _tileEntities, _tileTicks);
        Entities = new EntityCollection(_entities);
        Biomes = new AnvilBiomeCollection(_biomes);

        return this;
    }

    public AnvilChunk LoadTreeSafe(TagNode tree)
    {
        return !ValidateTree(tree) ? null : LoadTree(tree);
    }

    private bool ShouldIncludeSection(AnvilSection section)
    {
        int y = (section.Y + 1) * section.Blocks.YDim;
        for (int i = 0; i < _heightMap.Length; i++)
            if (_heightMap[i] > y)
                return true;

        return !section.CheckEmpty();
    }

    public TagNode BuildTree()
    {
        TagNodeCompound level = Tree.Root["Level"] as TagNodeCompound;
        TagNodeCompound levelCopy = new();
        foreach (KeyValuePair<string, TagNode> node in level)
            levelCopy.Add(node.Key, node.Value);

        TagNodeList sections = new(TagType.TAG_COMPOUND);
        for (int i = 0; i < Sections.Length; i++)
            if (ShouldIncludeSection(Sections[i]))
                sections.Add(Sections[i].BuildTree());

        levelCopy["Sections"] = sections;

        if (_tileTicks.Count == 0)
            levelCopy.Remove("TileTicks");

        return levelCopy;
    }

    public bool ValidateTree(TagNode tree)
    {
        NbtVerifier v = new(tree, LevelSchema);
        return v.Verify();
    }

    #endregion

    #region ICopyable<AnvilChunk> Members

    public AnvilChunk Copy()
    {
        return AnvilChunk.Create(Tree.Copy());
    }

    #endregion

    private void BuildConditional()
    {
        TagNodeCompound level = Tree.Root["Level"] as TagNodeCompound;
        if (_tileTicks != Blocks.TileTicks && Blocks.TileTicks.Count > 0)
        {
            _tileTicks = Blocks.TileTicks;
            level["TileTicks"] = _tileTicks;
        }
    }

    private void BuildNBTTree()
    {
        int elements2 = XDIM * ZDIM;

        Sections = new AnvilSection[16];
        TagNodeList sections = new(TagType.TAG_COMPOUND);

        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new AnvilSection(i);
            sections.Add(Sections[i].BuildTree());
        }

        FusedDataArray3[] blocksBA = new FusedDataArray3[Sections.Length];
        YZXNibbleArray[] dataBA = new YZXNibbleArray[Sections.Length];
        YZXNibbleArray[] skyLightBA = new YZXNibbleArray[Sections.Length];
        YZXNibbleArray[] blockLightBA = new YZXNibbleArray[Sections.Length];

        for (int i = 0; i < Sections.Length; i++)
        {
            blocksBA[i] = new FusedDataArray3(Sections[i].AddBlocks, Sections[i].Blocks);
            dataBA[i] = Sections[i].Data;
            skyLightBA[i] = Sections[i].SkyLight;
            blockLightBA[i] = Sections[i].BlockLight;
        }

        _blocks = new CompositeDataArray3(blocksBA);
        _data = new CompositeDataArray3(dataBA);
        _skyLight = new CompositeDataArray3(skyLightBA);
        _blockLight = new CompositeDataArray3(blockLightBA);

        TagNodeIntArray heightMap = new(new int[elements2]);
        _heightMap = new ZXIntArray(XDIM, ZDIM, heightMap);

        TagNodeByteArray biomes = new(new byte[elements2]);
        _biomes = new ZXByteArray(XDIM, ZDIM, biomes);
        for (int x = 0; x < XDIM; x++)
            for (int z = 0; z < ZDIM; z++)
                _biomes[x, z] = BiomeType.Default;

        _entities = new TagNodeList(TagType.TAG_COMPOUND);
        _tileEntities = new TagNodeList(TagType.TAG_COMPOUND);
        _tileTicks = new TagNodeList(TagType.TAG_COMPOUND);

        TagNodeCompound level = new();
        level.Add("Sections", sections);
        level.Add("HeightMap", heightMap);
        level.Add("Biomes", biomes);
        level.Add("Entities", _entities);
        level.Add("TileEntities", _tileEntities);
        level.Add("TileTicks", _tileTicks);
        level.Add("LastUpdate", new TagNodeLong(Timestamp()));
        level.Add("xPos", new TagNodeInt(X));
        level.Add("zPos", new TagNodeInt(Z));
        level.Add("TerrainPopulated", new TagNodeByte());

        Tree = new NbtTree();
        Tree.Root.Add("Level", level);

        Blocks = new AlphaBlockCollection(_blocks, _data, _blockLight, _skyLight, _heightMap, _tileEntities);
        Entities = new EntityCollection(_entities);
    }

    private int Timestamp()
    {
        DateTime epoch = new(1970, 1, 1, 0, 0, 0, 0);
        return (int)((DateTime.UtcNow - epoch).Ticks / (10000L * 1000L));
    }
}
