using Substrate.Core;
using Substrate.Nbt;

namespace Substrate;

/// <summary>
/// Represents an item (or item stack) within an item slot.
/// </summary>
public class Item : INbtObject<Item>, ICopyable<Item>
{
    private short _id;
    private byte _count;
    private short _damage;

    private List<Enchantment> _enchantments;

    /// <summary>
    /// Constructs an empty <see cref="Item"/> instance.
    /// </summary>
    public Item()
    {
        _enchantments = new List<Enchantment>();
        Source = new TagNodeCompound();
    }

    /// <summary>
    /// Constructs an <see cref="Item"/> instance representing the given item id.
    /// </summary>
    /// <param name="id">An item id.</param>
    public Item(int id)
        : this()
    {
        _id = (short)id;
    }

    #region Properties

    /// <summary>
    /// Gets an <see cref="ItemInfo"/> entry for this item's type.
    /// </summary>
    public ItemInfo Info => ItemInfo.ItemTable[_id];

    /// <summary>
    /// Gets or sets the current type (id) of the item.
    /// </summary>
    public int ID
    {
        get => _id;
        set => _id = (short)value;
    }

    /// <summary>
    /// Gets or sets the damage value of the item.
    /// </summary>
    /// <remarks>The damage value may represent a generic data value for some items.</remarks>
    public int Damage
    {
        get => _damage;
        set => _damage = (short)value;
    }

    /// <summary>
    /// Gets or sets the number of this item stacked together in an item slot.
    /// </summary>
    public int Count
    {
        get => _count;
        set => _count = (byte)value;
    }

    /// <summary>
    /// Gets the list of <see cref="Enchantment"/>s applied to this item.
    /// </summary>
    public IList<Enchantment> Enchantments => _enchantments;

    /// <summary>
    /// Gets the source <see cref="TagNodeCompound"/> used to create this <see cref="Item"/> if it exists.
    /// </summary>
    public TagNodeCompound Source { get; private set; }

    /// <summary>
    /// Gets a <see cref="SchemaNode"/> representing the schema of an item.
    /// </summary>
    public static SchemaNodeCompound Schema { get; } = new("")
    {
        new SchemaNodeScaler("id", TagType.TAG_SHORT),
        new SchemaNodeScaler("Damage", TagType.TAG_SHORT),
        new SchemaNodeScaler("Count", TagType.TAG_BYTE),
        new SchemaNodeCompound("tag", new SchemaNodeCompound("") {
            new SchemaNodeList("ench", TagType.TAG_COMPOUND, Enchantment.Schema, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("title", TagType.TAG_STRING, SchemaOptions.OPTIONAL),
            new SchemaNodeScaler("author", TagType.TAG_STRING, SchemaOptions.OPTIONAL),
            new SchemaNodeList("pages", TagType.TAG_STRING, SchemaOptions.OPTIONAL),
        }, SchemaOptions.OPTIONAL),
    };

    #endregion

    #region ICopyable<Item> Members

    /// <inheritdoc/>
    public Item Copy()
    {
        Item item = new();
        item._id = _id;
        item._count = _count;
        item._damage = _damage;

        foreach (Enchantment e in _enchantments)
        {
            item._enchantments.Add(e.Copy());
        }

        if (Source != null)
        {
            item.Source = Source.Copy() as TagNodeCompound;
        }

        return item;
    }

    #endregion

    #region INBTObject<Item> Members

    /// <inheritdoc/>
    public Item LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null)
        {
            return null;
        }

        _enchantments.Clear();

        _id = ctree["id"].ToTagShort();
        _count = ctree["Count"].ToTagByte();
        _damage = ctree["Damage"].ToTagShort();

        if (ctree.ContainsKey("tag"))
        {
            TagNodeCompound tagtree = ctree["tag"].ToTagCompound();
            if (tagtree.ContainsKey("ench"))
            {
                TagNodeList enchList = tagtree["ench"].ToTagList();

                foreach (TagNode tag in enchList)
                {
                    _enchantments.Add(new Enchantment().LoadTree(tag));
                }
            }
        }

        Source = ctree.Copy() as TagNodeCompound;

        return this;
    }

    /// <inheritdoc/>
    public Item LoadTreeSafe(TagNode tree)
    {
        return !ValidateTree(tree) ? null : LoadTree(tree);
    }

    /// <inheritdoc/>
    public TagNode BuildTree()
    {
        TagNodeCompound tree = new();
        tree["id"] = new TagNodeShort(_id);
        tree["Count"] = new TagNodeByte(_count);
        tree["Damage"] = new TagNodeShort(_damage);

        if (_enchantments.Count > 0)
        {
            TagNodeList enchList = new(TagType.TAG_COMPOUND);
            foreach (Enchantment e in _enchantments)
            {
                enchList.Add(e.BuildTree());
            }

            TagNodeCompound tagtree = new();
            tagtree["ench"] = enchList;

            if (Source != null && Source.ContainsKey("tag"))
            {
                tagtree.MergeFrom(Source["tag"].ToTagCompound());
            }

            tree["tag"] = tagtree;
        }

        if (Source != null)
        {
            tree.MergeFrom(Source);
        }

        return tree;
    }

    /// <inheritdoc/>
    public bool ValidateTree(TagNode tree)
    {
        return new NbtVerifier(tree, Schema).Verify();
    }

    #endregion
}
