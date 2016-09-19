// Please visit http://go.microsoft.com/fwlink/?LinkID=761099&clcid=0x409 for more information on settting up Github Webhooks
module.exports = function (context, data) {
    context.log('Issue: ', data.issue.number);
    
    var issue = data.issue;
    var labels = [];
    if (issue.labels) {
        issue.labels.forEach(function (x) {
            labels.push(x.name);
        });
    }
    
    var milestoneTitle = "";
    var milestoneNumber = 0;
    if (issue.milestone) {
        milestoneTitle = issue.milestone.title;
        milestoneNumber = issue.milestone.number;
    }
    
    var assignee = null;
    if (issue.assignee) {
        assignee = issue.assignee.login;
    }
    
    context.bindings.output = {
        "number": issue.number,
        "title": issue.title,
        "milestoneTitle": milestoneTitle,
        "milestoneNumber": milestoneNumber,
        "assignee": assignee,
        "state": issue.state,
        "repoFullName": data.repository.full_name,
        "labels": labels,
        "updatedAt": issue.updated_at
    };
    
    context.done();
};
