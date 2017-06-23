using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Company.WebApplication1.Core.Query.Tests
{
    public class QueryableExtensionTests
    {
        [Fact]
        public void Paged_QueryArgumentIsNull_ThrowsArgNullException()
        {
            // Act
            Action result = () => QueryableExtensions.Paged<string>(null, 1, 2);

            // Assert
            Assert.Throws<ArgumentNullException>(result);
        }

        [Fact]
        public void Paged_PageArgumentIsLowerThanOne_ThrowsArgOutOfRangeException()
        {
            // Arrange
            var queryable = new []{"a", "b", "c"}.AsQueryable();
            var pageNumber = 0;

            // Act
            Action result = () => queryable.Paged(pageNumber, 2);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(result);
        }

        [Fact]
        public void Paged_PageSizeArgumentIsLowerThanOne_ThrowsArgOutOfRangeException()
        {
            // Arrange
            var queryable = new []{"a", "b", "c"}.AsQueryable();
            var pageSize = 0;

            // Act
            Action result = () => queryable.Paged(1, pageSize);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(result);
        }

        [Fact]
        public void Paged_RequestPageOne_ReturnsPageOneResult()
        {
            // Arrange
            var expected = new List<string>{"a", "b"};
            var queryable = new List<string>{"a", "b", "c"}.AsQueryable();

            // Act
            var actual = queryable.Paged(1, 2).ToList();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Paged_RequestPageTwo_ReturnsPageTwoResult()
        {
            // Arrange
            var expected = new List<string>{"c"};
            var queryable = new List<string>{"a", "b", "c"}.AsQueryable();

            // Act
            var actual = queryable.Paged(2, 2).ToList();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
