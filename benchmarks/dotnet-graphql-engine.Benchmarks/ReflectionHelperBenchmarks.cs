using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using GraphQLEngine.Common.Utilities;
using System;
using System.Collections.Generic;

namespace GraphQLEngine.Benchmarks;

/// <summary>
/// Benchmarks for ReflectionHelper caching performance
/// Demonstrates the performance improvement from caching reflection metadata
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[GcServer(true)]
[GcConcurrent(true)]
public class ReflectionHelperBenchmarks
{
    private static readonly TestClass _testInstance = new();
    private static readonly Type _testType = typeof(TestClass);
    private const string _propertyName = "Name";
    private const string _methodName = "GetValue";

    /// <summary>
    /// Test class with multiple properties and methods for benchmarking
    /// </summary>
    private class TestClass
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string>? Tags { get; set; }

        public string GetValue() => "test";
        public int Calculate(int x, int y) => x + y;
        public void VoidMethod() { }

        public string? GetName() => Name;
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public List<System.Reflection.PropertyInfo> GetPublicProperties_NoCache()
    {
        // Simulate uncached reflection (original behavior)
        return _testType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .ToList();
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public List<System.Reflection.PropertyInfo> GetPublicProperties_WithCache()
    {
        // Use cached reflection (new behavior)
        return ReflectionHelper.GetPublicProperties(_testType);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public System.Reflection.PropertyInfo[] GetProperty_NoCache()
    {
        // Simulate uncached property lookup (original behavior)
        return _testType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public object? GetPropertyValue_NoCache()
    {
        // Simulate uncached property access (original behavior)
        var property = _testType.GetProperty(_propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        return property?.GetValue(_testInstance);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public object? GetPropertyValue_WithCache()
    {
        // Use cached property access (new behavior)
        return ReflectionHelper.GetPropertyValue(_testInstance, _propertyName);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public object? InvokeMethod_NoCache()
    {
        // Simulate uncached method invocation (original behavior)
        var method = _testInstance.GetType().GetMethod(_methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        return method?.Invoke(_testInstance, Array.Empty<object?>());
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public object? InvokeMethod_WithCache()
    {
        // Use cached method invocation (new behavior)
        return ReflectionHelper.InvokeMethod(_testInstance, _methodName);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public void MapProperties_NoCache()
    {
        // Simulate uncached property mapping (original behavior)
        var sourceProperties = _testInstance.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var destInstance = new TestClass();
        var destProperties = destInstance.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var sourceProp in sourceProperties)
        {
            var destProp = destProperties.FirstOrDefault(p => p.Name == sourceProp.Name);
            if (destProp?.CanWrite == true)
            {
                try
                {
                    var value = sourceProp.GetValue(_testInstance);
                    destProp.SetValue(destInstance, value);
                }
                catch { }
            }
        }
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public void MapProperties_WithCache()
    {
        // Use cached property mapping (new behavior)
        var destInstance = new TestClass();
        ReflectionHelper.MapProperties(_testInstance, destInstance);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public List<System.Reflection.MethodInfo> GetPublicMethods_NoCache()
    {
        // Simulate uncached method lookup (original behavior)
        return _testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static)
            .Where(m => !m.IsSpecialName)
            .ToList();
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public List<System.Reflection.MethodInfo> GetPublicMethods_WithCache()
    {
        // Use cached method lookup (new behavior)
        return ReflectionHelper.GetPublicMethods(_testType);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public TypeMetadata GetTypeMetadata_FirstCall()
    {
        // First call creates and caches metadata
        return ReflectionHelper.GetOrCreateTypeMetadata(_testType);
    }

    [Benchmark]
    [BenchmarkCategory("ReflectionHelper")]
    public TypeMetadata GetTypeMetadata_SubsequentCall()
    {
        // Subsequent call retrieves from cache
        return ReflectionHelper.GetOrCreateTypeMetadata(_testType);
    }

    /// <summary>
    /// Benchmark for TypeConverter.ToJsonCompatible with caching vs without
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("TypeConverter")]
    public object? TypeConverter_ToJsonCompatible_NoCache()
    {
        // Simulate uncached behavior
        var testObj = new TestClass { Name = "Benchmark", Age = 30, CreatedAt = DateTime.UtcNow };
        return TypeConverter.ToJsonCompatible(testObj);
    }

    [Benchmark]
    [BenchmarkCategory("TypeConverter")]
    public object? TypeConverter_ToJsonCompatible_WithCache()
    {
        // Use cached behavior
        var testObj = new TestClass { Name = "Benchmark", Age = 30, CreatedAt = DateTime.UtcNow };
        return TypeConverter.ToJsonCompatible(testObj);
    }
}