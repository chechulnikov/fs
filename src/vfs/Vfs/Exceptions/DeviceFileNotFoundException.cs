using System;

namespace Vfs
{
    public class DeviceFileNotFoundException : FileSystemException
    {
        private readonly string _deviceFilePath;

        public DeviceFileNotFoundException(string deviceFilePath)
        {
            _deviceFilePath = deviceFilePath ?? throw new ArgumentNullException(nameof(deviceFilePath));
        }

        public override string Message => $"Device file not found: {_deviceFilePath}";
    }
}