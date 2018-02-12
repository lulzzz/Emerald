﻿using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Jobs;
using System;

namespace Emerald.AspNetCore.Common
{
    public sealed class EmeraldOptions
    {
        private readonly EnvironmentConfigurationSection _environment;
        private readonly EmeraldSystemBuilder _emeraldSystemBuilder;

        internal EmeraldOptions(EnvironmentConfigurationSection environment, EmeraldSystemBuilder emeraldSystemBuilder)
        {
            _environment = environment;
            _emeraldSystemBuilder = emeraldSystemBuilder;
        }

        public void AddCommandHandler<T>() where T : CommandHandler
        {
            _emeraldSystemBuilder.AddCommandHandler<T>();
        }

        public void AddJob<T>() where T : class, IJob
        {
            _emeraldSystemBuilder.AddJob<T>(_environment.Jobs[typeof(T).Name]);
        }

        public void UseQueue(Action<QueueOptions> configure)
        {
            _emeraldSystemBuilder.UseQueue(_environment.Queue.ConnectionString, _environment.Queue.Interval, cfg => configure(new QueueOptions(cfg)));
        }
    }
}