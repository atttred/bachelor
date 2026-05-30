using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class EmailDuplicateGuard : IPlugin
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

                    if (!target.Contains(VY_Contact.Fields.VY_EmailAddress1))
                    {
                        return;
                    }

                    var email = target.GetAttributeValue<string>(VY_Contact.Fields.VY_EmailAddress1);

                    if (string.IsNullOrWhiteSpace(email))
                    {
                        return;
                    }

                    var query = new QueryExpression(VY_Contact.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(VY_Contact.Fields.VY_FullName),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression(VY_Contact.Fields.VY_EmailAddress1, ConditionOperator.Equal, email),
                                new ConditionExpression("statecode", ConditionOperator.Equal, (int)VY_Contact_StateCode.Active)
                            }
                        }
                    };

                    if (context.MessageName.ToLower() == Constants.Update && context.PrimaryEntityId != Guid.Empty)
                    {
                        query.Criteria.AddCondition(VY_Contact.PrimaryIdAttribute, ConditionOperator.NotEqual, context.PrimaryEntityId);
                    }

                    var matches = service.RetrieveMultiple(query);

                    if (matches.Entities.Count > 0)
                    {
                        var existing = matches.Entities[0].GetAttributeValue<string>(VY_Contact.Fields.VY_FullName);
                        throw new InvalidPluginExecutionException(
                            $"Contact with email '{email}' already exists ({existing}).");
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
