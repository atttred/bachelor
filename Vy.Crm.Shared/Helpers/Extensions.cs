using System;
using Microsoft.Xrm.Sdk;

namespace Vy.Crm.Shared.Helpers
{
    public static class EntityExtensions
    {
        public static Entity GetMergedEntity(this Entity target, IPluginExecutionContext context, string imageName, bool preferPostImage = false)
        {
            if (target == null) return null;
            if (context == null) return target;

            Entity image = null;
            if (!string.IsNullOrEmpty(imageName))
            {
                if (preferPostImage && context.PostEntityImages.Contains(imageName))
                {
                    image = context.PostEntityImages[imageName];
                }
                else if (context.PreEntityImages.Contains(imageName))
                {
                    image = context.PreEntityImages[imageName];
                }
            }
            if (image == null) return target;

            var merged = new Entity(target.LogicalName) { Id = target.Id != Guid.Empty ? target.Id : image.Id };
            foreach (var kv in image.Attributes) merged[kv.Key] = kv.Value;
            foreach (var kv in target.Attributes) merged[kv.Key] = kv.Value;
            return merged;
        }
    }
}
