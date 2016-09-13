using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs.Storage
{
    public sealed class RoachStatusEntity : TableEntity
    {
        public DateTime LastBulkUpdateRaw { get; set; }

        public string OwnerName => PartitionKey;
        public string RepoName => RowKey;
        public DateTimeOffset? LastBulkUpdate
        {
            get { return LastBulkUpdateRaw == default(DateTime) ? (DateTimeOffset?)null : LastBulkUpdateRaw; }
            set { LastBulkUpdateRaw = value.Value != null ? value.Value.UtcDateTime : default(DateTime); }
        }

        public RoachStatusEntity()
        {

        }

        public RoachStatusEntity(RoachRepoId repoId, DateTimeOffset lastBulkUpdate)
        {
            this.SetEntityKey(GetEntityKey(repoId));
            LastBulkUpdate = LastBulkUpdate;
        }

        public static EntityKey GetEntityKey(RoachRepoId id) => new EntityKey(id.Owner, id.Name);
    }
}
