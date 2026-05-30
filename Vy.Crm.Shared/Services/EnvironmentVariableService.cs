using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Vy.Crm.Shared.Services
{
    public class EnvironmentVariableService
    {
        private readonly IOrganizationService _service;

        public EnvironmentVariableService(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public string GetValue(string schemaName)
        {
            if (string.IsNullOrEmpty(schemaName)) return null;

            var q = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("value")
            };
            var link = q.AddLink("environmentvariabledefinition", "environmentvariabledefinitionid", "environmentvariabledefinitionid");
            link.LinkCriteria.AddCondition("schemaname", ConditionOperator.Equal, schemaName);
            var values = _service.RetrieveMultiple(q);
            if (values.Entities.Count > 0)
            {
                return values.Entities[0].GetAttributeValue<string>("value");
            }

            var d = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("defaultvalue")
            };
            d.Criteria.AddCondition("schemaname", ConditionOperator.Equal, schemaName);
            var defs = _service.RetrieveMultiple(d);
            return defs.Entities.Count > 0 ? defs.Entities[0].GetAttributeValue<string>("defaultvalue") : null;
        }
    }
}
