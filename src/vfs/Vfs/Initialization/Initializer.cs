using System;
using System.IO;

namespace Vfs.Initialization
{
    internal class Initializer
    {
        public void Initialize(string deviceFilePath)
        {
            if (File.Exists(deviceFilePath)) throw new DeviceFileNotFoundException(deviceFilePath);
            
            throw new NotImplementedException();
        }
    }
}