using Substrate.Core;
using Substrate.Nbt;

namespace Substrate.ImportExport;

/// <summary>
/// Provides import and export support for the 3rd party schematic file format.
/// </summary>
public class Schematic
{
    private static SchemaNodeCompound _schema = new()
    {
        new SchemaNodeScaler("Width", TagType.TAG_SHORT),
        new SchemaNodeScaler("Length", TagType.TAG_SHORT),
        new SchemaNodeScaler("Height", TagType.TAG_SHORT),
        new SchemaNodeString("Materials", "Alpha"),
        new SchemaNodeArray("Blocks"),
        new SchemaNodeArray("Data"),
        new SchemaNodeList("Entities", TagType.TAG_COMPOUND, Entity.Schema),
        new SchemaNodeList("TileEntities", TagType.TAG_COMPOUND, TileEntity.Schema),
    };

    private XZYByteArray _blocks;
    private XZYNibbleArray _data;
    private XZYNibbleArray _blockLight;
    private XZYNibbleArray _skyLight;
    private ZXByteArray _heightMap;

    private TagNodeList _entities;
    private TagNodeList _tileEntities;

    private Schematic()
    {
    }

    /// <summary>
    /// Create an exportable schematic wrapper around existing blocks and entities.
    /// </summary>
    /// <param name="blocks">An existing <see cref="AlphaBlockCollection"/>.</param>
    /// <param name="entities">An existing <see cref="EntityCollection"/>.</param>
    public Schematic(AlphaBlockCollection blocks, EntityCollection entities)
    {
        Blocks = blocks;
        Entities = entities;
    }

    /// <summary>
    /// Create an empty, exportable schematic of given dimensions.
    /// </summary>
    /// <param name="xdim">The length of the X-dimension in blocks.</param>
    /// <param name="ydim">The length of the Y-dimension in blocks.</param>
    /// <param name="zdim">The length of the Z-dimension in blocks.</param>
    public Schematic(int xdim, int ydim, int zdim)
    {
        _blocks = new XZYByteArray(xdim, ydim, zdim);
        _data = new XZYNibbleArray(xdim, ydim, zdim);
        _blockLight = new XZYNibbleArray(xdim, ydim, zdim);
        _skyLight = new XZYNibbleArray(xdim, ydim, zdim);
        _heightMap = new ZXByteArray(xdim, zdim);

        _entities = new TagNodeList(TagType.TAG_COMPOUND);
        _tileEntities = new TagNodeList(TagType.TAG_COMPOUND);

        Blocks = new AlphaBlockCollection(_blocks, _data, _blockLight, _skyLight, _heightMap, _tileEntities);
        Entities = new EntityCollection(_entities);
    }

    #region Properties

    /// <summary>
    /// Gets or sets the underlying block collection.
    /// </summary>
    public AlphaBlockCollection Blocks { get; set; }

    /// <summary>
    /// Gets or sets the underlying entity collection.
    /// </summary>
    public EntityCollection Entities { get; set; }

    #endregion

    /// <summary>
    /// Imports a schematic file at the given path and returns in as a <see cref="Schematic"/> object.
    /// </summary>
    /// <param name="path">The path to the schematic file.</param>
    /// <returns>A <see cref="Schematic"/> object containing the decoded schematic file data.</returns>
    public static Schematic Import(string path)
    {
        NBTFile schematicFile = new(path);
        if (!schematicFile.Exists())
        {
            return null;
        }
        NbtTree tree;

        using (Stream nbtStream = schematicFile.GetDataInputStream())
        {
            if (nbtStream == null)
            {
                return null;
            }

            tree = new NbtTree(nbtStream);
        }

        NbtVerifier v = new(tree.Root, _schema);
        if (!v.Verify())
        {
            return null;
        }

        //TagNodeCompound schematic = tree.Root["Schematic"] as TagNodeCompound;
        TagNodeCompound schematic = tree.Root;
        int xdim = schematic["Width"].ToTagShort();
        int zdim = schematic["Length"].ToTagShort();
        int ydim = schematic["Height"].ToTagShort();

        Schematic self = new(xdim, ydim, zdim);

        // Damnit, schematic is YZX ordering.
        YZXByteArray schemaBlocks = new(xdim, ydim, zdim, schematic["Blocks"].ToTagByteArray());
        YZXByteArray schemaData = new(xdim, ydim, zdim, schematic["Data"].ToTagByteArray());

        for (int x = 0; x < xdim; x++)
        {
            for (int y = 0; y < ydim; y++)
            {
                for (int z = 0; z < zdim; z++)
                {
                    self._blocks[x, y, z] = schemaBlocks[x, y, z];
                    self._data[x, y, z] = schemaData[x, y, z];
                }
            }
        }

        TagNodeList entities = schematic["Entities"] as TagNodeList;
        foreach (TagNode e in entities)
        {
            self._entities.Add(e);
        }

        TagNodeList tileEntities = schematic["TileEntities"] as TagNodeList;
        foreach (TagNode te in tileEntities)
        {
            self._tileEntities.Add(te);
        }

        self.Blocks.Refresh();

        return self;
    }

    /// <summary>
    /// Exports the <see cref="Schematic"/> object to a schematic file.
    /// </summary>
    /// <param name="path">The path to write out the schematic file to.</param>
    public void Export(string path)
    {
        int xdim = Blocks.XDim;
        int ydim = Blocks.YDim;
        int zdim = Blocks.ZDim;

        byte[] blockData = new byte[xdim * ydim * zdim];
        byte[] dataData = new byte[xdim * ydim * zdim];

        YZXByteArray schemaBlocks = new(Blocks.XDim, Blocks.YDim, Blocks.ZDim, blockData);
        YZXByteArray schemaData = new(Blocks.XDim, Blocks.YDim, Blocks.ZDim, dataData);

        TagNodeList entities = new(TagType.TAG_COMPOUND);
        TagNodeList tileEntities = new(TagType.TAG_COMPOUND);

        for (int x = 0; x < xdim; x++)
        {
            for (int z = 0; z < zdim; z++)
            {
                for (int y = 0; y < ydim; y++)
                {
                    AlphaBlock block = Blocks.GetBlock(x, y, z);
                    schemaBlocks[x, y, z] = (byte)block.ID;
                    schemaData[x, y, z] = (byte)block.Data;

                    TileEntity te = block.GetTileEntity();
                    if (te != null)
                    {
                        te.X = x;
                        te.Y = y;
                        te.Z = z;

                        tileEntities.Add(te.BuildTree());
                    }
                }
            }
        }

        foreach (TypedEntity e in Entities)
        {
            entities.Add(e.BuildTree());
        }

        TagNodeCompound schematic = new();
        schematic["Width"] = new TagNodeShort((short)xdim);
        schematic["Length"] = new TagNodeShort((short)zdim);
        schematic["Height"] = new TagNodeShort((short)ydim);

        schematic["Entities"] = entities;
        schematic["TileEntities"] = tileEntities;

        schematic["Materials"] = new TagNodeString("Alpha");

        schematic["Blocks"] = new TagNodeByteArray(blockData);
        schematic["Data"] = new TagNodeByteArray(dataData);

        NBTFile schematicFile = new(path);

        using Stream nbtStream = schematicFile.GetDataOutputStream();
        if (nbtStream == null)
        {
            return;
        }

        NbtTree tree = new(schematic, "Schematic");
        tree.WriteTo(nbtStream);
    }
}
