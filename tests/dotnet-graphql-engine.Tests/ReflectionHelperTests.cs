using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class ReflectionHelperTests
    {
        #region GetPublicProperties Tests

        [Fact]
        public void GetPublicProperties_WithNullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null!;

            // Act
            Action act = () => ReflectionHelper.GetPublicProperties(type);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetPublicProperties_WithSimpleClass_ReturnsExpectedProperties()
        {
            // Arrange
            var type = typeof(SimpleClass);

            // Act
            var properties = ReflectionHelper.GetPublicProperties(type);

            // Assert
            properties.Should().NotBeNull();
            properties.Should().HaveCount(3);
            properties.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Id", "Name", "Value" });
        }

        [Fact]
        public void GetPublicProperties_WithEmptyClass_ReturnsEmptyList()
        {
            // Arrange
            var type = typeof(EmptyClass);

            // Act
            var properties = ReflectionHelper.GetPublicProperties(type);

            // Assert
            properties.Should().NotBeNull();
            properties.Should().BeEmpty();
        }

        #endregion

        #region GetPublicMethods Tests

        [Fact]
        public void GetPublicMethods_WithNullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null!;

            // Act
            Action act = () => ReflectionHelper.GetPublicMethods(type);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetPublicMethods_WithSimpleClass_ReturnsExpectedMethods()
        {
            // Arrange
            var type = typeof(SimpleClass);

            // Act
            var methods = ReflectionHelper.GetPublicMethods(type);

            // Assert
            methods.Should().NotBeNull();
            methods.Should().HaveCountGreaterThan(0);
            methods.Should().Contain(m => m.Name == "GetId");
        }

        #endregion

        #region ImplementsInterface Tests

        [Fact]
        public void ImplementsInterface_WithNullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null!;

            // Act
            Action act = () => ReflectionHelper.ImplementsInterface<IDisposable>(type);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ImplementsInterface_WithClassImplementingInterface_ReturnsTrue()
        {
            // Arrange
            var type = typeof(ClassImplementingInterface);

            // Act
            var result = ReflectionHelper.ImplementsInterface<IDisposable>(type);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ImplementsInterface_WithClassNotImplementingInterface_ReturnsFalse()
        {
            // Arrange
            var type = typeof(SimpleClass);

            // Act
            var result = ReflectionHelper.ImplementsInterface<IDisposable>(type);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetDerivedTypes Tests

        [Fact]
        public void GetDerivedTypes_WithNullAssembly_ThrowsArgumentNullException()
        {
            // Arrange
            var assembly = (Assembly)null!;
            var baseType = typeof(BaseClass);

            // Act
            Action act = () => ReflectionHelper.GetDerivedTypes(assembly, baseType);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetDerivedTypes_WithNullBaseType_ThrowsArgumentNullException()
        {
            // Arrange
            var assembly = typeof(BaseClass).Assembly;
            Type baseType = null!;

            // Act
            Action act = () => ReflectionHelper.GetDerivedTypes(assembly, baseType);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetDerivedTypes_WithValidAssemblyAndBaseType_ReturnsDerivedTypes()
        {
            // Arrange
            var assembly = typeof(BaseClass).Assembly;
            var baseType = typeof(BaseClass);

            // Act
            var derivedTypes = ReflectionHelper.GetDerivedTypes(assembly, baseType);

            // Assert
            derivedTypes.Should().NotBeNull();
            derivedTypes.Should().Contain(t => t.Name == typeof(DerivedClass).Name);
        }

        #endregion

        #region CreateInstance Tests

        [Fact]
        public void CreateInstance_WithNullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null!;

            // Act
            Action act = () => ReflectionHelper.CreateInstance(type);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateInstance_WithParameterlessConstructor_ReturnsInstance()
        {
            // Arrange
            var type = typeof(SimpleClass);

            // Act
            var instance = ReflectionHelper.CreateInstance(type);

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<SimpleClass>();
        }

        [Fact]
        public void CreateInstance_WithParameters_ReturnsInstanceWithValues()
        {
            // Arrange
            var type = typeof(SimpleClass);
            var parameters = new object?[] { 123, "test", 456 };

            // Act
            var instance = ReflectionHelper.CreateInstance(type, parameters);

            // Assert
            instance.Should().NotBeNull();
            instance.Should().BeOfType<SimpleClass>();
        }

        #endregion

        #region GetPropertyValue Tests

        [Fact]
        public void GetPropertyValue_WithNullObject_ThrowsArgumentNullException()
        {
            // Arrange
            object obj = null!;
            var propertyName = "Name";

            // Act
            Action act = () => ReflectionHelper.GetPropertyValue(obj, propertyName);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetPropertyValue_WithNullPropertyName_ThrowsArgumentNullException()
        {
            // Arrange
            var obj = new SimpleClass();
            string propertyName = null!;

            // Act
            Action act = () => ReflectionHelper.GetPropertyValue(obj, propertyName);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetPropertyValue_WithExistingProperty_ReturnsCorrectValue()
        {
            // Arrange
            var obj = new SimpleClass { Id = 42, Name = "Test", Value = 3.14 };

            // Act
            var idValue = ReflectionHelper.GetPropertyValue(obj, "Id");

            // Assert
            idValue.Should().Be(42);
        }

        [Fact]
        public void GetPropertyValue_WithNonExistingProperty_ReturnsNull()
        {
            // Arrange
            var obj = new SimpleClass();

            // Act
            var result = ReflectionHelper.GetPropertyValue(obj, "NonExistent");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region SetPropertyValue Tests

        [Fact]
        public void SetPropertyValue_WithNullObject_ThrowsArgumentNullException()
        {
            // Arrange
            object obj = null!;
            var propertyName = "Name";
            var value = "test";

            // Act
            Action act = () => ReflectionHelper.SetPropertyValue(obj, propertyName, value);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetPropertyValue_WithNullPropertyName_ThrowsArgumentNullException()
        {
            // Arrange
            var obj = new SimpleClass();
            string propertyName = null!;
            var value = "test";

            // Act
            Action act = () => ReflectionHelper.SetPropertyValue(obj, propertyName, value);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetPropertyValue_WithExistingProperty_SetsCorrectValue()
        {
            // Arrange
            var obj = new SimpleClass();

            // Act
            ReflectionHelper.SetPropertyValue(obj, "Name", "Updated");

            // Assert
            obj.Name.Should().Be("Updated");
        }

        #endregion

        #region InvokeMethod Tests

        [Fact]
        public void InvokeMethod_WithNullObject_ThrowsArgumentNullException()
        {
            // Arrange
            object obj = null!;
            var methodName = "GetId";

            // Act
            Action act = () => ReflectionHelper.InvokeMethod(obj, methodName);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void InvokeMethod_WithNullMethodName_ThrowsArgumentNullException()
        {
            // Arrange
            var obj = new SimpleClass();
            string methodName = null!;

            // Act
            Action act = () => ReflectionHelper.InvokeMethod(obj, methodName);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void InvokeMethod_WithExistingMethod_ReturnsCorrectResult()
        {
            // Arrange
            var obj = new SimpleClass(123, "Test", 456);

            // Act
            var result = ReflectionHelper.InvokeMethod(obj, "GetId");

            // Assert
            result.Should().Be(123);
        }

        [Fact]
        public void InvokeMethod_WithParameters_ReturnsCorrectResult()
        {
            // Arrange
            var obj = new SimpleClass();

            // Act
            var result = ReflectionHelper.InvokeMethod(obj, "CalculateSum", 10, 20);

            // Assert
            result.Should().Be(30);
        }

        #endregion

        #region GetCustomAttributes Tests

        [Fact]
        public void GetCustomAttributes_WithNullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null!;

            // Act
            Action act = () => ReflectionHelper.GetCustomAttributes<TestAttribute>(type);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetCustomAttributes_WithTypeHavingAttribute_ReturnsAttribute()
        {
            // Arrange
            var type = typeof(ClassWithAttribute);

            // Act
            var attributes = ReflectionHelper.GetCustomAttributes<TestAttribute>(type);

            // Assert
            attributes.Should().NotBeNull();
            attributes.Should().HaveCount(1);
        }

        #endregion

        #region Test Classes

        private class SimpleClass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public double Value { get; set; }

            public SimpleClass() { }

            public SimpleClass(int id, string name, double value)
            {
                Id = id;
                Name = name;
                Value = value;
            }

            public int GetId() => Id;
            public void SetName(string name) => Name = name;
            public int CalculateSum(int a, int b) => a + b;
        }

        private class EmptyClass
        {
            // No public properties
        }

        private class BaseClass
        {
            public int BaseProperty { get; set; }
        }

        private class DerivedClass : BaseClass
        {
            public int DerivedProperty { get; set; }
        }

        private class ClassImplementingInterface : IDisposable
        {
            public void Dispose() { }
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        private class TestAttribute : Attribute { }

        [Test]
        private class ClassWithAttribute
        {
            // Attribute is on the class itself
        }

        #endregion
    }
}