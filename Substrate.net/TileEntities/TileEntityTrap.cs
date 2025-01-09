using Substrate.Core;
using Substrate.Nbt;

namespace Substrate.TileEntities;

public class TileEntityTrap : TileEntity, IItemContainer
{
    public static readonly SchemaNodeCompound TrapSchema = TileEntity.Schema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
        new SchemaNodeList("Items", TagType.TAG_COMPOUND, ItemCollection.Schema),
    });

    public static string TypeId => "Trap";

    private const int _CAPACITY = 8;

    protected TileEntityTrap(string id)
        : base(id)
    {
        Items = new ItemCollection(_CAPACITY);
    }

    public TileEntityTrap()
        : this(TypeId)
    {
    }

    public TileEntityTrap(TileEntity te)
        : base(te)
    {
        TileEntityTrap tec = te as TileEntityTrap;
        Items = tec != null ? tec.Items.Copy() : new ItemCollection(_CAPACITY);
    }


    #region ICopyable<TileEntity> Members

    public override TileEntity Copy()
    {
        return new TileEntityTrap(this);
    }

    #endregion


    #region IItemContainer Members

    public ItemCollection Items { get; private set; }

    #endregion


    #region INBTObject<TileEntity> Members

    public override TileEntity LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null || base.LoadTree(tree) == null)
        {
            return null;
        }

        TagNodeList items = ctree["Items"].ToTagList();
        Items = new ItemCollection(_CAPACITY).LoadTree(items);

        return this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;
        tree["Items"] = Items.BuildTree();

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, TrapSchema).Verify();
    }

    #endregion
}
