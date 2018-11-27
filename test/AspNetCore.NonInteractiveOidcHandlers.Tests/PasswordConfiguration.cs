using System;
using System.Threading.Tasks;
using AspNetCore.NonInteractiveOidcHandlers.Tests.Util;
using Microsoft.Extensions.DependencyInjection;
using NFluent;
using Xunit;

namespace AspNetCore.NonInteractiveOidcHandlers.Tests
{
	public class PasswordConfiguration
	{
		[Fact]
		public void Empty_options_should_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o => { }))
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act)
				.Throws<InvalidOperationException>()
				.WithMessage(GetExpectedValidationErrorMessage(
					"You must either set Authority or TokenEndpoint.",
					"You must set ClientId.",
					"You must set ClientSecret.",
					"You must set Scope.",
					"You must set UserCredentialsRetriever."));
		}

		[Fact]
		public void No_ClientId_should_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o => { o.Authority = "https://authority"; }))
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act)
				.Throws<InvalidOperationException>()
				.WithMessage(GetExpectedValidationErrorMessage(
					"You must set ClientId.",
					"You must set ClientSecret.",
					"You must set Scope.",
					"You must set UserCredentialsRetriever."));
		}

		[Fact]
		public void No_ClientSecret_should_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o =>
				{
					o.Authority = "https://authority";
					o.ClientId = "test-client";
				}))
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act)
				.Throws<InvalidOperationException>()
				.WithMessage(GetExpectedValidationErrorMessage(
					"You must set ClientSecret.",
					"You must set Scope.",
					"You must set UserCredentialsRetriever."));
		}

		[Fact]
		public void No_Scope_should_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o =>
				{
					o.Authority = "https://authority";
					o.ClientId = "test-client";
					o.ClientSecret = "test-client secret key";
				}))
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act)
				.Throws<InvalidOperationException>()
				.WithMessage(GetExpectedValidationErrorMessage(
					"You must set Scope.",
					"You must set UserCredentialsRetriever."));
		}

		[Fact]
		public void No_UserCredentialsRetriever_should_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o =>
				{
					o.Authority = "https://authority";
					o.ClientId = "test-client";
					o.ClientSecret = "test-client secret key";
					o.Scope = "downstream-api";
				}))
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act)
				.Throws<InvalidOperationException>()
				.WithMessage(GetExpectedValidationErrorMessage(
					"You must set UserCredentialsRetriever."));
		}

		[Fact]
		public void No_Authority_but_TokenEndpoint_should_not_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o =>
				{
					o.TokenEndpoint = "https://authority/connect/token";
					o.ClientId = "test-client";
					o.ClientSecret = "test-client secret key";
					o.Scope = "downstream-api";
					o.UserCredentialsRetriever = () => ("some-username", "some-password");
					o.AuthorityHttpClientAccessor = () => TokenEndpointHandler.ValidBearerToken("some-token", TimeSpan.MaxValue).AsHttpClient();
				}))
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act).Not.ThrowsAny();
		}

		[Fact]
		public void EnableCaching_but_no_caching_service_should_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o =>
				{
					o.Authority = "https://authority";
					o.ClientId = "test-client";
					o.ClientSecret = "test-client secret key";
					o.Scope = "downstream-api";
					o.UserCredentialsRetriever = () => ("some-username", "some-password");
					o.EnableCaching = true;
				}), addCaching: false)
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act)
				.Throws<InvalidOperationException>()
				.WithMessage("Caching is enabled, but no IDistributedCache is found in the services collection.");
		}

		[Fact]
		public void EnabledCaching_with_caching_service_should_not_throw()
		{
			Task Act() => HostFactory
				.CreateClient(b => b.AddOidcPassword(o =>
				{
					o.TokenEndpoint = "https://authority/connect/token";
					o.ClientId = "test-client";
					o.ClientSecret = "test-client secret key";
					o.Scope = "downstream-api";
					o.UserCredentialsRetriever = () => ("some-username", "some-password");
					o.AuthorityHttpClientAccessor = () => TokenEndpointHandler.ValidBearerToken("some-token", TimeSpan.MaxValue).AsHttpClient();
				}), addCaching: true)
				.GetAsync("https://default");

			Check.ThatAsyncCode(Act).Not.ThrowsAny();
		}

		private string GetExpectedValidationErrorMessage(params string[] validationErrors)
			=> $"Options are not valid:{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors)}";
	}
}
