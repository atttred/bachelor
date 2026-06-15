using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class OpportunityStageGuard : IPlugin
    {
        private const string _preImageEntity = "OpportunityStageGuardPreImage";

        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = serviceProvider.Get<ITracingService>();
            var context = serviceProvider.Get<IPluginExecutionContext>();

            if (context.InputParameters.Contains(Constants.Target) && context.InputParameters[Constants.Target] is Entity)
            {
                try
                {
                    var target = (Entity)context.InputParameters[Constants.Target];

                    if (!target.Contains(VY_Opportunity.Fields.VY_SalesStageCode))
                    {
                        return;
                    }

                    if (!context.PreEntityImages.Contains(_preImageEntity))
                    {
                        return;
                    }

                    var preImage = context.PreEntityImages[_preImageEntity];
                    var previousStage = preImage.GetAttributeValue<OptionSetValue>(VY_Opportunity.Fields.VY_SalesStageCode);
                    var newStage = target.GetAttributeValue<OptionSetValue>(VY_Opportunity.Fields.VY_SalesStageCode);

                    if (previousStage == null || newStage == null)
                    {
                        return;
                    }

                    if (newStage.Value < previousStage.Value)
                    {
                        throw new InvalidPluginExecutionException(
                            "Етап продажу не може повертатися назад. Натомість позначте угоду як «Втрачено».");
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
