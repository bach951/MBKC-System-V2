﻿using MBKC.DAL.DBContext;
using MBKC.DAL.RedisModels;
using Microsoft.Extensions.Configuration;
using Redis.OM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBKC.DAL.Infrastructures
{
    public class DbFactory : Disposable, IDbFactory
    {
        private MBKCDbContext _dbContext;
        private RedisConnectionProvider _redisConnectionProvider;
        public DbFactory()
        {

        }

        public MBKCDbContext InitDbContext()
        {
            if (_dbContext == null)
            {
                _dbContext = new MBKCDbContext();
            }
            return _dbContext;
        }

        public async Task<RedisConnectionProvider> InitRedisConnectionProvider()
        {
            if (this._redisConnectionProvider == null)
            {
                var builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                IConfigurationRoot configuration = builder.Build();
                this._redisConnectionProvider = new RedisConnectionProvider(configuration.GetConnectionString("RedisDbStore"));
                await this._redisConnectionProvider.Connection.CreateIndexAsync(typeof(AccountTokenRedisModel));
                await this._redisConnectionProvider.Connection.CreateIndexAsync(typeof(EmailVerificationRedisModel));
            }
            return this._redisConnectionProvider;
        }

        protected override void DisposeCore()
        {
            if (this._dbContext != null)
            {
                this._dbContext.Dispose();
            }
        }

    }
}
