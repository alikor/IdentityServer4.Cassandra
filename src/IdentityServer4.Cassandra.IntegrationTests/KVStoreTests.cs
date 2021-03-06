using System;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer4.Cassandra.IntegrationTests
{
    [Trait("Category","Integration")]
    public class KeyValueStoreTests: CassandraIntegrationTestBase
    {
        [Fact]
        public async Task StoresThenRetrievesByKey()
        {
            var kvStore = CassandraKeyValueStore<String,TestKvDto>.Initialize(_session, "kv_table1");
            var expected= new TestKvDto(){ Id="abc", Name="test data"};
            await kvStore.SaveAsync("abc", expected);
            var actual =await kvStore.GetAsync("abc");

            Assert.NotNull(actual);
            Assert.Equal(expected.Name,actual.Name);
        }


        [Fact]
        public async Task StoresThenRetrievesAll()
        {
            var kvStore = CassandraKeyValueStore<String,TestKvDto>.Initialize(_session, "kv_table3");
            var expected1= new TestKvDto(){ Id="abc", Name="test data"};
            var expected2= new TestKvDto(){ Id="def", Name="test data"};
            var expected3= new TestKvDto(){ Id="ghi", Name="test data"};
            await kvStore.SaveAsync(expected1.Id, expected1);
            await kvStore.SaveAsync(expected2.Id, expected2);
            await kvStore.SaveAsync(expected3.Id, expected3);
            var actuals = await kvStore.ListAsync();
            actuals = actuals.ToArray();
            Assert.NotNull(actuals);
            Assert.Equal(3, actuals.Count());
            Assert.Equal(1, actuals.Count(d => d.Id == "abc"));
            Assert.Equal(1, actuals.Count(d => d.Id == "def"));
            Assert.Equal(1, actuals.Count(d => d.Id == "ghi"));
        }


        class TestKvDto
        {
            public string Id {get;set;}
            public string Name {get;set;}
        }

    }
}