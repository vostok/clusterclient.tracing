using System.IO;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Tracing.Helpers;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Clusterclient.Tracing.Tests.Helpers
{
    [TestFixture]
    internal class TracingExtensions_Tests
    {
        private IHttpRequestSpanBuilder builder;
        private Request request;
        private Response response;

        [SetUp]
        public void TestSetup()
        {
            builder = Substitute.For<IHttpRequestSpanBuilder>();
            request = Request.Get("foo/bar");
            response = Responses.Ok;
        }

        [Test]
        public void SetRequestDetails_should_record_request_url_and_method()
        {
            builder.SetRequestDetails(request);

            builder.Received(1).SetRequestDetails(request.Url, request.Method, null);
        }

        [Test]
        public void SetRequestDetails_should_respect_buffer_content_length()
        {
            request = request.WithContent(new byte[10]);

            builder.SetRequestDetails(request);

            builder.Received(1).SetRequestDetails(request.Url, request.Method, 10L);
        }

        [Test]
        public void SetRequestDetails_should_respect_composite_content_length()
        {
            request = request.WithContent(new [] {new byte[10], new byte[11]});

            builder.SetRequestDetails(request);

            builder.Received(1).SetRequestDetails(request.Url, request.Method, 21L);
        }

        [Test]
        public void SetRequestDetails_should_respect_stream_content_length()
        {
            request = request.WithContent(new StreamContent(Stream.Null, 31));

            builder.SetRequestDetails(request);

            builder.Received(1).SetRequestDetails(request.Url, request.Method, 31L);
        }

        [Test]
        public void SetResponseDetails_should_record_response_code()
        {
            builder.SetResponseDetails(response);

            builder.Received(1).SetResponseDetails(200, null);
        }

        [Test]
        public void SetResponseDetails_should_respect_response_content_length()
        {
            response = response.WithContent(new byte[123]);

            builder.SetResponseDetails(response);

            builder.Received(1).SetResponseDetails(200, 123L);
        }

        [Test]
        public void SetResponseDetails_should_respect_stream_content_after_response_dispose_if_read()
        {
            response = response.WithStream(new MemoryStream(new byte[123]));

            response = builder.SetResponseDetails(response);

            builder.Received(1).SetAnnotation(Constants.StreamingAnnotation, true);
            builder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Code, (int)response.Code);
            
            response.Stream.CopyTo(new MemoryStream());

            response.Dispose();

            builder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Size, 123L);
        }

        [Test]
        public void SetResponseDetails_should_respect_stream_content_after_response_dispose_if_not_read()
        {
            response = response.WithStream(new MemoryStream(new byte[123]));

            response = builder.SetResponseDetails(response);

            builder.Received(1).SetAnnotation(Constants.StreamingAnnotation, true);
            builder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Code, (int)response.Code);

            response.Dispose();

            builder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Size, null);
        }

        [Test]
        public void SetResponseDetails_should_dispose_underlying_stream()
        {
            var stream = Substitute.For<Stream>();
            response = response.WithStream(stream);

            response = builder.SetResponseDetails(response);

            builder.Received(1).SetAnnotation(Constants.StreamingAnnotation, true);
            builder.Received(1).SetAnnotation(WellKnownAnnotations.Http.Response.Code, (int)response.Code);

            response.Dispose();

            stream.Received(1).Dispose();
        }
    }
}