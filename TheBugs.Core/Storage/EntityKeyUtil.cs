using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs.Storage
{
    public static class EntityKeyUtil
    {
        public static string ToKey(RoachRepoId id)
        {
            return $"{id.Owner}-{id.Name}";
        }

        public static RoachRepoId ParseRoachRepoIdKey(string key)
        {
            var parts = key.Split('-');
            return new RoachRepoId(parts[0], parts[1]);
        }
    }
}
