using Substrate.Core;
using Substrate.Nbt;

namespace Substrate.Data;

public class BetaDataManager : DataManager, INbtObject<BetaDataManager>
{
    private static SchemaNodeCompound _schema = new()
    {
        new SchemaNodeScaler("map", TagType.TAG_SHORT),
    };

    private TagNodeCompound _source;

    private NbtWorld _world;

    private short _mapId;

    public BetaDataManager(NbtWorld world)
    {
        _world = world;

        Maps = new MapManager(_world);
    }

    public override int CurrentMapId
    {
        get => _mapId;
        set => _mapId = (short)value;
    }

    public new MapManager Maps { get; }

    protected override IMapManager GetMapManager()
    {
        return Maps;
    }

    public override bool Save()
    {
        if (_world == null)
        {
            return false;
        }

        try
        {
            string path = Path.Combine(_world.Path, _world.DataDirectory);
            NBTFile nf = new(Path.Combine(path, "idcounts.dat"));

            using Stream zipstr = nf.GetDataOutputStream(CompressionType.None);
            if (zipstr == null)
            {
                NbtIOException nex = new("Failed to initialize uncompressed NBT stream for output");
                nex.Data["DataManager"] = this;
                throw nex;
            }

            new NbtTree(BuildTree() as TagNodeCompound).WriteTo(zipstr);

            return true;
        }
        catch (Exception ex)
        {
            Exception lex = new("Could not save idcounts.dat file.", ex);
            lex.Data["DataManager"] = this;
            throw lex;
        }
    }

    #region INBTObject<DataManager>

    public virtual BetaDataManager LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null)
        {
            return null;
        }

        _mapId = ctree["map"].ToTagShort();

        _source = ctree.Copy() as TagNodeCompound;

        return this;
    }

    public virtual BetaDataManager LoadTreeSafe(TagNode tree)
    {
        return !ValidateTree(tree) ? null : LoadTree(tree);
    }

    public virtual TagNode BuildTree()
    {
        TagNodeCompound tree = new();

        tree["map"] = new TagNodeLong(_mapId);

        if (_source != null)
        {
            tree.MergeFrom(_source);
        }

        return tree;
    }

    public virtual bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, _schema).Verify();
    }

    #endregion
}
