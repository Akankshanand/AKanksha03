// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Moq;

namespace Microsoft.VisualStudio.ProjectSystem.Logging
{
    internal static class IProjectLoggerFactory
    {
        public static IProjectLogger Create()
        {
            return Mock.Of<IProjectLogger>();
        }
    }
}
