﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.Triggered.Internal
{
    public class RecursiveTriggerContextDiscoveryStrategy : ITriggerContextDiscoveryStrategy
    {
        readonly static Action<ILogger, string, int, Exception?> _discoveryStarted = LoggerMessage.Define<string, int>(
            LogLevel.Debug,
            new EventId(1, "Discovered"),
            "Starting trigger discovery for {name} with a max recursion of {maxRecursion}");

        readonly static Action<ILogger, int, string, int, int, Exception?> _changesDetected = LoggerMessage.Define<int, string, int, int>(
            LogLevel.Information,
            new EventId(1, "Discovered"),
            "Discovered changes: {changes} for {name}. Iteration ({iteration}/{maxRecursion})");

        readonly string _name;
        readonly bool _skipDetectedChanges;

        public RecursiveTriggerContextDiscoveryStrategy(string name, bool skipDetectedChanges)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _skipDetectedChanges = skipDetectedChanges;
        }

        public IEnumerable<ITriggerContextDescriptor> Discover(TriggerOptions options, TriggerContextTracker tracker, ILogger logger)
        {
            var maxRecursion = options.MaxRecursion;
            _discoveryStarted(logger, _name, maxRecursion, null);

            var iteration = 0;
            while (true)
            {
                if (iteration > maxRecursion)
                {
                    throw new InvalidOperationException("MaxRecursion was reached");
                }

                IEnumerable<ITriggerContextDescriptor> changes = tracker.DiscoverChanges().ToList();
                    
                // In case someone made a call to TriggerSession.DetectChanges, prior to calling RaiseBeforeSaveTriggers, we want to make sure that we include that discovery result in the first iteration
                if (iteration == 0 && !_skipDetectedChanges)
                {
                    changes = tracker.DiscoveredChanges.ToList();
                }

                if (changes.Any())
                {
                    _changesDetected(logger, changes.Count(), _name, iteration, maxRecursion, null);
                     
                    foreach (var change in changes)
                    {
                        yield return change;
                    }
                }
                else
                {
                    break;
                }

                iteration++;
            }
        }
    }
}
