namespace Substrate.TileEntities;

using Substrate.Nbt;

public class TileEntityEnchantmentTable : TileEntity
{
    public static readonly SchemaNodeCompound EnchantTableSchema = TileEntity.Schema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
    });

    public static string TypeId => "EnchantTable";

    protected TileEntityEnchantmentTable(string id)
        : base(id)
    {
    }

    public TileEntityEnchantmentTable()
        : this(TypeId)
    {
    }

    public TileEntityEnchantmentTable(TileEntity te)
        : base(te)
    {
    }


    #region ICopyable<TileEntity> Members

    public override TileEntity Copy()
    {
        return new TileEntityEnchantmentTable(this);
    }

    #endregion


    #region INBTObject<TileEntity> Members

    public override TileEntity LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        return ctree == null || base.LoadTree(tree) == null ? null : (TileEntity)this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, EnchantTableSchema).Verify();
    }

    #endregion
}
