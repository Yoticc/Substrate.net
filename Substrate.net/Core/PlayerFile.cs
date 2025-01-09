namespace Substrate.Core;

public class PlayerFile : NBTFile
{
    public PlayerFile(string path)
        : base(path)
    {
    }

    public PlayerFile(string path, string name)
        : base("")
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string file = name + ".dat";
        FileName = Path.Combine(path, file);
    }

    public static string NameFromFilename(string filename)
    {
        return filename.EndsWith(".dat") ? filename.Remove(filename.Length - 4) : filename;
    }
}
