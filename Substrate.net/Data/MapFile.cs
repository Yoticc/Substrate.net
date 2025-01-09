﻿using Substrate.Core;

namespace Substrate.Data;

public class MapFile : NBTFile
{
    public MapFile(string path)
        : base(path)
    {
    }

    public MapFile(string path, int id)
        : base("")
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string file = "map_" + id + ".dat";
        FileName = Path.Combine(path, file);
    }

    public static int IdFromFilename(string filename)
    {
        return filename.EndsWith(".dat")
            ? Convert.ToInt32(filename[4..].Remove(filename.Length - 4))
            : throw new FormatException("Filename '" + filename + "' is not a .dat file");
    }
}
