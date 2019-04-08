using System.Collections.Generic;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal.DataAccess.Blocks;

namespace Jbta.VirtualFileSystem.Internal.Indexing.DataStructure
{
    internal interface IBPlusTreeNodesPersistenceManager
    {
        IBPlusTreeNode CreateFrom(IndexBlock indexBlock, int blockNumber);

        Task<IBPlusTreeNode> CreateNewNode();
        
        Task<IBPlusTreeNode> LoadNode(int blockNumber);
        
        Task UpdateNodes(IEnumerable<IBPlusTreeNode> nodes);

        Task DeleteNodes(IEnumerable<IBPlusTreeNode> nodes);
    }
}