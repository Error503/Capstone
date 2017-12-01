using Cassandra;
using MediaGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaGraph.Code
{
    public class CassandraRequestsDriver : IDisposable
    {
#if DEBUG
        private const string kContactPoint = "automediadata.southcentralus.cloudapp.azure.com";
#else
        private const string kContactPoint = "10.0.0.5";
#endif

        private Cluster connection;
        private string keyspace;
        private string table;

        /*
         * Autocomplete Table Schema:
         * Prefix, Remaining, Id, Name, DataType, CommonName, PRIMARY KEY (Prefix, Remaining, Id)
         * 
         * Autocomplete Metadata Table Schema:
         * Id, PrefixValues (list), RemainingValues (list), PRIMARY KEY (Id)
         */

        public CassandraRequestsDriver()
        {
            keyspace = "requests_keyspace";
            table = "requets";
            connection = Cluster.Builder()
                .WithCredentials(System.Configuration.ConfigurationManager.AppSettings["cassandraUser"], System.Configuration.ConfigurationManager.AppSettings["cassandraPass"])
                .AddContactPoint(kContactPoint)
                .WithPort(9042).Build();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }

        public void Add(DatabaseRequest request)
        {
            throw new NotImplementedException();
        }

        public void Get(Guid requestId)
        {
            throw new NotImplementedException();
        }

        public void Search()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid requestId)
        {
            throw new NotImplementedException();
        }
    }
}