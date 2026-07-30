using System.Collections.Generic;
using Xunit;
using GraphQLEngine.Services.Caching;

namespace GraphQLEngine.Tests
{
    /// <summary>
    /// Tests for the <see cref="CacheKeyBuilder"/> class.
    /// </summary>
    public class CacheKeyBuilderTests
    {
        private const string SchemaName = "TestSchema";
        private const string Query = "{ user { id name } }";

        /// <summary>
        /// Tests that the <see cref="CacheKeyBuilder.BuildQueryKey(string,string,System.Collections.Generic.IDictionary{string,object})"/> method produces the same key for the same input.
        /// </summary>
        [Fact]
        public void BuildQueryKey_IsDeterministic()
        {
            var variables = new Dictionary<string, object>
            {
                { "id", 123 },
                { "includePosts", true }
            };

            var key1 = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, variables);
            var key2 = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, variables);

            Assert.Equal(key1, key2);
        }

        /// <summary>
        /// Tests that the <see cref="CacheKeyBuilder.BuildQueryKey(string,string,System.Collections.Generic.IDictionary{string,object})"/> method produces different keys for different variable dictionaries.
        /// </summary>
        [Fact]
        public void BuildQueryKey_WithDifferentVariables_GeneratesDifferentKeys()
        {
            var vars1 = new Dictionary<string, object>
            {
                { "id", 123 },
                { "includePosts", true }
            };

            var vars2 = new Dictionary<string, object>
            {
                { "id", 456 },
                { "includePosts", false }
            };

            var key1 = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, vars1);
            var key2 = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, vars2);

            Assert.NotEqual(key1, key2);
            Assert.Contains(":vars:", key1);
            Assert.Contains(":vars:", key2);
        }

        /// <summary>
        /// Tests that the <see cref="CacheKeyBuilder.BuildQueryKey(string,string,System.Collections.Generic.IDictionary{string,object})"/> method produces the same key regardless of the order of variables in the dictionary.
        /// </summary>
        [Fact]
        public void BuildQueryKey_VariableOrderIsInsignificant()
        {
            var varsA = new Dictionary<string, object>
            {
                { "a", 1 },
                { "b", 2 }
            };

            var varsB = new Dictionary<string, object>
            {
                { "b", 2 },
                { "a", 1 }
            };

            var keyA = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, varsA);
            var keyB = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, varsB);

            Assert.Equal(keyA, keyB);
        }

        /// <summary>
        /// Tests that the <see cref="CacheKeyBuilder.BuildQueryKey(string,string,System.Collections.Generic.IDictionary{string,object})"/> method does not include a variables segment when the variables dictionary is null or empty.
        /// </summary>
        [Fact]
        public void BuildQueryKey_WithNullOrEmptyVariables_NoVarsSegment()
        {
            var keyWithNull = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, null);
            var keyWithEmpty = CacheKeyBuilder.BuildQueryKey(SchemaName, Query, new Dictionary<string, object>());

            Assert.DoesNotContain(":vars:", keyWithNull);
            Assert.DoesNotContain(":vars:", keyWithEmpty);
        }

        /// <summary>
        /// Tests that the <see cref="CacheKeyBuilder.WithNamespace(string)"/> method correctly prefixes the namespace to the generated key.
        /// </summary>
        [Fact]
        public void BuildQueryKeyNs_PrefixesNamespace()
        {
            var ns = "tenant-a:";
            var builder = CacheKeyBuilder.WithNamespace(ns);

            var key = builder.BuildQueryKeyNs(SchemaName, Query);

            Assert.StartsWith(ns, key);
            // The rest of the key should match the static method output
            var expected = CacheKeyBuilder.BuildQueryKey(SchemaName, Query);
            Assert.Equal(expected, key.Substring(ns.Length));
        }
    }
}
