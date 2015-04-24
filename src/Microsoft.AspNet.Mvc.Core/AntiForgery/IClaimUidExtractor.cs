// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// This interface can extract unique identifers for a claims-based identity.
    /// </summary>
    public interface IClaimUidExtractor
    {
        /// <summary>
        /// Extracts claims Uid.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/>.</param>
        /// <returns>The claims uid.</returns>
        string ExtractClaimUid(ClaimsIdentity identity);
    }
}