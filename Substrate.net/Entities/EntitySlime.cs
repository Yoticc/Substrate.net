﻿namespace Substrate.Entities;

using Substrate.Nbt;

public class EntitySlime : EntityMob
{
    public static readonly SchemaNodeCompound SlimeSchema = MobSchema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
        new SchemaNodeScaler("Size", TagType.TAG_INT),
    });

    public static new string TypeId => "Slime";

    public int Size { get; set; }

    protected EntitySlime(string id)
        : base(id)
    {
    }

    public EntitySlime()
        : this(TypeId)
    {
    }

    public EntitySlime(TypedEntity e)
        : base(e)
    {
        EntitySlime e2 = e as EntitySlime;
        if (e2 != null)
        {
            Size = e2.Size;
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

        Size = ctree["Size"].ToTagInt();

        return this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;
        tree["Size"] = new TagNodeInt(Size);

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, SlimeSchema).Verify();
    }

    #endregion


    #region ICopyable<Entity> Members

    public override TypedEntity Copy()
    {
        return new EntitySlime(this);
    }

    #endregion
}
