using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.Collections.Generic;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class OpportunityWinProbability : IPlugin
    {
        private const string _postImageEntity = "OpportunityWinProbabilityPostImage";

        private static readonly Dictionary<VY_Opportunity_VY_SalesStageCode, int> Probability =
            new Dictionary<VY_Opportunity_VY_SalesStageCode, int>
            {
                { VY_Opportunity_VY_SalesStageCode.Prospecting, 10 },
                { VY_Opportunity_VY_SalesStageCode.Qualified,   25 },
                { VY_Opportunity_VY_SalesStageCode.Proposal,    50 },
                { VY_Opportunity_VY_SalesStageCode.Negotiation, 75 },
                { VY_Opportunity_VY_SalesStageCode.Closing,     90 }
            };

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
                    var entity = context.PostEntityImages.Contains(_postImageEntity)
                        ? context.PostEntityImages[_postImageEntity]
                        : (Entity)context.InputParameters[Constants.Target];
                    var opp = entity.ToEntity<VY_Opportunity>();

                    if (opp.VY_SalesStageCode == null)
                    {
                        return;
                    }

                    if (!Probability.TryGetValue(opp.VY_SalesStageCode.Value, out var probability))
                    {
                        tracer.Trace("Stage {0} has no mapped probability", opp.VY_SalesStageCode);
                        return;
                    }

                    var update = new Entity(VY_Opportunity.EntityLogicalName, context.PrimaryEntityId);
                    update[VY_Opportunity.Fields.VY_Probability] = probability;
                    service.Update(update);
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
