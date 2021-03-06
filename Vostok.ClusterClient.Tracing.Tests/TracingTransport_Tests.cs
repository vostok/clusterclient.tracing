﻿using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Tracing.Helpers;
using Vostok.Tracing.Abstractions;

namespace Vostok.Clusterclient.Tracing.Tests
{
    [TestFixture]
    internal class TracingTransport_Tests
    {
        private ITracer tracer;
        private ISpanBuilder spanBuilder;
        private ITransport baseTransport;

        private string targetService;
        private string targetEnvironment;
        private Request request;
        private Response response;

        private TracingTransport transport;

        [SetUp]
        public void TestSetup()
        {
            tracer = Substitute.For<ITracer>();
            tracer.CurrentContext.Returns(new TraceContext(Guid.NewGuid(), Guid.NewGuid()));
            tracer.BeginSpan().Returns(spanBuilder = Substitute.For<ISpanBuilder>());

            baseTransport = Substitute.For<ITransport>();
            baseTransport.SendAsync(default, default, default, default).ReturnsForAnyArgs(_ => response);

            transport = new TracingTransport(baseTransport, new TracingConfiguration(tracer))
            {
                TargetServiceProvider = () => targetService,
                TargetEnvironmentProvider = () => targetEnvironment
            };

            targetService = Guid.NewGuid().ToString();
            targetEnvironment = Guid.NewGuid().ToString();

            request = Request.Get("foo/bar");
            response = Responses.Ok;
        }

        [Test]
        public void Should_not_record_any_specific_annotations_when_trace_context_is_null_after_beginning_a_span()
        {
            tracer.CurrentContext.ReturnsNull();

            Run();

            spanBuilder.ReceivedCalls().Should().HaveCount(2);
        }

        [Test]
        public void Should_record_request_annotations()
        {
            Run();

            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Request.Method, request.Method);
            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Request.Url, request.Url.ToString());
            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Request.TargetService, targetService);
            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Request.TargetEnvironment, targetEnvironment);
        }

        [Test]
        public void Should_record_response_code_annotation()
        {
            Run();

            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Code, 200);
        }

        [Test]
        public void Should_record_response_size_annotation_for_content()
        {
            response = response.WithContent(new byte[123]);

            response = Run();

            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Size, 123L);
        }

        [Test]
        public void Should_record_response_size_annotation_for_stream()
        {
            response = response.WithStream(new MemoryStream(new byte[123]));

            response = Run();

            response.Stream.CopyTo(new MemoryStream());
            response.Dispose();

            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Size, 123L);
            spanBuilder.Received(1).Dispose();
        }

        [Test]
        public void Should_dispose_underlying_stream()
        {
            var stream = Substitute.For<Stream>();
            response = response.WithStream(stream);

            var result = Run();

            result.Dispose();

            stream.Received(1).Dispose();
        }

        [Test]
        public void Should_record_common_annotations()
        {
            Run();

            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Common.Kind, WellKnownSpanKinds.HttpRequest.Client);
            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Common.Component, Constants.Component);
            spanBuilder.Received(1).SetAnnotation(WellKnownAnnotations.Common.Operation, "GET: foo/bar");
        }

        private Response Run() => transport
            .SendAsync(request, null, 5.Seconds(), CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }
}