﻿using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Triggered.Internal
{
#if EFCORETRIGGERED2
    public sealed class TriggeredDbContextFactory<TContext, TFactory> : IDbContextFactory<TContext>
        where TContext : DbContext
        where TFactory : IDbContextFactory<TContext>
    {
        private readonly TFactory _contextFactory;
        private readonly IServiceProvider _serviceProvider;

        public TriggeredDbContextFactory(TFactory contextFactory, IServiceProvider serviceProvider)
        {
            _contextFactory = contextFactory;
            _serviceProvider = serviceProvider;
        }

        public TContext CreateDbContext()
        {
            var context = _contextFactory.CreateDbContext();
            Debug.Assert(context != null);

            var applicationTriggerServiceProviderAccessor = context.GetService<ApplicationTriggerServiceProviderAccessor>();
            if (applicationTriggerServiceProviderAccessor != null)
            {
                applicationTriggerServiceProviderAccessor.SetTriggerServiceProvider(new HybridServiceProvider(_serviceProvider, context));
            }

            return context;
        }
    }
#endif
}
