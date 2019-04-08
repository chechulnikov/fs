namespace Jbta.VirtualFileSystem.Internal.Utils
{
    internal interface IBinarySerializer<T>
    {
        byte[] Serialize(T obj);
        
        T Deserialize(byte[] data);
    }
}