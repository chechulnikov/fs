namespace Jbta.VirtualFileSystem.Utils
{
    internal interface IBinarySerializer<T>
    {
        byte[] Serialize(T obj);
        
        T Deserialize(byte[] data);
    }
}