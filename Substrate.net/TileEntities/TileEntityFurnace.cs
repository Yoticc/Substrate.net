using Substrate.Core;
using Substrate.Nbt;

namespace Substrate.TileEntities;

public class TileEntityFurnace : TileEntity, IItemContainer
{
    public static readonly SchemaNodeCompound FurnaceSchema = TileEntity.Schema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
        new SchemaNodeScaler("BurnTime", TagType.TAG_SHORT),
        new SchemaNodeScaler("CookTime", TagType.TAG_SHORT),
        new SchemaNodeList("Items", TagType.TAG_COMPOUND, ItemCollection.Schema),
    });

    public static string TypeId => "Furnace";

    private const int _CAPACITY = 3;

    private short _burnTime;
    private short _cookTime;

    public int BurnTime
    {
        get => _burnTime;
        set => _burnTime = (short)value;
    }

    public int CookTime
    {
        get => _cookTime;
        set => _cookTime = (short)value;
    }

    protected TileEntityFurnace(string id)
        : base(id)
    {
        Items = new ItemCollection(_CAPACITY);
    }

    public TileEntityFurnace()
        : this(TypeId)
    {
    }

    public TileEntityFurnace(TileEntity te)
        : base(te)
    {
        TileEntityFurnace tec = te as TileEntityFurnace;
        if (tec != null)
        {
            _cookTime = tec._cookTime;
            _burnTime = tec._burnTime;
            Items = tec.Items.Copy();
        }
        else
        {
            Items = new ItemCollection(_CAPACITY);
        }
    }


    #region ICopyable<TileEntity> Members

    public override TileEntity Copy()
    {
        return new TileEntityFurnace(this);
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

        _burnTime = ctree["BurnTime"].ToTagShort();
        _cookTime = ctree["CookTime"].ToTagShort();

        TagNodeList items = ctree["Items"].ToTagList();
        Items = new ItemCollection(_CAPACITY).LoadTree(items);

        return this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;
        tree["BurnTime"] = new TagNodeShort(_burnTime);
        tree["CookTime"] = new TagNodeShort(_cookTime);
        tree["Items"] = Items.BuildTree();

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, FurnaceSchema).Verify();
    }

    #endregion
}
