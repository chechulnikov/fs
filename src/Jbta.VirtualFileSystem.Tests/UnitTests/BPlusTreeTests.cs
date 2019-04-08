using System;
using System.Collections.Generic;
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
        private readonly BPlusTree _tree;

        public BPlusTreeTests()
        {
            var nodePersistenceManagerMock = new Mock<IBPlusTreeNodesPersistenceManager>();
            nodePersistenceManagerMock
                .Setup(m => m.CreateNewNode())
                .ReturnsAsync(() => new BPlusTreeNode());
            _tree = new BPlusTree(nodePersistenceManagerMock.Object, new BPlusTreeNode());
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
            var root = _tree.Root;
            Assert.True(root.IsLeaf);
            Assert.Equal(1, root.KeysNumber);
            Assert.Equal(1, root.Keys.Count(k => k != null));
            Assert.Equal(1, root.Values.Count(v => v != 0));
            Assert.All(root.Children, Assert.Null);
            Assert.Null(root.Parent);
            Assert.Null(root.LeftSibling);
            Assert.Null(root.RightSibling);
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
            await SetupTree(30 * GlobalConstant.MinBPlusTreeDegree);
            
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
            await SetupTree(30 * GlobalConstant.MinBPlusTreeDegree);
            
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

        [Fact]
        public async Task Delete_NonExistingNode_False()
        {
            // act
            var result = await _tree.Delete("foobar");
            
            // assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task Delete_ExistingSingleNode_NodeDeleted()
        {
            // arrange
            const string key = "foobar";
            await _tree.Insert(key, 42);
            
            // act
            var result = await _tree.Delete(key);
            
            // assert
            Assert.True(result);
            var root = _tree.Root;
            Assert.Equal(0, root.KeysNumber);
            Assert.All(root.Keys, Assert.Null);
            Assert.All(root.Children, Assert.Null);
            Assert.All(root.Values, v => Assert.Equal(0, v));
            Assert.Null(root.Parent);
            Assert.Null(root.LeftSibling);
            Assert.Null(root.RightSibling);
        }
        
        [Fact]
        public async Task Delete__ManyNodes_DeleteOne__NotFound()
        {
            // arrange
            await SetupTree(20 * GlobalConstant.MinBPlusTreeDegree);
            const string key = "foo42";
            
            // act
            var result = await _tree.Delete(key);
            
            // assert
            Assert.True(result);
            var (value, found) = _tree.Search(key);
            Assert.False(found);
            Assert.Equal(0, value);
        }

        [Fact]
        public async Task Delete__DeleteOneKey_LeftSiblingHaveMoreKeys__MoveKeyFromLeftSibling()
        {
            // arrange
            await SetupTree(3 * GlobalConstant.MinBPlusTreeDegree); // 3 children
            await _tree.Insert("foo1111", 1111);
            var midKey = _tree.Root.Children[1].Keys[0];
            
            // act
            var result = await _tree.Delete(midKey);
            
            // assert
            Assert.True(result);
            var root = _tree.Root;
            Assert.False(root.IsLeaf);
            Assert.Equal(2, root.KeysNumber);
            Assert.Equal(3, root.Children.Count(c => c != null));
            Assert.Equal("foo18", root.Keys[0]);
            Assert.Equal("foo28", root.Keys[1]);
            
            var first = root.Children[0];
            var second = root.Children[1];
            var third = root.Children[2];

            Assert.NotNull(first);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, first.KeysNumber);
            Assert.DoesNotContain(first.Keys, k => k == "foo19");
            Assert.DoesNotContain(first.Values, v => v == 19);
            Assert.DoesNotContain(first.Keys, k => k == "foo18");
            Assert.DoesNotContain(first.Values, v => v == 18);
            Assert.All(first.Children, Assert.Null);
            Assert.Same(root, first.Parent);
            Assert.Null(first.LeftSibling);
            Assert.Same(second, first.RightSibling);
            
            Assert.NotNull(second);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, second.KeysNumber);
            Assert.DoesNotContain(second.Keys, k => k == "foo19");
            Assert.DoesNotContain(second.Values, v => v == 19);
            Assert.Contains(second.Keys, k => k == "foo18");
            Assert.Contains(second.Values, v => v == 18);
            Assert.All(second.Children, Assert.Null);
            Assert.Same(root, second.Parent);
            Assert.Same(first, second.LeftSibling);
            Assert.Same(third, second.RightSibling);

            Assert.NotNull(third);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, third.KeysNumber);
            Assert.All(third.Children, Assert.Null);
            Assert.Same(root, third.Parent);
            Assert.Same(second, third.LeftSibling);
            Assert.Null(third.RightSibling);
        }
        
        [Fact]
        public async Task Delete__DeleteOneKey_RightSiblingHaveMoreKeys__MoveKeyFromRightSibling()
        {
            // arrange
            await SetupTree(3 * GlobalConstant.MinBPlusTreeDegree); // 3 children
            await _tree.Insert("foo4444", 4444);
            var key = _tree.Root.Children[1].Keys[0];

            // act
            var result = await _tree.Delete(key);
            
            // assert
            Assert.True(result);
            var root = _tree.Root;
            Assert.False(root.IsLeaf);
            Assert.Equal(2, root.KeysNumber);
            Assert.Equal(3, root.Children.Count(c => c != null));
            Assert.Equal("foo2", root.Keys[0]);
            Assert.Equal("foo29", root.Keys[1]);
            
            var first = root.Children[0];
            var second = root.Children[1];
            var third = root.Children[2];

            Assert.NotNull(first);
            Assert.True(first.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, first.KeysNumber);
            Assert.All(first.Children, Assert.Null);
            Assert.Same(root, first.Parent);
            Assert.Null(first.LeftSibling);
            Assert.Same(second, first.RightSibling);
            
            Assert.NotNull(second);
            Assert.True(second.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, second.KeysNumber);
            Assert.DoesNotContain(second.Keys, k => k == "foo19");
            Assert.DoesNotContain(second.Values, v => v == 19);
            Assert.Equal("foo28", second.Keys[second.KeysNumber - 1]);
            Assert.All(second.Children, Assert.Null);
            Assert.Same(root, second.Parent);
            Assert.Same(first, second.LeftSibling);
            Assert.Same(third, second.RightSibling);

            Assert.NotNull(third);
            Assert.True(third.IsLeaf);
            Assert.Equal(GlobalConstant.MinBPlusTreeDegree, third.KeysNumber);
            Assert.Equal("foo29", third.Keys[0]);
            Assert.All(third.Children, Assert.Null);
            Assert.Same(root, third.Parent);
            Assert.Same(second, third.LeftSibling);
            Assert.Null(third.RightSibling);
        }

        [Fact]
        public async Task Delete__DeleteOneKey_LeftAndRightSiblingsHasEqualNumberOfNodes__True()
        {
            // arrange
            await SetupTree(3 * GlobalConstant.MinBPlusTreeDegree); // 3 children
            var key = _tree.Root.Children[1].Keys[0];

            // act
            var result = await _tree.Delete(key);

            // assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task Delete__AddMany_DeleteAll__EmptyTree()
        {
            // arrange
            const int size = 200;
            foreach (var i in Enumerable.Range(1, size))
            {
                var number = i < 10 ? $"00{i}" : i < 100 ? $"0{i}" : i.ToString();
                await _tree.Insert($"foo{number}", i);
            }
            var results = new List<bool>();

            // act
            foreach (var i in Enumerable.Range(1, size))
            {
                var number = i < 10 ? $"00{i}" : i < 100 ? $"0{i}" : i.ToString();
                results.Add(await _tree.Delete($"foo{number}"));
            }

            // assert
            Assert.All(results, Assert.True);
        }

        private async Task SetupTree(int keysCount)
        {
            foreach (var i in Enumerable.Range(1, keysCount))
            {
                await _tree.Insert($"foo{i}", i);
            }
        }
    }
}