﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using IdentityServer4.Models;
using Xunit;

namespace IdentityServer4.Cassandra.Tests.IntegrationTests
{
    [Trait("Category","Integration")]
    public class PersistedGrantStoreTests : IDisposable
    {
        private readonly ISession _session;
        private static readonly string Keyspace =  typeof(PersistedGrantStoreTests).Name.ToLower();

        public PersistedGrantStoreTests()
        {

            _session = Cluster.Builder().AddContactPoint("localhost").Build().Connect();
            _session.Execute(
                $"CREATE KEYSPACE IF NOT EXISTS {Keyspace} WITH REPLICATION = {{ 'class' : 'SimpleStrategy', 'replication_factor' : 1 }};");
            _session.ChangeKeyspace(Keyspace);
        }


        public void Dispose()
        {
            _session.Execute($"DROP KEYSPACE {Keyspace};");
        }

        [Fact]
        public async Task StoresThenRetrievesByKey()
        {
            var stores = new CassandraIdentityServerStores(_session);
            var grantsStore = await  stores.InitializeGrantsStoreAsync();
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "123"});
            var storedGrant = await grantsStore.GetAsync("123");
            Assert.NotNull(storedGrant);
            Assert.Equal("123", storedGrant.Key);
        }

        [Fact]
        public async Task StoresThenDeletesByKey()
        {
            var stores = new CassandraIdentityServerStores(_session);
            var grantsStore = await  stores.InitializeGrantsStoreAsync();
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "999"});
            var storedGrant = await grantsStore.GetAsync("999");
            Assert.NotNull(storedGrant);
            Assert.Equal("999", storedGrant.Key);
        }

        [Fact]
        public async Task StoresThenFetchesBySubject()
        {
            var stores = new CassandraIdentityServerStores(_session);
            var grantsStore = await  stores.InitializeGrantsStoreAsync();
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "321", SubjectId = "Some App"});
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "456", SubjectId = "Some App"});
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "789", SubjectId = "Some Other App"});
            var storedGrants = await grantsStore.GetAllAsync("Some App");
            Assert.Equal(2, storedGrants.Count());
        }

        [Fact]
        public async Task StoresThenRemovesBySubjectAndClient()
        {
            var stores = new CassandraIdentityServerStores(_session);
            var grantsStore = await  stores.InitializeGrantsStoreAsync();
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "666", SubjectId = "Some App", ClientId = "mt"});
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "111", SubjectId = "Some App", ClientId = "jp"});
            await grantsStore.RemoveAllAsync("Some App", "mt");
            var storedGrants = await grantsStore.GetAllAsync("Some App");
            storedGrants = storedGrants.ToArray();
            Assert.Equal(1, storedGrants.Count());
            Assert.Equal("jp", storedGrants.Single().ClientId);
        }

        [Fact]
        public async Task StoresThenRemovesBySubjectAndClientAndType()
        {
            var stores = new CassandraIdentityServerStores(_session);
            var grantsStore = await  stores.InitializeGrantsStoreAsync();
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "666", SubjectId = "Some App", ClientId = "mt", Type = "User"});
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "111", SubjectId = "Some App", ClientId = "mt", Type = "Admin"});
            await grantsStore.StoreAsync(new PersistedGrant() {Key = "456", SubjectId = "Some App", ClientId = "jp", Type = "User"});
            await grantsStore.RemoveAllAsync("Some App", "mt", "User");
            var storedGrants = await grantsStore.GetAllAsync("Some App");
            storedGrants = storedGrants.ToArray();
            Assert.Equal(2, storedGrants.Count());
        }
    }
}