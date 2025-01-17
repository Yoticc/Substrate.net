﻿using Substrate.Core;

namespace Substrate;

public class AlphaBlockManager : BlockManager
{
    public AlphaBlockManager(IChunkManager cm)
        : base(cm)
    {
        IChunk c = AlphaChunk.Create(0, 0);

        chunkXDim = c.Blocks.XDim;
        chunkYDim = c.Blocks.YDim;
        chunkZDim = c.Blocks.ZDim;
        chunkXMask = chunkXDim - 1;
        chunkYMask = chunkYDim - 1;
        chunkZMask = chunkZDim - 1;
        chunkXLog = Log2(chunkXDim);
        chunkYLog = Log2(chunkYDim);
        chunkZLog = Log2(chunkZDim);
    }
}

public class AnvilBlockManager : BlockManager
{
    public AnvilBlockManager(IChunkManager cm)
        : base(cm)
    {
        IChunk c = AnvilChunk.Create(0, 0);

        chunkXDim = c.Blocks.XDim;
        chunkYDim = c.Blocks.YDim;
        chunkZDim = c.Blocks.ZDim;
        chunkXMask = chunkXDim - 1;
        chunkYMask = chunkYDim - 1;
        chunkZMask = chunkZDim - 1;
        chunkXLog = Log2(chunkXDim);
        chunkYLog = Log2(chunkYDim);
        chunkZLog = Log2(chunkZDim);
    }
}

/// <summary>
/// Represents an Alpha-compatible interface for globally managing blocks.
/// </summary>
public abstract class BlockManager : IVersion10BlockManager, IBlockManager
{
    public const int MIN_X = -32000000;
    public const int MAX_X = 32000000;
    public const int MIN_Y = 0;
    public const int MAX_Y = 256;
    public const int MIN_Z = -32000000;
    public const int MAX_Z = 32000000;

    protected int chunkXDim;
    protected int chunkYDim;
    protected int chunkZDim;
    protected int chunkXMask;
    protected int chunkYMask;
    protected int chunkZMask;
    protected int chunkXLog;
    protected int chunkYLog;
    protected int chunkZLog;

    protected IChunkManager chunkMan;

    protected ChunkRef cache;

    /// <summary>
    /// Gets or sets a value indicating whether changes to blocks will trigger automatic lighting updates.
    /// </summary>
    public bool AutoLight { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether changes to blocks will trigger automatic fluid updates.
    /// </summary>
    public bool AutoFluid { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether changes to blocks will trigger automatic fluid updates.
    /// </summary>
    public bool AutoTileTick { get; set; } = false;

    /// <summary>
    /// Constructs a new <see cref="BlockManager"/> instance on top of the given <see cref="IChunkManager"/>.
    /// </summary>
    /// <param name="cm">An <see cref="IChunkManager"/> instance.</param>
    public BlockManager(IChunkManager cm)
    {
        chunkMan = cm;
    }

    /// <summary>
    /// Returns a new <see cref="AlphaBlock"/> object from global coordinates.
    /// </summary>
    /// <param name="x">Global X-coordinate of block.</param>
    /// <param name="y">Global Y-coordinate of block.</param>
    /// <param name="z">Global Z-coordiante of block.</param>
    /// <returns>A new <see cref="AlphaBlock"/> object representing context-independent data of a single block.</returns>
    /// <remarks>Context-independent data excludes data such as lighting.  <see cref="AlphaBlock"/> object actually contain a copy
    /// of the data they represent, so changes to the <see cref="AlphaBlock"/> will not affect this container, and vice-versa.</remarks>
    public AlphaBlock GetBlock(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null || !Check(x, y, z) ? null : cache.Blocks.GetBlock(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <summary>
    /// Returns a new <see cref="AlphaBlockRef"/> object from global coordaintes.
    /// </summary>
    /// <param name="x">Global X-coordinate of block.</param>
    /// <param name="y">Global Y-coordinate of block.</param>
    /// <param name="z">Global Z-coordinate of block.</param>
    /// <returns>A new <see cref="AlphaBlockRef"/> object representing context-dependent data of a single block.</returns>
    /// <remarks>Context-depdendent data includes all data associated with this block.  Since a <see cref="AlphaBlockRef"/> represents
    /// a view of a block within this container, any updates to data in the container will be reflected in the <see cref="AlphaBlockRef"/>,
    /// and vice-versa for updates to the <see cref="AlphaBlockRef"/>.</remarks>
    public AlphaBlockRef GetBlockRef(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null || !Check(x, y, z)
            ? new AlphaBlockRef()
            : cache.Blocks.GetBlockRef(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <summary>
    /// Updates a block with values from a <see cref="AlphaBlock"/> object.
    /// </summary>
    /// <param name="x">Global X-coordinate of a block.</param>
    /// <param name="y">Global Y-coordinate of a block.</param>
    /// <param name="z">Global Z-coordinate of a block.</param>
    /// <param name="block">A <see cref="AlphaBlock"/> object to copy block data from.</param>
    public void SetBlock(int x, int y, int z, AlphaBlock block)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetBlock(x & chunkXMask, y & chunkYMask, z & chunkZMask, block);
    }

    /// <summary>
    /// Gets a reference object to a single chunk given global coordinates to a block within that chunk.
    /// </summary>
    /// <param name="x">Global X-coordinate of a block.</param>
    /// <param name="y">Global Y-coordinate of a block.</param>
    /// <param name="z">Global Z-coordinate of a block.</param>
    /// <returns>A <see cref="ChunkRef"/> to a single chunk containing the given block.</returns>
    public ChunkRef GetChunk(int x, int y, int z)
    {
        x >>= chunkXLog;
        z >>= chunkZLog;
        return chunkMan.GetChunkRef(x, z);
    }

    protected int Log2(int x)
    {
        int c = 0;
        while (x > 1)
        {
            x >>= 1;
            c++;
        }
        return c;
    }

    /// <summary>
    /// Called by other block-specific 'get' and 'set' functions to filter
    /// out operations on some blocks.  Override this method in derrived
    /// classes to filter the entire BlockManager.
    /// </summary>
    protected virtual bool Check(int x, int y, int z)
    {
        return (x >= MIN_X) && (x < MAX_X) &&
            (y >= MIN_Y) && (y < MAX_Y) &&
            (z >= MIN_Z) && (z < MAX_Z);
    }

    #region IBlockContainer Members

    IBlock IBlockCollection.GetBlock(int x, int y, int z)
    {
        return GetBlock(x, y, z);
    }

    IBlock IBlockCollection.GetBlockRef(int x, int y, int z)
    {
        return GetBlockRef(x, y, z);
    }

    /// <inheritdoc/>
    public void SetBlock(int x, int y, int z, IBlock block)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetBlock(x, y, z, block);
    }

    /// <inheritdoc/>
    public BlockInfo GetInfo(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null || !Check(x, y, z) ? null : cache.Blocks.GetInfo(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public int GetID(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null ? 0 : cache.Blocks.GetID(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void SetID(int x, int y, int z, int id)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        bool autolight = cache.Blocks.AutoLight;
        bool autofluid = cache.Blocks.AutoFluid;
        bool autoTileTick = cache.Blocks.AutoTileTick;

        cache.Blocks.AutoLight = AutoLight;
        cache.Blocks.AutoFluid = AutoFluid;
        cache.Blocks.AutoTileTick = AutoTileTick;

        cache.Blocks.SetID(x & chunkXMask, y & chunkYMask, z & chunkZMask, id);

        cache.Blocks.AutoFluid = autofluid;
        cache.Blocks.AutoLight = autolight;
        cache.Blocks.AutoTileTick = autoTileTick;
    }

    #endregion


    #region IDataBlockCollection Members

    IDataBlock IDataBlockCollection.GetBlock(int x, int y, int z)
    {
        return GetBlock(x, y, z);
    }

    IDataBlock IDataBlockCollection.GetBlockRef(int x, int y, int z)
    {
        return GetBlockRef(x, y, z);
    }

    /// <inheritdoc/>
    public void SetBlock(int x, int y, int z, IDataBlock block)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetBlock(x, y, z, block);
    }

    /// <inheritdoc/>
    public int GetData(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null ? 0 : cache.Blocks.GetData(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void SetData(int x, int y, int z, int data)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetData(x & chunkXMask, y & chunkYMask, z & chunkZMask, data);
    }

    #endregion


    #region ILitBlockContainer Members

    ILitBlock ILitBlockCollection.GetBlock(int x, int y, int z)
    {
        throw new NotImplementedException();
    }

    ILitBlock ILitBlockCollection.GetBlockRef(int x, int y, int z)
    {
        return GetBlockRef(x, y, z);
    }

    /// <inheritdoc/>
    public void SetBlock(int x, int y, int z, ILitBlock block)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetBlock(x, y, z, block);
    }

    /// <inheritdoc/>
    public int GetBlockLight(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null ? 0 : cache.Blocks.GetBlockLight(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public int GetSkyLight(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null ? 0 : cache.Blocks.GetSkyLight(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void SetBlockLight(int x, int y, int z, int light)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetBlockLight(x & chunkXMask, y & chunkYMask, z & chunkZMask, light);
    }

    /// <inheritdoc/>
    public void SetSkyLight(int x, int y, int z, int light)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetSkyLight(x & chunkXMask, y & chunkYMask, z & chunkZMask, light);
    }

    /// <inheritdoc/>
    public int GetHeight(int x, int z)
    {
        cache = GetChunk(x, 0, z);
        return cache == null || !Check(x, 0, z) ? 0 : cache.Blocks.GetHeight(x & chunkXMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void SetHeight(int x, int z, int height)
    {
        cache = GetChunk(x, 0, z);
        if (cache == null || !Check(x, 0, z))
        {
            return;
        }

        cache.Blocks.SetHeight(x & chunkXMask, z & chunkZMask, height);
    }

    /// <inheritdoc/>
    public void UpdateBlockLight(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.UpdateBlockLight(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void UpdateSkyLight(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.UpdateBlockLight(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    #endregion


    #region IPropertyBlockContainer Members

    IPropertyBlock IPropertyBlockCollection.GetBlock(int x, int y, int z)
    {
        return GetBlock(x, y, z);
    }

    IPropertyBlock IPropertyBlockCollection.GetBlockRef(int x, int y, int z)
    {
        return GetBlockRef(x, y, z);
    }

    /// <inheritdoc/>
    public void SetBlock(int x, int y, int z, IPropertyBlock block)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetBlock(x, y, z, block);
    }

    /// <inheritdoc/>
    public TileEntity GetTileEntity(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null || !Check(x, y, z) ? null : cache.Blocks.GetTileEntity(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void SetTileEntity(int x, int y, int z, TileEntity te)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetTileEntity(x & chunkXMask, y & chunkYMask, z & chunkZMask, te);
    }

    /// <inheritdoc/>
    public void CreateTileEntity(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.CreateTileEntity(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void ClearTileEntity(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.ClearTileEntity(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    #endregion


    #region IActiveBlockContainer Members

    IActiveBlock IActiveBlockCollection.GetBlock(int x, int y, int z)
    {
        return GetBlock(x, y, z);
    }

    IActiveBlock IActiveBlockCollection.GetBlockRef(int x, int y, int z)
    {
        return GetBlockRef(x, y, z);
    }

    /// <inheritdoc/>
    public void SetBlock(int x, int y, int z, IActiveBlock block)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetBlock(x, y, z, block);
    }

    /// <inheritdoc/>
    public int GetTileTickValue(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null || !Check(x, y, z) ? 0 : cache.Blocks.GetTileTickValue(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void SetTileTickValue(int x, int y, int z, int tickValue)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetTileTickValue(x & chunkXMask, y & chunkYMask, z & chunkZMask, tickValue);
    }

    /// <inheritdoc/>
    public TileTick GetTileTick(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        return cache == null || !Check(x, y, z) ? null : cache.Blocks.GetTileTick(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void SetTileTick(int x, int y, int z, TileTick te)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.SetTileTick(x & chunkXMask, y & chunkYMask, z & chunkZMask, te);
    }

    /// <inheritdoc/>
    public void CreateTileTick(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.CreateTileTick(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    /// <inheritdoc/>
    public void ClearTileTick(int x, int y, int z)
    {
        cache = GetChunk(x, y, z);
        if (cache == null || !Check(x, y, z))
        {
            return;
        }

        cache.Blocks.ClearTileTick(x & chunkXMask, y & chunkYMask, z & chunkZMask);
    }

    #endregion
}
