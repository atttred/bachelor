using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class ActivityAutoNumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = serviceProvider.Get<ITracingService>();
            var context = serviceProvider.Get<IPluginExecutionContext>();

            if (context.InputParameters.Contains(Constants.Target) && context.InputParameters[Constants.Target] is Entity)
            {
                var serviceFactory = serviceProvider.Get<IOrganizationServiceFactory>();
                var service = serviceFactory.CreateOrganizationService(null);

                try
                {
                    var target = (Entity)context.InputParameters[Constants.Target];
                    var subject = target.GetAttributeValue<string>(VY_Activity.Fields.VY_Subject);

                    if (!string.IsNullOrWhiteSpace(subject))
                    {
                        return;
                    }

                    var query = new QueryExpression(VY_Activity.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(VY_Activity.Fields.VY_Subject),
                        TopCount = 1,
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression(VY_Activity.Fields.VY_Subject, ConditionOperator.Like, "ACT-%")
                            }
                        }
                    };
                    query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));

                    var last = service.RetrieveMultiple(query);
                    int nextNumber = 1;

                    if (last.Entities.Count > 0)
                    {
                        var lastSubject = last.Entities[0].GetAttributeValue<string>(VY_Activity.Fields.VY_Subject);
                        if (lastSubject != null && lastSubject.StartsWith("ACT-") &&
                            int.TryParse(lastSubject.Substring(4), out var n))
                        {
                            nextNumber = n + 1;
                        }
                    }

                    target[VY_Activity.Fields.VY_Subject] = $"ACT-{nextNumber:D5}";
                    tracer.Trace("Auto-assigned subject {0}", target[VY_Activity.Fields.VY_Subject]);
                }
                catch (Exception ex)
                {
                    tracer.Trace(ex.ToString());
                    throw new InvalidPluginExecutionException(Constants.UnexpectedErrorMessage);
                }
            }
        }
    }
}
