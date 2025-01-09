﻿namespace Substrate.Entities;

using Substrate.Nbt;

public class EntityMinecart : TypedEntity
{
    public enum CartType
    {
        EMPTY = 0,
        CHEST = 1,
        FURNACE = 2,
    }

    public static readonly SchemaNodeCompound MinecartSchema = TypedEntity.Schema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
        new SchemaNodeScaler("Type", TagType.TAG_INT),
    });

    public static string TypeId => "Minecart";

    public CartType Type { get; private set; }

    protected EntityMinecart(string id)
        : base(id)
    {
    }

    public EntityMinecart()
        : this(TypeId)
    {
    }

    public EntityMinecart(TypedEntity e)
        : base(e)
    {
        EntityMinecart e2 = e as EntityMinecart;
        if (e2 != null)
        {
            Type = e2.Type;
        }
    }


    #region INBTObject<Entity> Members

    public override TypedEntity LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null || base.LoadTree(tree) == null)
        {
            return null;
        }

        Type = (CartType)ctree["Type"].ToTagInt().Data;

        switch (Type)
        {
            case CartType.EMPTY:
                return this;
            case CartType.CHEST:
                return new EntityMinecartChest().LoadTreeSafe(tree);
            case CartType.FURNACE:
                return new EntityMinecartFurnace().LoadTreeSafe(tree);
            default:
                return this;
        }
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;
        tree["Type"] = new TagNodeInt((int)Type);

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, MinecartSchema).Verify();
    }

    #endregion


    #region ICopyable<Entity> Members

    public override TypedEntity Copy()
    {
        return new EntityMinecart(this);
    }

    #endregion
}
