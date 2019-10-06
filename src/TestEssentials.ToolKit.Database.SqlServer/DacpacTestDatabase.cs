﻿using Microsoft.SqlServer.Dac;
using System;

namespace TestEssentials.ToolKit.Database.SqlServer
{
    public class DacpacTestDatabase : TestDatabase
    {
        private readonly DacpacDatabaseOptions _databaseOptions;
        private Action<DacpacDatabaseOptions> _configurationDatabaseOptions;

        #region ctor
        public DacpacTestDatabase(string connectionString)
            :base(connectionString)
        {
            _databaseOptions = new DacpacDatabaseOptions();
        }
        #endregion

        public ITestDatabase ConfigureOptions(Action<DacpacDatabaseOptions> databaseOptionBuilder)
        {
            _configurationDatabaseOptions += databaseOptionBuilder;
            return this;
        }

        public override ITestDatabase Build()
        {
            _configurationDatabaseOptions?.Invoke(_databaseOptions);

            if (!_databaseOptions.AlwayCreate && CheckDatabaseExists())
                return this;

            Drop();

            var _deployOptions = new DacDeployOptions();

            foreach (var variable in _databaseOptions.DeployVariables)
            {
                _deployOptions.SqlCommandVariableValues.Add(variable.Key, variable.Value);
            }
            var dacpacPath = _databaseOptions.DacpacPath;
            var instance = new DacServices(_dbBuilder.ConnectionString);
            _deployOptions.AllowIncompatiblePlatform = _databaseOptions.AllowIncompatiblePlatform;
            using (var dacpac = DacPackage.Load(dacpacPath))
            {
                instance.Deploy(dacpac, _dbBuilder.InitialCatalog, upgradeExisting: true, options: _deployOptions);
            }
            return this;
        }
    }
}
