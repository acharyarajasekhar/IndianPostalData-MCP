// Copyright (c) 2026 Indian Postal Data MCP
// Unit tests for PostalDataService

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MCP.Core.Models;
using MCP.Server.Services;
using Xunit;

namespace MCP.Tests
{
    // Simple HttpMessageHandler that returns pre-configured responses based on request URI
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = _handler(request);
            return Task.FromResult(response);
        }
    }

    public class PostalDataServiceTests
    {
        private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

        private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            var messageHandler = new FakeHttpMessageHandler(handler);
            return new HttpClient(messageHandler)
            {
                BaseAddress = new Uri("https://api.postalpincode.in/")
            };
        }

        private static PostalApiResponse CreateSuccessResponse(string pincode = "110001")
        {
            return new PostalApiResponse
            {
                Message = "Success",
                Status = "Success",
                PostOffice = new List<PostOffice>
                {
                    new PostOffice
                    {
                        Name = "Test Office",
                        District = "Test District",
                        State = "Test State",
                        Pincode = pincode
                    }
                }
            };
        }

        [Fact]
        public async Task GetPincodeDataAsync_InvalidInput_ReturnsFail()
        {
            var service = new PostalDataService(CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)), CreateCache());

            var resultEmpty = await service.GetPincodeDataAsync(string.Empty);
            Assert.False(resultEmpty.Success);
            Assert.Equal("INVALID_INPUT", resultEmpty.ErrorCode);

            var resultBad = await service.GetPincodeDataAsync("123");
            Assert.False(resultBad.Success);
            Assert.Equal("INVALID_INPUT", resultBad.ErrorCode);
        }

        [Fact]
        public async Task GetPincodeDataAsync_HttpError_ReturnsApiUnavailable()
        {
            var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var service = new PostalDataService(client, CreateCache());

            var result = await service.GetPincodeDataAsync("560001");
            Assert.False(result.Success);
            Assert.Equal("API_UNAVAILABLE", result.ErrorCode);
        }

        [Fact]
        public async Task GetPincodeDataAsync_ApiError_ReturnsApiError()
        {
            var errorResponse = new PostalApiResponse
            {
                Message = "Invalid Pincode",
                Status = "Error",
                PostOffice = new()
            };
            var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[] { errorResponse })
            });
            var service = new PostalDataService(client, CreateCache());

            var result = await service.GetPincodeDataAsync("999999");
            Assert.False(result.Success);
            Assert.Equal("API_ERROR", result.ErrorCode);
        }

        [Fact]
        public async Task GetPincodeDataAsync_Success_CachesResult()
        {
            int callCount = 0;
            var client = CreateClient(_ =>
            {
                callCount++;
                var resp = CreateSuccessResponse();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[] { resp })
                };
            });
            var cache = CreateCache();
            var service = new PostalDataService(client, cache);

            var first = await service.GetPincodeDataAsync("110001");
            var second = await service.GetPincodeDataAsync("110001");

            Assert.True(first.Success);
            Assert.True(second.Success);
            // Underlying HTTP should have been called only once due to caching
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task GetPostOfficeByCityAsync_InvalidInput_ReturnsFail()
        {
            var service = new PostalDataService(CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)), CreateCache());
            var result = await service.GetPostOfficeByCityAsync(string.Empty);
            Assert.False(result.Success);
            Assert.Equal("INVALID_INPUT", result.ErrorCode);
        }

        [Fact]
        public async Task GetPostOfficeByCityAsync_Success_ReturnsData()
        {
            var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[] { CreateSuccessResponse() })
            });
            var service = new PostalDataService(client, CreateCache());

            var result = await service.GetPostOfficeByCityAsync("TestCity");
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Test Office", result.Data!.PostOffice[0].Name);
        }
    }
}
