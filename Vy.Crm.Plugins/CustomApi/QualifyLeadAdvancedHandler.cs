using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class QualifyLeadAdvancedHandler : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = serviceProvider.Get<ITracingService>();
            var context = serviceProvider.Get<IPluginExecutionContext>();
            var serviceFactory = serviceProvider.Get<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(null);

            try
            {
                EntityReference leadRef = null;

                if (context.InputParameters.Contains("entity"))
                {
                    leadRef = context.InputParameters["entity"] as EntityReference;
                }

                if (leadRef == null && context.InputParameters.Contains("Target"))
                {
                    leadRef = context.InputParameters["Target"] as EntityReference;
                }

                if (leadRef == null && context.PrimaryEntityId != Guid.Empty)
                {
                    leadRef = new EntityReference(VY_Lead.EntityLogicalName, context.PrimaryEntityId);
                }

                if (leadRef == null)
                {
                    throw new InvalidPluginExecutionException("Lead reference missing. Call this API from a Lead record.");
                }

                tracer.Trace("Qualifying lead {0}", leadRef.Id);
                var lead = service.Retrieve(leadRef.LogicalName, leadRef.Id, new ColumnSet(true)).ToEntity<VY_Lead>();

                Guid accountId = Guid.Empty;

                if (!string.IsNullOrWhiteSpace(lead.VY_CompanyName))
                {
                    var account = new VY_Account
                    {
                        VY_Name = lead.VY_CompanyName,
                        VY_CustomerTypeCode = VY_Account_VY_CustomerTypeCode.Customer
                    };

                    if (!string.IsNullOrWhiteSpace(lead.VY_EmailAddress1))
                    {
                        account.VY_EmailAddress1 = lead.VY_EmailAddress1;
                    }

                    if (!string.IsNullOrWhiteSpace(lead.VY_Telephone1))
                    {
                        account.VY_Telephone1 = lead.VY_Telephone1;
                    }

                    accountId = service.Create(account);
                    tracer.Trace("Created account {0}", accountId);
                }

                Guid contactId = Guid.Empty;

                if (!string.IsNullOrWhiteSpace(lead.VY_FullName))
                {
                    var contact = new VY_Contact
                    {
                        VY_FullName      = lead.VY_FullName,
                        VY_EmailAddress1 = lead.VY_EmailAddress1
                    };

                    if (accountId != Guid.Empty)
                    {
                        contact.VY_ParentCustomerId = new EntityReference(VY_Account.EntityLogicalName, accountId);
                    }

                    contactId = service.Create(contact);
                    tracer.Trace("Created contact {0}", contactId);
                }

                Guid oppId = Guid.Empty;

                if (accountId != Guid.Empty || contactId != Guid.Empty)
                {
                    var opp = new VY_Opportunity
                    {
                        VY_Name             = $"Opportunity from {lead.VY_Topic ?? lead.VY_FullName ?? "lead"}",
                        VY_EstimatedValue   = new Money(0m),
                        VY_SalesStageCode   = VY_Opportunity_VY_SalesStageCode.Prospecting
                    };
                    opp.VY_CustomerId = accountId != Guid.Empty
                        ? new EntityReference(VY_Account.EntityLogicalName, accountId)
                        : new EntityReference(VY_Contact.EntityLogicalName, contactId);
                    oppId = service.Create(opp);
                    tracer.Trace("Created opportunity {0}", oppId);
                }

                service.Update(new Entity(VY_Lead.EntityLogicalName, leadRef.Id)
                {
                    ["statecode"]  = new OptionSetValue(1),
                    ["statuscode"] = new OptionSetValue(2)
                });

                context.OutputParameters["AccountId"]     = accountId;
                context.OutputParameters["ContactId"]     = contactId;
                context.OutputParameters["OpportunityId"] = oppId;
                context.OutputParameters["Success"]       = true;
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracer.Trace(ex.ToString());
                throw new InvalidPluginExecutionException($"Qualify Lead Advanced failed: {ex.Message}");
            }
        }
    }
}
