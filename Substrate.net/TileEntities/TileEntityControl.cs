namespace Substrate.TileEntities;

using Substrate.Nbt;

public class TileEntityControl : TileEntity
{
    public static readonly SchemaNodeCompound ControlSchema = TileEntity.Schema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
        new SchemaNodeScaler("Command", TagType.TAG_STRING),
    });

    public static string TypeId => "Control";

    public string Command { get; set; }

    protected TileEntityControl(string id)
        : base(id)
    {
    }

    public TileEntityControl()
        : this(TypeId)
    {
    }

    public TileEntityControl(TileEntity te)
        : base(te)
    {
        TileEntityControl tes = te as TileEntityControl;
        if (tes != null)
        {
            Command = tes.Command;
        }
    }


    #region ICopyable<TileEntity> Members

    public override TileEntity Copy()
    {
        return new TileEntityControl(this);
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

        Command = ctree["Command"].ToTagString();

        return this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;
        tree["Command"] = new TagNodeString(Command);

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, ControlSchema).Verify();
    }

    #endregion
}
