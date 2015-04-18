// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Tracing;

namespace Microsoft.AspNet.Mvc
{
    [EventSource(Name = "Microsoft.AspNet.Mvc")]
    public class MvcEventSource : EventSource
    {
        public static MvcEventSource Instance { get; } = new MvcEventSource();

        [Event(1, Message = "Starting Action '{0}' Id '{1}'", Opcode = EventOpcode.Start, Level = EventLevel.Informational)]
        public void ActionStarting(ActionDescriptor action)
        {
            WriteEvent(1, action.Id, action.DisplayName);
        }

        [Event(2, Message = "Finished Action '{0}' Id '{1}'", Opcode = EventOpcode.Stop, Level = EventLevel.Informational)]
        public void ActionFinished(ActionDescriptor action)
        {
            WriteEvent(2, action.Id, action.DisplayName);
        }
    }
}
