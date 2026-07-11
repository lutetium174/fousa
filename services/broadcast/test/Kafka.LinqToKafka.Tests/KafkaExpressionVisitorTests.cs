using System.Linq.Expressions;

namespace Kafka.LinqToKafka.Tests;

public class KafkaExpressionVisitorTests
{
    [Fact]
    public void VisitBinary_Equal_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var left = Expression.Property(param, nameof(TestEntity.Age));
        var right = Expression.Constant(30);
        var binary = Expression.Equal(left, right);

        // Act
        visitor.Visit(binary);

        // Assert
        Assert.Contains(" = ", visitor.Query);
        Assert.Contains("30", visitor.Query);
    }

    [Fact]
    public void VisitBinary_Equal_WithMemberAccessOnBothSides_ProducesCorrectQuery()
    {
        var visitor = new KafkaExpressionVisitor();
        var messageParam = Expression.Parameter(typeof(TestMessage), "x");
        var filter = new Filter { Sender = Guid.Parse("11111111-1111-1111-1111-111111111111") };
        var filterConstant = Expression.Constant(filter);
        
        var left = Expression.Property(messageParam, nameof(TestMessage.Sender));
        var right = Expression.Property(filterConstant, nameof(Filter.Sender));
        
        var lambda = Expression.Lambda<Func<TestMessage, bool>>(
            Expression.Equal(left, right),
            messageParam);
        
        var source = Expression.Constant(new List<TestMessage>().AsQueryable());
        var whereCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Where),
            [typeof(TestMessage)],
            source,
            lambda);

        // Act
        visitor.Visit(whereCall);

        // Assert
        Assert.Contains(" = ", visitor.Query);
        Assert.Contains("WHERE", visitor.Query);
        Assert.Contains("Sender = '11111111-1111-1111-1111-111111111111'", visitor.Query);
    }

    [Fact]
    public void VisitMethodCall_Where_WithFilterObjectSender_ProducesCorrectQuery()
    {
        var visitor = new KafkaExpressionVisitor();
        var filter = new Filter { Sender = Guid.Parse("11111111-1111-1111-1111-111111111111") };
        var filterConstant = Expression.Constant(filter);
        
        var messageParam = Expression.Parameter(typeof(TestMessage), "x");
        var senderProp = Expression.Property(messageParam, nameof(TestMessage.Sender));
        var filterSenderProp = Expression.Property(filterConstant, nameof(Filter.Sender));
        
        var whereClause = Expression.Lambda<Func<TestMessage, bool>>(
            Expression.Equal(senderProp, filterSenderProp), 
            messageParam);
        
        var source = Expression.Constant(new List<TestMessage>().AsQueryable());
        var whereCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Where),
            [typeof(TestMessage)],
            source,
            whereClause);

        // Act
        visitor.Visit(whereCall);

        // Assert
        Assert.Contains("WHERE", visitor.Query);
        Assert.Contains("Sender = '11111111-1111-1111-1111-111111111111'", visitor.Query);
    }
    
    [Fact]
    public void VisitBinary_GreaterThan_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var left = Expression.Property(param, nameof(TestEntity.Age));
        var right = Expression.Constant(25);
        var binary = Expression.GreaterThan(left, right);

        // Act
        visitor.Visit(binary);

        // Assert
        Assert.Contains(" > ", visitor.Query);
        Assert.Contains("25", visitor.Query);
    }

    [Fact]
    public void VisitBinary_AndAlso_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var ageProp = Expression.Property(param, nameof(TestEntity.Age));
        var ageConst = Expression.Constant(25);
        var nameProp = Expression.Property(param, nameof(TestEntity.Name));
        var nameConst = Expression.Constant("Alice");
        var left = Expression.GreaterThan(ageProp, ageConst);
        var right = Expression.Equal(nameProp, nameConst);
        var binary = Expression.AndAlso(left, right);

        // Act
        visitor.Visit(binary);

        // Assert
        Assert.Contains(" AND ", visitor.Query);
        Assert.Contains(" > ", visitor.Query);
        Assert.Contains(" = ", visitor.Query);
    }

    [Fact]
    public void VisitBinary_LessThan_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var left = Expression.Property(param, nameof(TestEntity.Age));
        var right = Expression.Constant(30);
        var binary = Expression.LessThan(left, right);

        // Act
        visitor.Visit(binary);

        // Assert
        Assert.Contains(" < ", visitor.Query);
    }

    [Fact]
    public void VisitBinary_NotEqual_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var left = Expression.Property(param, nameof(TestEntity.Name));
        var right = Expression.Constant("Alice");
        var binary = Expression.NotEqual(left, right);

        // Act
        visitor.Visit(binary);

        // Assert
        Assert.Contains(" <> ", visitor.Query);
    }

    [Fact]
    public void VisitMethodCall_Where_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var ageProp = Expression.Property(param, nameof(TestEntity.Age));
        var ageConst = Expression.Constant(25);
        var whereClause = Expression.Lambda<Func<TestEntity, bool>>(
            Expression.GreaterThan(ageProp, ageConst), param);
        
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        var whereCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Where),
            [typeof(TestEntity)],
            source,
            whereClause);

        // Act
        visitor.Visit(whereCall);

        // Assert
        Assert.Contains("WHERE", visitor.Query);
    }

    [Fact]
    public void VisitMethodCall_OrderBy_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var ageProp = Expression.Property(param, nameof(TestEntity.Age));
        var orderByClause = Expression.Lambda<Func<TestEntity, int>>(ageProp, param);
        
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        var orderByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.OrderBy),
            [typeof(TestEntity), typeof(int)],
            source,
            orderByClause);

        // Act
        visitor.Visit(orderByCall);

        // Assert
        Assert.Contains("ORDER BY", visitor.Query);
        Assert.Contains("ASC", visitor.Query);
    }

    [Fact]
    public void VisitMethodCall_OrderByDescending_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var ageProp = Expression.Property(param, nameof(TestEntity.Age));
        var orderByClause = Expression.Lambda<Func<TestEntity, int>>(ageProp, param);
        
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        var orderByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.OrderByDescending),
            [typeof(TestEntity), typeof(int)],
            source,
            orderByClause);

        // Act
        visitor.Visit(orderByCall);

        // Assert
        Assert.Contains("ORDER BY", visitor.Query);
        Assert.Contains("DESC", visitor.Query);
    }

    [Fact]
    public void VisitMethodCall_Take_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        var takeCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Take),
            [typeof(TestEntity)],
            source,
            Expression.Constant(5));

        // Act
        visitor.Visit(takeCall);

        // Assert
        Assert.Contains("LIMIT", visitor.Query);
    }

    [Fact]
    public void VisitMethodCall_Skip_ProducesCorrectQuery()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        var skipCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Skip),
            [typeof(TestEntity)],
            source,
            Expression.Constant(2));

        // Act
        visitor.Visit(skipCall);

        // Assert
        Assert.Contains("OFFSET", visitor.Query);
    }

    [Fact]
    public void VisitMember_AccessesPropertyCorrectly()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var member = Expression.Property(param, nameof(TestEntity.Name));

        // Act
        visitor.Visit(member);

        // Assert
        Assert.Contains("Name", visitor.Query);
    }

    [Fact]
    public void VisitConstant_WithIQueryable_ProducesFromClause()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var queryable = new List<TestEntity>().AsQueryable();
        var constant = Expression.Constant(queryable);

        // Act
        visitor.Visit(constant);

        // Assert
        Assert.Contains("FROM", visitor.Query);
        Assert.Contains("TestEntity", visitor.Query);
    }

    [Fact]
    public void FullQuery_WhereAndOrderBy_ProducesCorrectStructure()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var ageProp = Expression.Property(param, nameof(TestEntity.Age));
        var ageConst = Expression.Constant(25);
        var whereClause = Expression.Lambda<Func<TestEntity, bool>>(
            Expression.GreaterThan(ageProp, ageConst), param);
        
        var nameProp = Expression.Property(param, nameof(TestEntity.Name));
        var orderByClause = Expression.Lambda<Func<TestEntity, string>>(nameProp, param);
        
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        
        var whereCall = Expression.Call(
            typeof(Queryable), nameof(Queryable.Where), [typeof(TestEntity)], source, whereClause);
        
        var orderByCall = Expression.Call(
            typeof(Queryable), nameof(Queryable.OrderBy), [typeof(TestEntity), typeof(string)], 
            whereCall, orderByClause);

        // Act
        visitor.Visit(orderByCall);

        // Assert
        Assert.Contains("FROM \"TestEntity\" ", visitor.Query);
        Assert.Contains("WHERE (Age > 25)", visitor.Query);
        Assert.Contains("ORDER BY Name ASC", visitor.Query);
    }

    [Fact]
    public void OrderBy_ProducesCorrectOrder_ColumnBeforeDirection()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var nameProp = Expression.Property(param, nameof(TestEntity.Name));
        var orderByClause = Expression.Lambda<Func<TestEntity, string>>(nameProp, param);
        
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        var orderByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.OrderBy),
            [typeof(TestEntity), typeof(string)],
            source,
            orderByClause);

        // Act
        visitor.Visit(orderByCall);

        // Assert
        var query = visitor.Query;
        var nameIndex = query.IndexOf("Name", StringComparison.Ordinal);
        var ascIndex = query.IndexOf("ASC", StringComparison.Ordinal);
        Assert.True(nameIndex < ascIndex, "Column name should appear before ASC direction");
    }

    [Fact]
    public void OrderByDescending_ProducesCorrectOrder_ColumnBeforeDirection()
    {
        // Arrange
        var visitor = new KafkaExpressionVisitor();
        var param = Expression.Parameter(typeof(TestEntity), "e");
        var ageProp = Expression.Property(param, nameof(TestEntity.Age));
        var orderByClause = Expression.Lambda<Func<TestEntity, int>>(ageProp, param);
        
        var source = Expression.Constant(new List<TestEntity>().AsQueryable());
        var orderByCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.OrderByDescending),
            [typeof(TestEntity), typeof(int)],
            source,
            orderByClause);

        // Act
        visitor.Visit(orderByCall);

        // Assert
        var query = visitor.Query;
        var ageIndex = query.IndexOf("Age", StringComparison.Ordinal);
        var descIndex = query.IndexOf("DESC", StringComparison.Ordinal);
        Assert.True(ageIndex < descIndex, "Column name should appear before DESC direction");
    }
}

internal class Filter
{
    public Guid Sender { get; set; }
}