using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBugs.Models
{
    public sealed class ListModel
    {
        public QueryModel QueryModel { get; }
        public List<RoachIssue> Issues { get; }

        public ListModel(QueryModel queryModel, List<RoachIssue> issues)
        {
            QueryModel = queryModel;
            Issues = issues;
        }
    }
}