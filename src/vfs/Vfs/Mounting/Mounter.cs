using System;
using System.IO;

namespace Vfs.Mounting
{
    internal class Mounter
    {
        public IFileSystem Mount(string deviceFilePath)
        {
            if (!File.Exists(deviceFilePath)) throw new DeviceFileNotFoundException(deviceFilePath);
            
            throw new NotImplementedException();
        }
        
        public void Unmount(string deviceFilePath)
        {
            if (!File.Exists(deviceFilePath)) throw new DeviceFileNotFoundException(deviceFilePath);
            
            throw new NotImplementedException();
        }
    }
}