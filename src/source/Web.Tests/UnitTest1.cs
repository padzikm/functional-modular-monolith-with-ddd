using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Web.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public async Task Test1()
        {
            var factory = new WebApplicationFactory<Startup>();
            var httpClient = factory.CreateClient();
            var q = QueryHelpers.AddQueryString("testing/bla", new Dictionary<string, string>(){ ["bla"] = "costam"});
            var result = await httpClient.GetAsync(q);
            var cont = await result.Content.ReadAsStringAsync();
            _output.WriteLine("output from http");
            _output.WriteLine(cont);
            result.EnsureSuccessStatusCode();
        }
    }
}
