namespace Substrate.Core;

public interface IDataArray
{
    int this[int i] { get; set; }
    int Length { get; }
    int DataWidth { get; }

    void Clear();
}

public interface IDataArray2 : IDataArray
{
    int this[int x, int z] { get; set; }

    int XDim { get; }
    int ZDim { get; }
}

public interface IDataArray3 : IDataArray
{
    int this[int x, int y, int z] { get; set; }

    int XDim { get; }
    int YDim { get; }
    int ZDim { get; }

    int GetIndex(int x, int y, int z);
    void GetMultiIndex(int index, out int x, out int y, out int z);
}

public class ByteArray : IDataArray, ICopyable<ByteArray>
{
    protected readonly byte[] dataArray;

    public ByteArray(int length)
    {
        dataArray = new byte[length];
    }

    public ByteArray(byte[] data)
    {
        dataArray = data;
    }

    public int this[int i]
    {
        get => dataArray[i];
        set => dataArray[i] = (byte)value;
    }

    public int Length => dataArray.Length;

    public int DataWidth => 8;

    public void Clear()
    {
        for (int i = 0; i < dataArray.Length; i++)
        {
            dataArray[i] = 0;
        }
    }

    #region ICopyable<ByteArray> Members

    public virtual ByteArray Copy()
    {
        byte[] data = new byte[dataArray.Length];
        dataArray.CopyTo(data, 0);

        return new ByteArray(data);
    }

    #endregion
}

public sealed class XZYByteArray : ByteArray, IDataArray3
{
    public XZYByteArray(int xdim, int ydim, int zdim)
        : base(xdim * ydim * zdim)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;
    }

    public XZYByteArray(int xdim, int ydim, int zdim, byte[] data)
        : base(data)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;

        if (xdim * ydim * zdim != data.Length)
        {
            throw new ArgumentException("Product of dimensions must equal length of data");
        }
    }

    public int this[int x, int y, int z]
    {
        get
        {
            int index = YDim * (x * ZDim + z) + y;
            return dataArray[index];
        }

        set
        {
            int index = YDim * (x * ZDim + z) + y;
            dataArray[index] = (byte)value;
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

    #region ICopyable<XZYByteArray> Members

    public override ByteArray Copy()
    {
        byte[] data = new byte[dataArray.Length];
        dataArray.CopyTo(data, 0);

        return new XZYByteArray(XDim, YDim, ZDim, data);
    }

    #endregion
}

public sealed class YZXByteArray : ByteArray, IDataArray3
{
    public YZXByteArray(int xdim, int ydim, int zdim)
        : base(xdim * ydim * zdim)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;
    }

    public YZXByteArray(int xdim, int ydim, int zdim, byte[] data)
        : base(data)
    {
        XDim = xdim;
        YDim = ydim;
        ZDim = zdim;

        if (xdim * ydim * zdim != data.Length)
        {
            throw new ArgumentException("Product of dimensions must equal length of data");
        }
    }

    public int this[int x, int y, int z]
    {
        get
        {
            int index = XDim * (y * ZDim + z) + x;
            return dataArray[index];
        }

        set
        {
            int index = XDim * (y * ZDim + z) + x;
            dataArray[index] = (byte)value;
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

    #region ICopyable<YZXByteArray> Members

    public override ByteArray Copy()
    {
        byte[] data = new byte[dataArray.Length];
        dataArray.CopyTo(data, 0);

        return new YZXByteArray(XDim, YDim, ZDim, data);
    }

    #endregion
}

public sealed class ZXByteArray : ByteArray, IDataArray2
{
    public ZXByteArray(int xdim, int zdim)
        : base(xdim * zdim)
    {
        XDim = xdim;
        ZDim = zdim;
    }

    public ZXByteArray(int xdim, int zdim, byte[] data)
        : base(data)
    {
        XDim = xdim;
        ZDim = zdim;

        if (xdim * zdim != data.Length)
        {
            throw new ArgumentException("Product of dimensions must equal length of data");
        }
    }

    public int this[int x, int z]
    {
        get
        {
            int index = z * XDim + x;
            return dataArray[index];
        }

        set
        {
            int index = z * XDim + x;
            dataArray[index] = (byte)value;
        }
    }

    public int XDim { get; }

    public int ZDim { get; }

    #region ICopyable<ZXByteArray> Members

    public override ByteArray Copy()
    {
        byte[] data = new byte[dataArray.Length];
        dataArray.CopyTo(data, 0);

        return new ZXByteArray(XDim, ZDim, data);
    }

    #endregion
}

public class IntArray : IDataArray, ICopyable<IntArray>
{
    protected readonly int[] dataArray;

    public IntArray(int length)
    {
        dataArray = new int[length];
    }

    public IntArray(int[] data)
    {
        dataArray = data;
    }

    public int this[int i]
    {
        get => dataArray[i];
        set => dataArray[i] = value;
    }

    public int Length => dataArray.Length;

    public int DataWidth => 32;

    public void Clear()
    {
        for (int i = 0; i < dataArray.Length; i++)
        {
            dataArray[i] = 0;
        }
    }

    #region ICopyable<ByteArray> Members

    public virtual IntArray Copy()
    {
        int[] data = new int[dataArray.Length];
        dataArray.CopyTo(data, 0);

        return new IntArray(data);
    }

    #endregion
}

public sealed class ZXIntArray : IntArray, IDataArray2
{
    public ZXIntArray(int xdim, int zdim)
        : base(xdim * zdim)
    {
        XDim = xdim;
        ZDim = zdim;
    }

    public ZXIntArray(int xdim, int zdim, int[] data)
        : base(data)
    {
        XDim = xdim;
        ZDim = zdim;

        if (xdim * zdim != data.Length)
        {
            throw new ArgumentException("Product of dimensions must equal length of data");
        }
    }

    public int this[int x, int z]
    {
        get
        {
            int index = z * XDim + x;
            return dataArray[index];
        }

        set
        {
            int index = z * XDim + x;
            dataArray[index] = value;
        }
    }

    public int XDim { get; }

    public int ZDim { get; }

    #region ICopyable<ZXByteArray> Members

    public override IntArray Copy()
    {
        int[] data = new int[dataArray.Length];
        dataArray.CopyTo(data, 0);

        return new ZXIntArray(XDim, ZDim, data);
    }

    #endregion
}
