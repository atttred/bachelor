using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class CheckOpportunityUniqueness : IPlugin
    {
        private const string _preImageEntity = "CheckOpportunityUniquenessPreImage";

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
                    var merged = target.GetMergedEntity(context, _preImageEntity);
                    var opp = merged.ToEntity<VY_Opportunity>();

                    if (opp.VY_CustomerId == null || opp.VY_EstimatedValue == null)
                    {
                        return;
                    }

                    var query = new QueryExpression(VY_Opportunity.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(VY_Opportunity.Fields.VY_Name),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression(VY_Opportunity.Fields.VY_CustomerId, ConditionOperator.Equal, opp.VY_CustomerId.Id),
                                new ConditionExpression(VY_Opportunity.Fields.VY_EstimatedValue, ConditionOperator.Equal, opp.VY_EstimatedValue.Value),
                                new ConditionExpression("statecode", ConditionOperator.Equal, (int)VY_Opportunity_StateCode.Active)
                            }
                        }
                    };

                    if (context.MessageName.ToLower() == Constants.Update && context.PrimaryEntityId != Guid.Empty)
                    {
                        query.Criteria.AddCondition(VY_Opportunity.PrimaryIdAttribute, ConditionOperator.NotEqual, context.PrimaryEntityId);
                    }

                    if (service.RetrieveMultiple(query).Entities.Count > 0)
                    {
                        throw new InvalidPluginExecutionException(
                            "Активна угода для цього клієнта з такою ж очікуваною вартістю вже існує.");
                    }
                }
                catch (InvalidPluginExecutionException)
                {
                    throw;
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
