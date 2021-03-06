// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.ProjectSystem.Query.ProjectModel.Implementation;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Query
{
    /// <summary>
    /// Holds the state we need to pass from producers of <see cref="UIPropertyValue"/> instances
    /// to other producers that will create the <see cref="UIPropertyValue"/>s' child entities.
    /// </summary>
    internal sealed class PropertyProviderState
    {
        public PropertyProviderState(IPropertyPageQueryCache cache, Rule containingRule, QueryProjectPropertiesContext context, string propertyName)
        {
            Cache = cache;
            ContainingRule = containingRule;
            Context = context;
            PropertyName = propertyName;
        }

        public IPropertyPageQueryCache Cache { get; }
        public Rule ContainingRule { get; }
        public QueryProjectPropertiesContext Context { get; }
        public string PropertyName { get; }
    }
}
