using Substrate.Core;
using Substrate.Nbt;

namespace Substrate.Entities;

public class EntityMinecartChest : EntityMinecart, IItemContainer
{
    public static readonly SchemaNodeCompound MinecartChestSchema = MinecartSchema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeList("Items", TagType.TAG_COMPOUND, ItemCollection.Schema),
    });

    public static new string TypeId => EntityMinecart.TypeId;

    private static int _CAPACITY = 27;

    protected EntityMinecartChest(string id)
        : base(id)
    {
        Items = new ItemCollection(_CAPACITY);
    }

    public EntityMinecartChest()
        : base()
    {
        Items = new ItemCollection(_CAPACITY);
    }

    public EntityMinecartChest(TypedEntity e)
        : base(e)
    {
        EntityMinecartChest e2 = e as EntityMinecartChest;
        if (e2 != null)
        {
            Items = e2.Items.Copy();
        }
    }

    #region IItemContainer Members

    public ItemCollection Items { get; private set; }

    #endregion


    #region INBTObject<Entity> Members

    public override TypedEntity LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null || base.LoadTree(tree) == null)
        {
            return null;
        }

        TagNodeList items = ctree["Items"].ToTagList();
        Items = Items.LoadTree(items);

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
        return new NbtVerifier(tree, MinecartChestSchema).Verify();
    }

    #endregion


    #region ICopyable<Entity> Members

    public override TypedEntity Copy()
    {
        return new EntityMinecartChest(this);
    }

    #endregion
}
