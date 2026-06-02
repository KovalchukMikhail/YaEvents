using System;
using System.Collections.Generic;
using System.Text;

namespace YaEvents.IntegrationTests
{

    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DbWorker>
    {

    }
}
