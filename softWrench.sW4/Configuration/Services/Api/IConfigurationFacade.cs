﻿using softWrench.sW4.Configuration.Definitions;
using cts.commons.simpleinjector;
using JetBrains.Annotations;

namespace softWrench.sW4.Configuration.Services.Api {

    public interface IConfigurationFacade : ISingletonComponent {

        [NotNull]
        T Lookup<T>([NotNull]string configKey);

        void Register([NotNull]string configKey, [NotNull]PropertyDefinition definition);

        void SetValue([NotNull]string configkey, [NotNull]object value);


    }
}