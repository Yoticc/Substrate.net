namespace Substrate.TileEntities;

using Substrate.Nbt;

public class TileEntityBeacon : TileEntity
{
    public static readonly SchemaNodeCompound BeaconSchema = TileEntity.Schema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
        new SchemaNodeScaler("Levels", TagType.TAG_INT),
        new SchemaNodeScaler("Primary", TagType.TAG_INT),
        new SchemaNodeScaler("Secondary", TagType.TAG_INT),
    });

    public static string TypeId => "Beacon";

    public int Levels { get; set; }

    public int Primary { get; set; }

    public int Secondary { get; set; }

    protected TileEntityBeacon(string id)
        : base(id)
    {
    }

    public TileEntityBeacon()
        : this(TypeId)
    {
    }

    public TileEntityBeacon(TileEntity te)
        : base(te)
    {
        TileEntityBeacon tes = te as TileEntityBeacon;
        if (tes != null)
        {
            Levels = tes.Levels;
            Primary = tes.Primary;
            Secondary = tes.Secondary;
        }
    }


    #region ICopyable<TileEntity> Members

    public override TileEntity Copy()
    {
        return new TileEntityBeacon(this);
    }

    #endregion


    #region INBTObject<TileEntity> Members

    public override TileEntity LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null || base.LoadTree(tree) == null)
        {
            return null;
        }

        Levels = ctree["Levels"].ToTagInt();
        Primary = ctree["Primary"].ToTagInt();
        Secondary = ctree["Secondary"].ToTagInt();

        return this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;
        tree["Levels"] = new TagNodeInt(Levels);
        tree["Primary"] = new TagNodeInt(Primary);
        tree["Secondary"] = new TagNodeInt(Secondary);

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, BeaconSchema).Verify();
    }

    #endregion
}
