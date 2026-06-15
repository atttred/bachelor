using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class SendQuoteEmailHandler : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = serviceProvider.Get<ITracingService>();
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var serviceFactory = serviceProvider.Get<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(null);

            try
            {
                EntityReference oppRef = null;

                if (context.InputParameters.Contains("entity"))
                {
                    oppRef = context.InputParameters["entity"] as EntityReference;
                }

                if (oppRef == null && context.InputParameters.Contains("Target"))
                {
                    oppRef = context.InputParameters["Target"] as EntityReference;
                }

                if (oppRef == null && context.PrimaryEntityId != Guid.Empty)
                {
                    oppRef = new EntityReference(VY_Opportunity.EntityLogicalName, context.PrimaryEntityId);
                }

                if (oppRef == null)
                {
                    throw new InvalidPluginExecutionException("Відсутнє посилання на угоду.");
                }

                string templateId = context.InputParameters.Contains("TemplateId")
                    ? context.InputParameters["TemplateId"] as string
                    : null;

                if (string.IsNullOrEmpty(templateId))
                {
                    throw new InvalidPluginExecutionException("TemplateId є обов'язковим.");
                }

                var opp = service.Retrieve(
                    oppRef.LogicalName, oppRef.Id,
                    new ColumnSet(
                        VY_Opportunity.Fields.VY_Name,
                        VY_Opportunity.Fields.VY_CustomerId,
                        VY_Opportunity.Fields.VY_EstimatedValue)).ToEntity<VY_Opportunity>();

                var email = new Entity("email");
                email["subject"] = $"Комерційна пропозиція для {opp.VY_Name}";
                email["description"] = $"Очікувана вартість: {opp.VY_EstimatedValue?.Value:N2}. Шаблон: {templateId}";
                email["regardingobjectid"] = oppRef;
                email["directioncode"] = true;

                var emailId = service.Create(email);
                tracer.Trace("Created email {0} for opportunity {1}", emailId, oppRef.Id);

                context.OutputParameters["EmailId"] = new EntityReference("email", emailId);
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
