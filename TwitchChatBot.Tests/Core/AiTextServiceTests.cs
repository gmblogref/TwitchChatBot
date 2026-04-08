using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;
using System.Net.Http;
using System.Text;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Models;
using Xunit;

namespace TwitchChatBot.Tests.Core
{
	public class AiTextServiceTests
	{
		private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
		private readonly FakeHttpMessageHandler _handler = new();

		private readonly AiTextService _sut;

		public AiTextServiceTests()
		{
			AppSettings.Configuration = CreateConfiguration();

			var httpClient = new HttpClient(_handler)
			{
				BaseAddress = new Uri("https://example.test/")
			};

			_httpClientFactoryMock
				.Setup(x => x.CreateClient("openai"))
				.Returns(httpClient);

			_sut = new AiTextService(_httpClientFactoryMock.Object);
		}

		private IConfiguration CreateConfiguration()
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["OpenAI:BaseUrl"] = "https://example.test/",
					["OpenAI:ApiKey"] = "test-key",
					["OpenAI:Model"] = "gpt-test",
					["OpenAI:DefaultAlertTone"] = "hype",
					["OpenAI:DefaultAlertMaxWords"] = "8",
					["Ads:DefaultUserName"] = "TestUser"
				})
				.Build();
		}

		private static AlertContext RaidContext()
		{
			return new AlertContext
			{
				AiType = AlertAiType.Raid,
				Username = "Tyler",
				ViewerCount = 42,
				MaxWords = 8
			};
		}

		// =========================
		// SUCCESS
		// =========================

		[Fact]
		public async Task GenerateAlertLineAsync_Should_ReturnText_When_AiResponds()
		{
			_handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(
					"""{"output":[{"content":[{"type":"output_text","text":"Welcome in Tyler and crew right now"}]}]}""",
					Encoding.UTF8,
					"application/json")
			};

			var result = await _sut.GenerateAlertLineAsync(RaidContext());

			result.Should().Be("Welcome in Tyler and crew right now");
		}

		// =========================
		// EDGE CASES
		// =========================

		[Fact]
		public async Task GenerateAlertLineAsync_Should_ReturnNull_When_ResponseIsEmpty()
		{
			_handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(
					"""{"output":[{"content":[{"type":"output_text","text":""}]}]}""",
					Encoding.UTF8,
					"application/json")
			};

			var result = await _sut.GenerateAlertLineAsync(RaidContext());

			result.Should().BeNull();
		}

		[Fact]
		public async Task GenerateAlertLineAsync_Should_ReturnNull_When_ServiceThrows()
		{
			_handler.ExceptionToThrow = new HttpRequestException("boom");

			var result = await _sut.GenerateAlertLineAsync(RaidContext());

			result.Should().BeNull();
		}

		[Fact]
		public async Task GenerateAlertLineAsync_Should_ReturnNull_When_ResponseIsNotSuccessful()
		{
			_handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);

			var result = await _sut.GenerateAlertLineAsync(RaidContext());

			result.Should().BeNull();
		}

		// =========================
		// REQUEST VALIDATION
		// =========================

		[Fact]
		public async Task GenerateAlertLineAsync_Should_BuildCorrectPrompt()
		{
			_handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(
					"""{"output":[{"content":[{"type":"output_text","text":"Raid message"}]}]}""",
					Encoding.UTF8,
					"application/json")
			};

			await _sut.GenerateAlertLineAsync(RaidContext());

			_handler.LastRequestBody.Should().Contain("Raider: Tyler");
			_handler.LastRequestBody.Should().Contain("Viewers: 42");
			_handler.LastRequestBody.Should().Contain("\"model\":\"gpt-test\"");
		}

		[Fact]
		public async Task GenerateAlertLineAsync_Should_CallHttpClientOnce()
		{
			_handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(
					"""{"output":[{"content":[{"type":"output_text","text":"Raid message"}]}]}""",
					Encoding.UTF8,
					"application/json")
			};

			await _sut.GenerateAlertLineAsync(RaidContext());

			_httpClientFactoryMock.Verify(x => x.CreateClient("openai"), Times.Once);
			_handler.CallCount.Should().Be(1);
		}

		// =========================
		// FAKE HANDLER
		// =========================

		private sealed class FakeHttpMessageHandler : HttpMessageHandler
		{
			public int CallCount { get; private set; }
			public string LastRequestBody { get; private set; } = string.Empty;
			public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }
			public Exception? ExceptionToThrow { get; set; }

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				CallCount++;

				LastRequestBody = request.Content == null
					? string.Empty
					: await request.Content.ReadAsStringAsync(cancellationToken);

				if (ExceptionToThrow != null)
				{
					throw ExceptionToThrow;
				}

				return ResponseFactory!(request);
			}
		}
	}
}