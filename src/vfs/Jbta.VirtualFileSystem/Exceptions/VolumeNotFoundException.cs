using System;

namespace Vfs
{
    public class VolumeNotFoundException : FileSystemException
    {
        private readonly string _deviceFilePath;

        public VolumeNotFoundException(string devicePath)
        {
            _deviceFilePath = devicePath ?? throw new ArgumentNullException(nameof(devicePath));
        }

        public override string Message => $"Volume not found: {_deviceFilePath}";
    }
}