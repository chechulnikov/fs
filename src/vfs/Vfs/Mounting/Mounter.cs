using System;
using System.IO;

namespace Vfs.Mounting
{
    internal class Mounter
    {
        public IFileSystem Mount(string volumePath)
        {
            if (!File.Exists(volumePath)) throw new VolumeNotFoundException(volumePath);
            
            throw new NotImplementedException();
        }
        
        public void Unmount(string volumePath)
        {
            if (!File.Exists(volumePath)) throw new VolumeNotFoundException(volumePath);
            
            throw new NotImplementedException();
        }
    }
}