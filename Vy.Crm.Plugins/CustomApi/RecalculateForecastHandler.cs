using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class RecalculateForecastHandler : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = serviceProvider.Get<ITracingService>();
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var serviceFactory = serviceProvider.Get<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(null);

            try
            {
                EntityReference ownerFilter = null;

                if (context.InputParameters.Contains("OwnerId"))
                {
                    ownerFilter = context.InputParameters["OwnerId"] as EntityReference;
                }

                var query = new QueryExpression(VY_Opportunity.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(
                        VY_Opportunity.Fields.VY_EstimatedValue,
                        VY_Opportunity.Fields.VY_Probability),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("statecode", ConditionOperator.Equal, (int)VY_Opportunity_StateCode.Active)
                        }
                    }
                };

                if (ownerFilter != null)
                {
                    query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, ownerFilter.Id);
                }

                decimal forecast = 0m;
                int counted = 0;
                var opps = service.RetrieveMultiple(query);

                foreach (var opp in opps.Entities)
                {
                    var money = opp.GetAttributeValue<Money>(VY_Opportunity.Fields.VY_EstimatedValue);

                    if (money == null)
                    {
                        continue;
                    }

                    var probability = opp.Contains(VY_Opportunity.Fields.VY_Probability)
                        ? Convert.ToInt32(opp[VY_Opportunity.Fields.VY_Probability])
                        : 100;
                    forecast += money.Value * probability / 100m;
                    counted++;
                }

                tracer.Trace("Forecast from {0} open opps: {1}", counted, forecast);

                context.OutputParameters["Forecast"] = new Money(forecast);
                context.OutputParameters["OpportunityCount"] = counted;
            }
            catch (Exception ex)
            {
                tracer.Trace(ex.ToString());
                throw new InvalidPluginExecutionException(Constants.UnexpectedErrorMessage);
            }
        }
    }
}
