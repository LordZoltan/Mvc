// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Core.Test
{
    // A TokenProvider that can be passed to MoQ
    internal interface ITokenProvider : IAntiForgeryTokenValidator, IAntiForgeryTokenGenerator
    {
    }
}