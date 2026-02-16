using AwesomeAssertions;
using IsoBoiler.HTTP.Authentication;
using IsoBoiler.Logging;
using IsoBoiler.Json;
using IsoBoiler.Testing;
using IsoBoiler.Testing.HTTP;
using IsoBoiler.UnitTests.Helpers.HTTP.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace IsoBoiler.UnitTests
{
    public static class AuthenticationTests
    {
        #region AuthTokenHandler
        [Fact]
        public static async Task HttpClientUsingAuthTokenHandler_WhenCalledMultipleTimesRapidly_OnlyMintsOneToken()
        {
            //Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI""
                }
            }";
            var dotFoodsOktaToken = new DefaultOktaToken() { access_token = "token", expires_in = 3600, scope = "machine", token_type = "Bearer" };
            var oktaHttpClientMock = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK, dotFoodsOktaToken.ToJson());
            var always200HttpClient = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK);

            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();

                //We have to replace this call so we can use the oktaHttpClientMock in it:
                //services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
                //START
                services.Configure<AuthSettings<DefaultOktaToken>>(context.Configuration.GetSection("MyAppSettingsFilter"));
                services.AddHttpClient(typeof(DefaultOktaToken).Name, client =>
                {
                    client.BaseAddress = new Uri("https://localhost");
                })
                .ConfigurePrimaryHttpMessageHandler(() => oktaHttpClientMock.GetMessageHandler());
                services.AddScoped<ITokenProvider<DefaultOktaToken>, TokenProvider<DefaultOktaToken>>();
                services.AddScoped(typeof(AuthTokenHandler<DefaultOktaToken>));
                //END

                services.AddScoped<IExampleHttpService, ExampleHttpService>();
                services.AddHttpClient("MyHttpClientName", client =>
                {
                    client.BaseAddress = new Uri("https://localhost");
                })
                .ConfigurePrimaryHttpMessageHandler(() => always200HttpClient.GetMessageHandler())
                .UseDefaultOktaTokenProvider();
            });

            var testableExampleHttpService = serviceProvider.GetRequiredService<IExampleHttpService>();

            //Act
            for (int i = 0; i < 5; i++)
            {
                var getResult = await testableExampleHttpService.GetRequest();
                var postResult = await testableExampleHttpService.PostRequest();
            }

            //Assert
            oktaHttpClientMock.VerifySendAsync(Times.Once());
        }
        #endregion AuthTokenHandler

        #region TokenProvider
        [Fact]
        public static async Task TokenProvider_WhenTwoTokenProvidersPresent_HandlesTokensSeparately()
        {
            //Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI""
                }
            }";
            var dotFoodsOktaToken = new DefaultOktaToken() { access_token = "token", expires_in = 3600, scope = "machine", token_type = "Bearer" };
            var otherAuthToken = new ExampleToken() { Access_token = "OTHER_TOKEN", Expires_in = 3600, Scope = "machine", Token_type = "Bearer" };
            var oktaHttpClientMock = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK, dotFoodsOktaToken.ToJson());
            var otherHttpClientMock = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK, otherAuthToken.ToJson());
            var always200HttpClient_okta = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK);
            var always200HttpClient_other = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK);

            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();

                //SETUP OKTA
                services.Configure<AuthSettings<DefaultOktaToken>>(context.Configuration.GetSection("MyAppSettingsFilter"));
                services.AddHttpClient(typeof(DefaultOktaToken).Name, client =>
                {
                    client.BaseAddress = new Uri("https://localhost");
                })
                .ConfigurePrimaryHttpMessageHandler(() => oktaHttpClientMock.GetMessageHandler());
                services.AddScoped<ITokenProvider<DefaultOktaToken>, TokenProvider<DefaultOktaToken>>();
                services.AddScoped(typeof(AuthTokenHandler<DefaultOktaToken>));

                services.AddScoped<IExampleHttpService, ExampleHttpService>();
                services.AddHttpClient("MyHttpClientName", client =>
                {
                    client.BaseAddress = new Uri("https://localhost");
                })
                .ConfigurePrimaryHttpMessageHandler(() => always200HttpClient_okta.GetMessageHandler())
                .UseDefaultOktaTokenProvider();

                //SETUP OTHER
                services.Configure<AuthSettings<ExampleToken>>(context.Configuration.GetSection("MyAppSettingsFilter"));
                services.AddHttpClient(typeof(ExampleToken).Name, client =>
                {
                    client.BaseAddress = new Uri("https://localhost");
                })
                .ConfigurePrimaryHttpMessageHandler(() => otherHttpClientMock.GetMessageHandler());
                services.AddScoped<ITokenProvider<ExampleToken>, TokenProvider<ExampleToken>>();
                services.AddScoped(typeof(AuthTokenHandler<ExampleToken>));

                services.AddScoped<IExampleHttpService_Other, ExampleHttpService_Other>();
                services.AddHttpClient("MyHttpClientName_Other", client =>
                {
                    client.BaseAddress = new Uri("https://localhost");
                })
                .ConfigurePrimaryHttpMessageHandler(() => always200HttpClient_other.GetMessageHandler())
                .UseTokenProvider<ExampleToken>();

            });

            var testableExampleHttpService = serviceProvider.GetRequiredService<IExampleHttpService>();
            var otherTestableExampleHttpService = serviceProvider.GetRequiredService<IExampleHttpService_Other>();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

            //Act
            for (int i = 0; i < 5; i++)
            {
                var getResult = await testableExampleHttpService.GetRequest();
                var postResult = await testableExampleHttpService.PostRequest();
                var otherGetResult = await otherTestableExampleHttpService.GetRequest();
                var otherPostResult = await otherTestableExampleHttpService.PostRequest();
            }

            //Assert
            var oktaToken = memoryCache.Get(typeof(DefaultOktaToken).Name);
            var otherToken = memoryCache.Get(typeof(ExampleToken).Name);

            oktaHttpClientMock.VerifySendAsync(Times.Once());
            otherHttpClientMock.VerifySendAsync(Times.Once());

            oktaToken.Should().BeEquivalentTo(dotFoodsOktaToken.access_token);
            otherToken.Should().BeEquivalentTo(otherAuthToken.Access_token);
        }
        #endregion TokenProvider

        #region AuthSettings
        [Fact]
        public static void AuthSettings_WhenConfigurationHasAllRequiredValues_DoesNotThrowError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI""
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().NotThrow();
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationIsMissingClientID_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI""
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "The ClientID field is required."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationIsMissingClientSecret_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""URI"": ""URI""
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "The ClientSecret field is required."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationIsMissingURI_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret""
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "The URI field is required."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationIsHasEmptyGrantType_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI"",
                ""GrantType"": """"
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "GrantType cannot be null, empty, or whitespace."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationIsHasWhiteSpaceGrantType_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI"",
                ""GrantType"": ""     ""
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "GrantType cannot be null, empty, or whitespace."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationIsHasNullGrantType_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI"",
                ""GrantType"": null
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "GrantType cannot be null, empty, or whitespace."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationHasEmptyScope_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI"",
                ""Scope"": """"
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "Scope cannot be null, empty, or whitespace."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationHasWhiteSpaceScope_ThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI"",
                ""Scope"": ""     ""
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "Scope cannot be null, empty, or whitespace."
                                                                                    ]
                                                                                    """);
        }

        [Fact]
        public static void TokenProviderConstructor_WhenConfigurationHasNullScope_DoesNotThrowsCorrectError()
        {
            // Arrange
            var jsonConfig = @"{
                ""MyAppSettingsFilter"": {
                ""ClientID"": ""ClientID"",
                ""ClientSecret"": ""ClientSecret"",
                ""URI"": ""URI"",
                ""Scope"": null
                }
            }";

            //Act
            var serviceProvider = ConfigHelper.UseJsonAsConfiguration(jsonConfig).GetServiceProvider((context, services) =>
            {
                services.AddILog();
                services.AddMemoryCache();
                services.AddDefaultOktaTokenProvider(context.Configuration.GetSection("MyAppSettingsFilter"));
            });
            var runTokenProviderConstructor = () => serviceProvider.GetRequiredService<ITokenProvider<DefaultOktaToken>>();

            //Assert
            runTokenProviderConstructor.Should().Throw<ValidationException>().WithMessage("""
                                                                                    [
                                                                                      "Scope cannot be null, empty, or whitespace."
                                                                                    ]
                                                                                    """);
        }

        #endregion AuthSettings
    }
}
