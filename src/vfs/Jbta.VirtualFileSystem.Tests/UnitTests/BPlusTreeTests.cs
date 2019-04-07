using System;
using System.Linq;
using System.Threading.Tasks;
using Jbta.VirtualFileSystem.Internal;
using Jbta.VirtualFileSystem.Internal.Indexing.DataStructure;
using Moq;
using Xunit;

namespace Jbta.VirtualFileSystem.Tests.UnitTests
{
    public class BPlusTreeTests
    {
        private Mock<IBPlusTreeNodesPersistenceManager> _nodePersistenceManagerMock;
        private readonly BPlusTreeNode _rootNode;
        private readonly BPlusTree _tree;

        public BPlusTreeTests()
        {
            _nodePersistenceManagerMock = new Mock<IBPlusTreeNodesPersistenceManager>();
            _rootNode = new BPlusTreeNode();
            _tree = new BPlusTree(_nodePersistenceManagerMock.Object, _rootNode);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public Task Insert_InvalidKey_ArgumentException(string key)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => _tree.Insert(key, default(int)));
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public Task Insert_InvalidValue_ArgumentException(int value)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _tree.Insert("foo", value));
        }

        [Fact]
        public async Task Insert_EmptyTree_True()
        {
            // act
            var result = await _tree.Insert("foo", 42);
            
            // assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task Insert__OnlyOneNodeInTree_RootNodeCapacityExceeded__FirstRootSplit()
        {
            // arrange
            foreach (var i in Enumerable.Range(1, 2 * GlobalConstant.MinBPlusTreeDegree - 1))
            {
                var number = i < 10 ? $"0{i}" : i.ToString();
                await _tree.Insert($"foo{number}", i);
            }
            
            // act
            var result = await _tree.Insert("foobar", 42); 

            // assert
            Assert.True(result);
            
            var root = _tree.Root;
            Assert.False(root.IsLeaf);
            Assert.Equal(1, root.KeysNumber);
            Assert.Equal("foo11", root.Keys[0]);
            Assert.Equal(2, root.Children.Count(c => c != null));

            var first = root.Children[0];
            var second = root.Children[1];
            
            Assert.NotNull(first);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, first.KeysNumber);
            Assert.Contains(
                first.Keys,
                k => new []{"foo01", "foo02", "foo03", "foo04", "foo05", "foo06", "foo07", "foo08", "foo09", "foo10"}.Contains(k)
            );
            Assert.Contains(first.Values, v => new []{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Contains(v));
            Assert.All(first.Children, Assert.Null);
            Assert.Same(root, first.Parent);
            Assert.Null(first.LeftSibling);
            Assert.Same(second, first.RightSibling);
            
            Assert.NotNull(second);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, second.KeysNumber);
            Assert.Contains(
                second.Keys,
                k => new []{"foo11", "foo12", "foo13", "foo14", "foo15", "foo16", "foo17", "foo18", "foo19", "foobar"}.Contains(k)
            );
            Assert.Contains(second.Values, v => new []{11,12, 13, 14, 15, 16, 17, 18, 19, 42}.Contains(v));
            Assert.All(second.Children, Assert.Null);
            Assert.Same(root, second.Parent);
            Assert.Same(first, second.LeftSibling);
            Assert.Null(second.RightSibling);
        }
        
        [Fact]
        public async Task Insert__TwoTiesInTree_NodeCapacityExceeded__NodeSplit()
        {
            // arrange
            foreach (var i in Enumerable.Range(1, 3 * GlobalConstant.MinBPlusTreeDegree - 1))
            {
                var number = i < 10 ? $"0{i}" : i.ToString();
                await _tree.Insert($"foo{number}", i);
            }
            
            // act
            var result = await _tree.Insert("foobar", 42); 

            // assert
            Assert.True(result);
            
            var root = _tree.Root;
            Assert.False(root.IsLeaf);
            Assert.Equal(2, root.KeysNumber);
            Assert.Equal("foo11", root.Keys[0]);
            Assert.Equal("foo21", root.Keys[1]);
            Assert.Equal(3, root.Children.Count(c => c != null));

            var first = root.Children[0];
            var second = root.Children[1];
            var third = root.Children[2];

            Assert.NotNull(first);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, first.KeysNumber);
            Assert.Contains(
                first.Keys,
                k => new []{"foo01", "foo02", "foo03", "foo04", "foo05", "foo06", "foo07", "foo08", "foo09", "foo10"}.Contains(k)
            );
            Assert.Contains(first.Values, v => new []{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.Contains(v));
            Assert.All(first.Children, Assert.Null);
            Assert.Same(root, first.Parent);
            Assert.Null(first.LeftSibling);
            Assert.Same(second, first.RightSibling);
            
            Assert.NotNull(second);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, second.KeysNumber);
            Assert.Contains(
                second.Keys,
                k => new []{"foo11", "foo12", "foo13", "foo14", "foo15", "foo16", "foo17", "foo18", "foo19", "foo20"}.Contains(k)
            );
            Assert.Contains(second.Values, v => new []{11, 12, 13, 14, 15, 16, 17, 18, 19, 20}.Contains(v));
            Assert.All(second.Children, Assert.Null);
            Assert.Same(root, second.Parent);
            Assert.Same(first, second.LeftSibling);
            Assert.Same(third, second.RightSibling);

            Assert.NotNull(third);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, third.KeysNumber);
            Assert.Contains(
                third.Keys,
                k => new []{"foo21", "foo22", "foo23", "foo24", "foo25", "foo26", "foo27", "foo28", "foo29", "foobar"}.Contains(k)
            );
            Assert.Contains(third.Values, v => new []{21, 22, 23, 24, 25, 26, 27, 28, 29, 42}.Contains(v));
            Assert.All(third.Children, Assert.Null);
            Assert.Same(root, third.Parent);
            Assert.Same(second, third.LeftSibling);
            Assert.Null(third.RightSibling);
        }

        [Fact]
        public async Task Insert__TwoTiersInTrie_NodeCapacityExceeded__ParentSplit()
        {
            // arrange
            foreach (var i in Enumerable.Range(1, 20 * GlobalConstant.MinBPlusTreeDegree + 18))
            {
                await _tree.Insert($"foo{i}", i);
            }

            // act
            var result = await _tree.Insert("foobar", 4242);

            // assert
            Assert.True(result);

            var root = _tree.Root;
            Assert.False(root.IsLeaf);
            Assert.Equal(1, root.KeysNumber);
            Assert.Equal("foo199", root.Keys[0]);
            Assert.Equal(2, root.Children.Count(c => c != null));

            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, root.Children[0].KeysNumber);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree - 1, root.Children[1].KeysNumber);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void Search_InvalidKey_ArgumentException(string key)
        {
            // act, assert
            Assert.Throws<ArgumentException>(() => _tree.Search(key));
        }

        [Fact]
        public async Task Search__HappyPath_ExistedKey__Found()
        {
            // arrange
            await SetupTree();
            
            // act
            var (value, found) = _tree.Search("foo42");
            
            // assert
            Assert.True(found);
            Assert.Equal(42, value);
        }
        
        [Fact]
        public async Task Search__HappyPath_NonExistedKey__NotFound()
        {
            // arrange
            await SetupTree();
            
            // act
            var (result, found) = _tree.Search("foo4242");
            
            // assert
            Assert.False(found);
            Assert.Equal(0, result);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public Task Delete_InvalidKey_ArgumentException(string key)
        {
            // act, assert
            return Assert.ThrowsAsync<ArgumentException>(() => _tree.Delete(key));
        }
        
        // todo tests on delete !!!

        private async Task SetupTree()
        {
            foreach (var i in Enumerable.Range(1, 30 * GlobalConstant.MinBPlusTreeDegree))
            {
                await _tree.Insert($"foo{i}", i);
            }
        }
    }
}