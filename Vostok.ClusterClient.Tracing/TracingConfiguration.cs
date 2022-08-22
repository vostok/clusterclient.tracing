using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Clusterclient.Tracing
{
    [PublicAPI]
    public class TracingConfiguration
    {
        public TracingConfiguration([NotNull] ITracer tracer)
        {
            Tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        [NotNull]
        public ITracer Tracer { get; }

        /// <summary>
        /// If set to a non-null value, an additional request transformation will be applied to given <see cref="Request"/> with current <see cref="TraceContext"/>.
        /// </summary>
        [CanBeNull]
        public Func<Request, TraceContext, Request> AdditionalRequestTransformation { get; set; }
        
        /// <summary>
        /// If set to a non-null value, an additional <see cref="Request"/> transformation will be applied to current <see cref="IHttpRequestSpanBuilder"/>.
        /// </summary>
        [CanBeNull]
        public Action<IHttpRequestSpanBuilder, Request> SetAdditionalRequestDetails { get; set; }
        
        /// <summary>
        /// If set to a non-null value, an additional <see cref="Response"/> transformation will be applied to current <see cref="IHttpRequestSpanBuilder"/>.
        /// </summary>
        [CanBeNull]
        public Action<IHttpRequestClientSpanBuilder, Response> SetAdditionalResponseDetails { get; set; }
        
        /// <summary>
        /// If set to a non-null value, an additional <see cref="Response"/> transformation will be applied to current <see cref="IHttpRequestSpanBuilder"/>.
        /// </summary>
        [CanBeNull]
        public Action<IHttpRequestClusterSpanBuilder, ClusterResult> SetAdditionalClusterResponseDetails { get; set; }
    }
}