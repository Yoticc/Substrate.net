using Substrate.Core;
using Substrate.Nbt;

namespace Substrate;

public class AnvilSection : INbtObject<AnvilSection>, ICopyable<AnvilSection>
{
    public static SchemaNodeCompound SectionSchema = new()
    {
        new SchemaNodeArray("Blocks", 4096),
        new SchemaNodeArray("Data", 2048),
        new SchemaNodeArray("SkyLight", 2048),
        new SchemaNodeArray("BlockLight", 2048),
        new SchemaNodeScaler("Y", TagType.TAG_BYTE),
        new SchemaNodeArray("Add", 2048, SchemaOptions.OPTIONAL),
    };

    private const int XDIM = 16;
    private const int YDIM = 16;
    private const int ZDIM = 16;

    private const int MIN_Y = 0;
    private const int MAX_Y = 15;

    private TagNodeCompound _tree;

    private byte _y;

    private AnvilSection()
    {
    }

    public AnvilSection(int y)
    {
        if (y < MIN_Y || y > MAX_Y)
            throw new ArgumentOutOfRangeException();

        _y = (byte)y;
        BuildNbtTree();
    }

    public AnvilSection(TagNodeCompound tree)
    {
        LoadTree(tree);
    }

    public int Y
    {
        get => _y;
        set
        {
            if (value < MIN_Y || value > MAX_Y)
                throw new ArgumentOutOfRangeException();

            _y = (byte)value;
            _tree["Y"].ToTagByte().Data = _y;
        }
    }

    public YZXByteArray Blocks { get; private set; }

    public YZXNibbleArray Data { get; private set; }

    public YZXNibbleArray BlockLight { get; private set; }

    public YZXNibbleArray SkyLight { get; private set; }

    public YZXNibbleArray AddBlocks { get; private set; }

    public bool CheckEmpty()
    {
        return CheckBlocksEmpty() && CheckAddBlocksEmpty();
    }

    private bool CheckBlocksEmpty()
    {
        for (int i = 0; i < Blocks.Length; i++)
            if (Blocks[i] != 0)
                return false;
        return true;
    }

    private bool CheckAddBlocksEmpty()
    {
        if (AddBlocks != null)
            for (int i = 0; i < AddBlocks.Length; i++)
                if (AddBlocks[i] != 0)
                    return false;
        return true;
    }

    #region INbtObject<AnvilSection> Members

    public AnvilSection LoadTree(TagNode tree)
    {
        TagNodeCompound ctree = tree as TagNodeCompound;
        if (ctree == null)
        {
            return null;
        }

        _y = ctree["Y"] as TagNodeByte;

        Blocks = new YZXByteArray(XDIM, YDIM, ZDIM, ctree["Blocks"] as TagNodeByteArray);
        Data = new YZXNibbleArray(XDIM, YDIM, ZDIM, ctree["Data"] as TagNodeByteArray);
        SkyLight = new YZXNibbleArray(XDIM, YDIM, ZDIM, ctree["SkyLight"] as TagNodeByteArray);
        BlockLight = new YZXNibbleArray(XDIM, YDIM, ZDIM, ctree["BlockLight"] as TagNodeByteArray);

        if (!ctree.ContainsKey("Add"))
            ctree["Add"] = new TagNodeByteArray(new byte[2048]);
        AddBlocks = new YZXNibbleArray(XDIM, YDIM, ZDIM, ctree["Add"] as TagNodeByteArray);

        _tree = ctree;

        return this;
    }

    public AnvilSection LoadTreeSafe(TagNode tree)
    {
        return !ValidateTree(tree) ? null : LoadTree(tree);
    }

    public TagNode BuildTree()
    {
        TagNodeCompound copy = new();
        foreach (KeyValuePair<string, TagNode> node in _tree)
        {
            copy.Add(node.Key, node.Value);
        }

        if (CheckAddBlocksEmpty())
            copy.Remove("Add");

        return copy;
    }

    public bool ValidateTree(TagNode tree)
    {
        NbtVerifier v = new(tree, SectionSchema);
        return v.Verify();
    }

    #endregion

    #region ICopyable<AnvilSection> Members

    public AnvilSection Copy()
    {
        return new AnvilSection().LoadTree(_tree.Copy());
    }

    #endregion

    private void BuildNbtTree()
    {
        int elements3 = XDIM * YDIM * ZDIM;

        TagNodeByteArray blocks = new(new byte[elements3]);
        TagNodeByteArray data = new(new byte[elements3 >> 1]);
        TagNodeByteArray skyLight = new(new byte[elements3 >> 1]);
        TagNodeByteArray blockLight = new(new byte[elements3 >> 1]);
        TagNodeByteArray addBlocks = new(new byte[elements3 >> 1]);

        Blocks = new YZXByteArray(XDIM, YDIM, ZDIM, blocks);
        Data = new YZXNibbleArray(XDIM, YDIM, ZDIM, data);
        SkyLight = new YZXNibbleArray(XDIM, YDIM, ZDIM, skyLight);
        BlockLight = new YZXNibbleArray(XDIM, YDIM, ZDIM, blockLight);
        AddBlocks = new YZXNibbleArray(XDIM, YDIM, ZDIM, addBlocks);

        TagNodeCompound tree = new();
        tree.Add("Y", new TagNodeByte(_y));
        tree.Add("Blocks", blocks);
        tree.Add("Data", data);
        tree.Add("SkyLight", skyLight);
        tree.Add("BlockLight", blockLight);
        tree.Add("Add", addBlocks);

        _tree = tree;
    }
}
