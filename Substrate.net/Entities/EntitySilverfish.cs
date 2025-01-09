namespace Substrate.Entities;

using Substrate.Nbt;

public class EntitySilverfish : EntityMob
{
    public static readonly SchemaNodeCompound SilverfishSchema = MobSchema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
    });

    public static new string TypeId => "Silverfish";

    protected EntitySilverfish(string id)
        : base(id)
    {
    }

    public EntitySilverfish()
        : this(TypeId)
    {
    }

    public EntitySilverfish(TypedEntity e)
        : base(e)
    {
    }


    #region INBTObject<Entity> Members

    public override TypedEntity LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        return ctree == null || base.LoadTree(tree) == null ? null : (TypedEntity)this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, SilverfishSchema).Verify();
    }

    #endregion


    #region ICopyable<Entity> Members

    public override TypedEntity Copy()
    {
        return new EntitySilverfish(this);
    }

    #endregion
}
