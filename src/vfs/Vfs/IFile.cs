namespace Vfs
{
    public interface IFile
    {
        string Path { get; set; }
        string Name { get; set; }
        ulong Size { get; }

        byte[] Read(ulong start, ulong length);
       
        void Write(byte[] data);
        void Write(ulong start, byte[] data);
    }
}