using System.Linq;
using Kafka.LinqToKafka;
using Xunit;

namespace Kafka.LinqToKafka.Tests;

public class KafkaQueryProviderTests
{
    private readonly List<TestEntity> _testData =
    [
        new() { Id = 1, Name = "Alice", Age = 30 },
        new() { Id = 2, Name = "Bob", Age = 25 },
        new() { Id = 3, Name = "Charlie", Age = 35 },
        new() { Id = 4, Name = "David", Age = 20 },
        new() { Id = 5, Name = "Eve", Age = 28 }
    ];

    [Fact]
    public void Where_FiltersCorrectly()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.Where(e => e.Age > 25).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, e => e.Name == "Alice");
        Assert.Contains(result, e => e.Name == "Charlie");
        Assert.Contains(result, e => e.Name == "Eve");
        Assert.DoesNotContain(result, e => e.Name == "Bob");
        Assert.DoesNotContain(result, e => e.Name == "David");
    }

    [Fact]
    public void OrderBy_OrdersAscending()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.OrderBy(e => e.Age).ToList();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal("David", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Eve", result[2].Name);
        Assert.Equal("Alice", result[3].Name);
        Assert.Equal("Charlie", result[4].Name);
    }

    [Fact]
    public void OrderByDescending_OrdersDescending()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.OrderByDescending(e => e.Age).ToList();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal("Charlie", result[0].Name);
        Assert.Equal("Alice", result[1].Name);
        Assert.Equal("Eve", result[2].Name);
        Assert.Equal("Bob", result[3].Name);
        Assert.Equal("David", result[4].Name);
    }

    [Fact]
    public void Take_LimitsResults()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.Take(2).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Skip_SkipsResults()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.Skip(2).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Charlie", result[0].Name);
    }

    [Fact]
    public void Where_And_Take_Combined()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.Where(e => e.Age > 25).Take(2).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Name == "Alice");
        Assert.Contains(result, e => e.Name == "Charlie");
    }

    [Fact]
    public void OrderBy_And_Take_Combined()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.OrderBy(e => e.Age).Take(3).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("David", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Eve", result[2].Name);
    }

    [Fact]
    public void Skip_And_Take_Combined()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.Skip(1).Take(2).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Bob", result[0].Name);
        Assert.Equal("Charlie", result[1].Name);
    }

    [Fact]
    public void Where_And_OrderBy_Combined()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source.Where(e => e.Age > 25).OrderBy(e => e.Name).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Charlie", result[1].Name);
        Assert.Equal("Eve", result[2].Name);
    }

    [Fact]
    public void ComplexQuery_Where_OrderByDescending_Skip_Take()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var result = source
            .Where(e => e.Age >= 25)
            .OrderByDescending(e => e.Age)
            .Skip(1)
            .Take(2)
            .ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Eve", result[1].Name);
    }

    [Fact]
    public void EmptySource_ReturnsEmpty()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(new List<TestEntity>());

        // Act
        var result = source.Where(e => e.Age > 25).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Count_ExecutesCorrectly()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var count = source.Where(e => e.Age > 25).Count();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void First_ExecutesCorrectly()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var first = source.OrderBy(e => e.Age).First();

        // Assert
        Assert.Equal("David", first.Name);
    }

    [Fact]
    public void Single_WithPredicate_ExecutesCorrectly()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var single = source.Single(e => e.Age == 20);

        // Assert
        Assert.Equal("David", single.Name);
    }

    [Fact]
    public void Any_ExecutesCorrectly()
    {
        // Arrange
        var source = new KafkaQueryable<TestEntity>(_testData);

        // Act
        var hasYoung = source.Any(e => e.Age < 25);
        var hasOld = source.Any(e => e.Age > 40);

        // Assert
        Assert.True(hasYoung);
        Assert.False(hasOld);
    }
}
