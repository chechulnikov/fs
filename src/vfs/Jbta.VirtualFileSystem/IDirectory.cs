
using System.Collections.Generic;

namespace Vfs
{
    public interface IDirectory
    {
        string Path { get; set; }
        string Name { get; set; }
        uint ElementsCount { get; set; }
        IDirectory Parent { get; set; }
        IEnumerable<IDirectory> Directories { get; }
        IEnumerable<IFile> Files { get; }
    }
}