using BenchmarkDotNet.Core.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Statistics;
using BenchmarkDotNet.Mathematics.Random;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.UnitTesting;
using System.Runtime.InteropServices;

namespace BenchmarkDotNet
{
    [MemoryDiagnoser]
    public class QueryFieldBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            // TODO: Set up test data here
        }

        [Benchmark]
        public void Benchmark_QueryField_Query()
        {
            // TODO: Add benchmark code here
            var queryField = new QueryField();
            var query = new Query();
            queryField.AddQuery(query);
            queryField.RemoveQuery(query);
        }

        [Benchmark]
        [Params(10)]
        public void Benchmark_QueryField_Query_With_Params()
        {
            // TODO: Add benchmark code here
            var queryField = new QueryField();
            var query = new Query();
            for (int i = 0; i < 10; i++)
            {
                queryField.AddQuery(query);
            }
        }

        [Benchmark]
        [Params(100)]
        public void Benchmark_QueryField_Query_With_Params_100()
        {
            // TODO: Add benchmark code here
            var queryField = new QueryField();
            var query = new Query();
            for (int i = 0; i < 100; i++)
            {
                queryField.AddQuery(query);
            }
        }
    }
}