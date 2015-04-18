
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Extensions;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Notify;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MvcSample.Web
{
    public class ListenerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly INotifier _notifier;
        private readonly object _listener;

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            ContractResolver = new DeclaredOnlyContractResolver(),
        };

        private readonly ConcurrentDictionary<string, EventStore> _requestStore = new ConcurrentDictionary<string, EventStore>(StringComparer.Ordinal);

        public ListenerMiddleware(RequestDelegate next, INotifier notifier)
        {
            _next = next;
            _notifier = notifier;

            _listener = new Listener();
            _notifier.EnlistTarget(_listener);
        }

        public async Task Invoke(HttpContext context)
        {
            PathString remaining;
            if (context.Request.Path.StartsWithSegments(new PathString("/GetData"), out remaining))
            {
                var trackingId = remaining.Value.Substring(1);

                EventStore eventStore;
                if (_requestStore.TryRemove(trackingId, out eventStore))
                {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(eventStore, _settings));
                    return;
                }
                else
                {
                    context.Response.StatusCode = 404;
                    return;
                }
            }
            else
            {
                var trackingId = context.Request.Query.GetValues("trackingId")?.FirstOrDefault();
                if (trackingId != null)
                {
                    var eventStore = new EventStore();
                    context.SetFeature(eventStore);

                    _requestStore.TryAdd(trackingId, eventStore);
                }

                await _next(context);
            }
        }

        private class EventStore
        {
            public string Action { get; set; }

            public List<FilterResult> Filters { get; } = new List<FilterResult>();

            public IDictionary<string, object> RouteValues { get; set; }
        }

        private class FilterResult
        {
            public string Type { get; set; }

            public string FilterType { get; set; }

            public bool ShortCircuited { get; set; }
        }

        private class Listener
        {
            private HttpContextAccessor _accessor = new HttpContextAccessor();

            [NotificationName("Microsoft.AspNet.Mvc.ActionStarting")]
            public void OnActionStarted(IActionDescriptor actionDescriptor, IDictionary<string, object> routeValues)
            {
                var store = GetEventStore();
                if (store == null)
                {
                    return;
                }

                store.Action = actionDescriptor.DisplayName;
                store.RouteValues = routeValues;
            }

            [NotificationName("Microsoft.AspNet.Mvc.BeforeAsyncAuthorizationFilter")]
            public void BeforeAsyncAuthorizationFilter(object filter)
            {
            }

            [NotificationName("Microsoft.AspNet.Mvc.AfterAsyncAuthorizationFilter")]
            public void AfterAsyncAuthorizationFilter(object filter, IActionResult result)
            {
                var store = GetEventStore();
                if (store == null)
                {
                    return;
                }

                store.Filters.Add(new FilterResult()
                {
                    FilterType = "Authorization",
                    ShortCircuited = result == null,
                    Type = filter.GetType().FullName,
                });
            }

            [NotificationName("Microsoft.AspNet.Mvc.BeforeAuthorizationFilter")]
            public void BeforeAuthorizationFilter(object filter)
            {
            }

            [NotificationName("Microsoft.AspNet.Mvc.AfterAuthorizationFilter")]
            public void AfterAuthorizationFilter(object filter, IActionResult result)
            {
                var store = GetEventStore();
                if (store == null)
                {
                    return;
                }

                store.Filters.Add(new FilterResult()
                {
                    FilterType = "Authorization",
                    ShortCircuited = result == null,
                    Type = filter.GetType().FullName,
                });
            }

            private EventStore GetEventStore()
            {
                return _accessor.HttpContext?.GetFeature<EventStore>();
            }
        }
    }

    public interface IActionDescriptor
    {
        [JsonProperty]
        string DisplayName { get; }

        [JsonProperty]
        string Name { get; }

        string Id { get; }
    }

    public class DeclaredOnlyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization);
        }
    }
}
