namespace Substrate.Core;


public class NibbleArray : IDataArray, ICopyable<NibbleArray>
{
    public NibbleArray(int length)
    {
        Data = new byte[(int)Math.Ceiling(length / 2.0)];
    }

    public NibbleArray(byte[] data)
    {
        Data = data;
    }

    public int this[int index]
    {
        get
        {
            int subs = index >> 1;
            return (index & 1) == 0 ? (byte)(Data[subs] & 0x0F) : (byte)((Data[subs] >> 4) & 0x0F);
        }

        set
        {
            int subs = index >> 1;
            Data[subs] = (index & 1) == 0 ? (byte)((Data[subs] & 0xF0) | (value & 0x0F)) : (byte)((Data[subs] & 0x0F) | ((value & 0x0F) << 4));
        }
    }

    public int Length => Data.Length << 1;

    public int DataWidth => 4;

    protected byte[] Data { get; } = null;

    public void Clear()
    {
        for (int i = 0; i < Data.Length; i++)
        {
            Data[i] = 0;
        }
    }

    #region ICopyable<NibbleArray> Members

    public virtual NibbleArray Copy()
    {
        byte[] data = new byte[Data.Length];
        Data.CopyTo(data, 0);

        return new NibbleArray(data);
    }

    #endregion
}

public sealed class XZYNibbleArray : NibbleArray, IDataArray3
{
    public XZYNibbleArray(int xdim, int ydim, int zdim)
        : base(xdim * ydim * zdim)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;
    }

    public XZYNibbleArray(int xdim, int ydim, int zdim, byte[] data)
        : base(data)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;

        if (xdim * ydim * zdim != data.Length * 2)
        {
            throw new ArgumentException("Product of dimensions must equal half length of raw data");
        }
    }

    public int this[int x, int y, int z]
    {
        get
        {
            int index = YDim * (x * ZDim + z) + y;
            return this[index];
        }

        set
        {
            int index = YDim * (x * ZDim + z) + y;
            this[index] = value;
        }
    }

    public int XDim { get; }

    public int YDim { get; }

    public int ZDim { get; }

    public int GetIndex(int x, int y, int z)
    {
        return YDim * (x * ZDim + z) + y;
    }

    public void GetMultiIndex(int index, out int x, out int y, out int z)
    {
        int yzdim = YDim * ZDim;
        x = index / yzdim;

        int zy = index - (x * yzdim);
        z = zy / YDim;
        y = zy - (z * YDim);
    }

    #region ICopyable<NibbleArray> Members

    public override NibbleArray Copy()
    {
        byte[] data = new byte[Data.Length];
        Data.CopyTo(data, 0);

        return new XZYNibbleArray(XDim, YDim, ZDim, data);
    }

    #endregion
}

public sealed class YZXNibbleArray : NibbleArray, IDataArray3
{
    public YZXNibbleArray(int xdim, int ydim, int zdim)
        : base(xdim * ydim * zdim)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;
    }

    public YZXNibbleArray(int xdim, int ydim, int zdim, byte[] data)
        : base(data)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;

        if (xdim * ydim * zdim != data.Length * 2)
        {
            throw new ArgumentException("Product of dimensions must equal half length of raw data");
        }
    }

    public int this[int x, int y, int z]
    {
        get
        {
            int index = XDim * (y * ZDim + z) + x;
            return this[index];
        }

        set
        {
            int index = XDim * (y * ZDim + z) + x;
            this[index] = value;
        }
    }

    public int XDim { get; }

    public int YDim { get; }

    public int ZDim { get; }

    public int GetIndex(int x, int y, int z)
    {
        return XDim * (y * ZDim + z) + x;
    }

    public void GetMultiIndex(int index, out int x, out int y, out int z)
    {
        int xzdim = XDim * ZDim;
        y = index / xzdim;

        int zx = index - (y * xzdim);
        z = zx / XDim;
        x = zx - (z * XDim);
    }

    #region ICopyable<NibbleArray> Members

    public override NibbleArray Copy()
    {
        byte[] data = new byte[Data.Length];
        Data.CopyTo(data, 0);

        return new YZXNibbleArray(XDim, YDim, ZDim, data);
    }

    #endregion
}
