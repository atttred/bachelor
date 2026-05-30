using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using Vy.Crm.Shared.Helpers;
using Vy.Crm.Shared.Proxy;

namespace Vy.Crm.Plugins
{
    public class AccountContactCascade : IPlugin
    {
        private const string _postImageEntity = "AccountContactCascadePostImage";

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

                    if (!target.Contains(VY_Account.Fields.VY_EmailAddress1))
                    {
                        return;
                    }

                    var entity = context.PostEntityImages.Contains(_postImageEntity)
                        ? context.PostEntityImages[_postImageEntity]
                        : target;
                    var newEmail = entity.GetAttributeValue<string>(VY_Account.Fields.VY_EmailAddress1);

                    var query = new QueryExpression(VY_Contact.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(VY_Contact.PrimaryIdAttribute, VY_Contact.Fields.VY_EmailAddress1),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression(VY_Contact.Fields.VY_ParentCustomerId, ConditionOperator.Equal, context.PrimaryEntityId)
                            }
                        }
                    };

                    var contacts = service.RetrieveMultiple(query);
                    tracer.Trace("Cascading email to {0} child contacts", contacts.Entities.Count);

                    foreach (var existingContact in contacts.Entities)
                    {
                        var existingEmail = existingContact.GetAttributeValue<string>(VY_Contact.Fields.VY_EmailAddress1);

                        if (!string.IsNullOrWhiteSpace(existingEmail))
                        {
                            continue;
                        }

                        var update = new Entity(VY_Contact.EntityLogicalName, existingContact.Id);
                        update[VY_Contact.Fields.VY_EmailAddress1] = newEmail;
                        service.Update(update);
                    }
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
