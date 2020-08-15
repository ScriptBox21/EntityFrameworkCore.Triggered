﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.Triggered.Internal
{
    public class NonRecursiveTriggerContextDiscoveryStrategy : ITriggerContextDiscoveryStrategy
    {
        readonly static Action<ILogger, int, string, Exception?> _changesDetected = LoggerMessage.Define<int, string>(
            LogLevel.Information,
            new EventId(1, "Discovered"),
            "Discovered changes: {changes} for {name}");

        readonly string _name;

        public NonRecursiveTriggerContextDiscoveryStrategy(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public IEnumerable<IEnumerable<TriggerContextDescriptor>> Discover(TriggerOptions options, TriggerContextTracker tracker, ILogger logger)
        {
            var changes = tracker.DiscoveredChanges ?? throw new InvalidOperationException("Trigger discovery process has not yet started. Please ensure that TriggerSession.DiscoverChanges() or TriggerSession.RaiseBeforeSaveTriggers() has been called");

            _changesDetected(logger, changes.Count(), _name, null);

            return Enumerable.Repeat(changes, 1);
        }
    }
}
