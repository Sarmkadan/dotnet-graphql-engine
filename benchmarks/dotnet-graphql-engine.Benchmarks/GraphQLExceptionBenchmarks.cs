using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Diagnostics;
using BenchmarkDotNet.Diagnostics.Memory;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading;
using GraphQLException = Exceptions.GraphQLException;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BenchmarkDotNet
{
    [MemoryDiagnoser]
    public class GraphQLExceptionBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            // Setup test data
        }

        [Benchmark]
        public void Benchmark_ThrowGraphQLException_10Times()
        {
            // Benchmark throwing GraphQLException 10 times
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    throw new GraphQLException();
                }
                catch (GraphQLException ex)
                {
                    // Handle the exception
                }
            }
        }

        [Benchmark]
        [Params(10)]
        public void Benchmark_ThrowGraphQLException_Parametrized(int n)
        {
            // Benchmark throwing GraphQLException n times
            for (int i = 0; i < n; i++)
            {
                try
                {
                    throw new GraphQLException();
                }
                catch (GraphQLException ex)
                {
                    // Handle the exception
                }
            }
        }
    }
}