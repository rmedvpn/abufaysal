// Helpers/Database.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Faysal.Helpers
{
    public static class Database
    {
        static IConfiguration _config = null!;
        public static void Configure(IConfiguration cfg) => _config = cfg;
        public static DbWrapper Open(string connName)
        {
            var cs = _config.GetConnectionString(connName)
                     ?? throw new InvalidOperationException($"Missing connection-string '{connName}'");
            return new DbWrapper(cs);
        }
    }

    public sealed class DbWrapper : IDisposable
    {
        readonly IDbConnection _cn;
        public DbWrapper(string connectionString)
        {
            _cn = new SqlConnection(connectionString);
            _cn.Open();
        }

        // Build a Dapper parameter bag out of your args[] => @0,@1,...
        DynamicParameters BuildParams(object[] args)
        {
            var dp = new DynamicParameters();
            for (int i = 0; i < args.Length; i++)
                dp.Add(i.ToString(), args[i]);
            return dp;
        }

        // --- GENERIC strongly-typed methods (no change to your calls) ---
        public T QuerySingle<T>(string sql, params object[] args)
            => _cn.QuerySingle<T>(sql, BuildParams(args));

        public T QuerySingleOrDefault<T>(string sql, params object[] args)
            => _cn.QuerySingleOrDefault<T>(sql, BuildParams(args));

        public IEnumerable<T> Query<T>(string sql, params object[] args)
            => _cn.Query<T>(sql, BuildParams(args));

        public int Execute(string sql, params object[] args)
            => _cn.Execute(sql, BuildParams(args));


        // --- dynamic (case-insensitive) methods for your old dynamic queries ---
        private static dynamic Wrap(dynamic row)
        {
            if (row == null) return null!;
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in (IEnumerable<KeyValuePair<string, object?>>)row)
                dict[kv.Key] = kv.Value;
            return new CiDynamic(dict);
        }

        public dynamic QuerySingle(string sql, params object[] args)
            => Wrap(_cn.QuerySingle<dynamic>(sql, BuildParams(args)));

        public dynamic QuerySingleOrDefault(string sql, params object[] args)
            => Wrap(_cn.QuerySingleOrDefault<dynamic>(sql, BuildParams(args)));

        public IEnumerable<dynamic> Query(string sql, params object[] args)
        {
            foreach (var row in _cn.Query<dynamic>(sql, BuildParams(args)))
                yield return Wrap(row);
        }

        public void Close() => _cn.Close();
        public void Dispose() => Close();
    }


    /// <summary>
    /// A tiny DynamicObject that looks up your column-names ignoring case.
    /// </summary>
    public sealed class CiDynamic : DynamicObject
    {
        readonly IDictionary<string, object?> _d;
        public CiDynamic(IDictionary<string, object?> d) => _d = d;

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (_d.TryGetValue(binder.Name, out result)) return true;
            foreach (var kv in _d)
                if (string.Equals(kv.Key, binder.Name, StringComparison.OrdinalIgnoreCase))
                {
                    result = kv.Value;
                    return true;
                }
            result = null;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames() => _d.Keys;
    }
}
