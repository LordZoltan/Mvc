// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    public class ContentResult : ActionResult
    {
        private readonly MediaTypeHeaderValue DefaultContentType = new MediaTypeHeaderValue("text/plain")
        {
            Encoding = Encodings.UTF8EncodingWithoutBOM
        };

        public string Content { get; set; }

        public MediaTypeHeaderValue ContentType { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        public override async Task ExecuteResultAsync([NotNull] ActionContext context)
        {
            var response = context.HttpContext.Response;

            // Encoding property on MediaTypeHeaderValue does not return the exact encoding instance that
            // is set, so any settings(for example: BOM) on it will be lost when retrieving the value.
            // In the scenario where the user does not supply encoding, we want to use UTF8 without BOM and
            // for this reason do not rely on Encoding property.
            MediaTypeHeaderValue contentTypeHeader;
            Encoding encoding;
            if (ContentType == null)
            {
                encoding = Encodings.UTF8EncodingWithoutBOM;
                contentTypeHeader = DefaultContentType;
            }
            else
            {
                if (ContentType.Encoding == null)
                {
                    encoding = Encodings.UTF8EncodingWithoutBOM;
                    // 1. Do not modify the user supplied content type
                    // 2. Parse here to handle parameters apart from charset
                    contentTypeHeader = MediaTypeHeaderValue.Parse(ContentType.ToString());
                    contentTypeHeader.Encoding = encoding;
                }
                else
                {
                    encoding = ContentType.Encoding;
                    contentTypeHeader = ContentType;
                }
            }

            response.ContentType = contentTypeHeader.ToString();

            if (StatusCode != null)
            {
                response.StatusCode = StatusCode.Value;
            }

            if (Content != null)
            {
                await response.WriteAsync(Content, encoding);
            }
        }
    }
}
