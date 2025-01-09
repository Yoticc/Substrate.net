namespace Substrate.Entities;

using Substrate.Nbt;

public class EntityWolf : EntityAnimal
{
    public static readonly SchemaNodeCompound WolfSchema = AnimalSchema.MergeInto(new SchemaNodeCompound("")
    {
        new SchemaNodeString("id", TypeId),
        new SchemaNodeScaler("Owner", TagType.TAG_STRING),
        new SchemaNodeScaler("Sitting", TagType.TAG_BYTE),
        new SchemaNodeScaler("Angry", TagType.TAG_BYTE),
    });

    public static new string TypeId => "Wolf";

    public string Owner { get; set; }

    public bool IsSitting { get; set; }

    public bool IsAngry { get; set; }

    protected EntityWolf(string id)
        : base(id)
    {
    }

    public EntityWolf()
        : this(TypeId)
    {
    }

    public EntityWolf(TypedEntity e)
        : base(e)
    {
        EntityWolf e2 = e as EntityWolf;
        if (e2 != null)
        {
            Owner = e2.Owner;
            IsSitting = e2.IsSitting;
            IsAngry = e2.IsAngry;
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

        Owner = ctree["Owner"].ToTagString();
        IsSitting = ctree["Sitting"].ToTagByte() == 1;
        IsAngry = ctree["Angry"].ToTagByte() == 1;

        return this;
    }

    public override TagNode BuildTree()
    {
        TagNodeCompound tree = base.BuildTree() as TagNodeCompound;
        tree["Owner"] = new TagNodeString(Owner);
        tree["Sitting"] = new TagNodeByte((byte)(IsSitting ? 1 : 0));
        tree["Angry"] = new TagNodeByte((byte)(IsAngry ? 1 : 0));

        return tree;
    }

    public override bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, WolfSchema).Verify();
    }

    #endregion


    #region ICopyable<Entity> Members

    public override TypedEntity Copy()
    {
        return new EntityWolf(this);
    }

    #endregion
}
