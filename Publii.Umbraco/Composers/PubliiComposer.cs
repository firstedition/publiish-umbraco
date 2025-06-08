using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Publii.Umbraco.Controllers.Page;
using Publii.Umbraco.Extensions;
using Publii.Umbraco.Migrations;
using Publii.Umbraco.Models;
using Publii.Umbraco.Providers;
using Publii.Umbraco.Providers.Interfaces;
using Publii.Umbraco.Services;
using Publii.Umbraco.Services.Interfaces;
using Publii.Umbraco.Settings;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Publii.Umbraco.Composers;

public class PubliiComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		// config
		var publiiSection = builder.Config.GetSection("Publii");
		builder.Services.Configure<PubliiAppSettings>(publiiSection);

		// migrations
		builder.AddNotificationHandler<UmbracoApplicationStartingNotification, PublicationTableMigration>();
        
		// providers
		builder.Services.AddSingleton<IPublicationProvider, PublicationProvider>();
		builder.Services.AddSingleton<IUmbracoProvider, UmbracoProvider>();
		builder.Services.AddSingleton<IZipFileProcessingProvider, ZipFileProcessingProvider>();
        
		// services
		builder.Services.AddSingleton(typeof(ILoggingService<>), typeof(LoggingService<>));
		builder.Services.AddSingleton<IPublicationService, PublicationService>();
		builder.Services.AddSingleton<IPubliiMediaService, PubliiMediaService>();
		builder.Services.AddSingleton<IPubliiUrlService, PubliiUrlService>();

		// background service
		builder.Services.AddSingleton<IExtractQueueService, ExtractQueueServiceService>();
		builder.Services.AddHostedService<ExtractBackgroundService>();
		
		// media url
		ConfigureMediaUrl(builder, publiiSection);
	}
	
	private static void ConfigureMediaUrl(IUmbracoBuilder builder, IConfigurationSection? configSection)
    {
        var mediaUrlConfig = MediaUrlConfig.UseVirtualPageAndRouting;
        
        if (configSection != null)
        {
            var mediaUrlConfigValue = configSection[AppSettings.MediaUrlConfigName] ?? AppSettings.DefaultMediaUrlConfig.ToString();
            Enum.TryParse(mediaUrlConfigValue, out mediaUrlConfig);
        }

        switch (mediaUrlConfig)
        {
            case MediaUrlConfig.OverrideDefaultMediaUrlProvider:
                // media url provider
                builder.MediaUrlProviders().Replace<DefaultMediaUrlProvider, PubliiMediaUrlProvider>();
                break;
            
            // using virtual page routing also requires that static content routing is setup in Startup.cs
            // use StartupExtensions.AddVirtualPageStaticFileRouting(app)
            // must be placed before app.UseUmbraco() 
            case MediaUrlConfig.UseVirtualPageAndRouting:
                // dynamic route
                builder.Services.Configure<UmbracoPipelineOptions>(options =>
                {
                    options.AddFilter(new UmbracoPipelineFilter(nameof(PubliiVirtualPageController))
                    {
                        Endpoints = app => app.UseEndpoints(endpoints =>
                        {
                            ConfigureDynamicRoute(endpoints, configSection);
                        })
                    });
                });
                break;
        }
    }
    
    private static void ConfigureDynamicRoute(IEndpointRouteBuilder endpoints, IConfigurationSection? configSection)
    {
        const string controller = "PubliiVirtualPage";
        const string action = "Index";
        const string pathPlaceholder = "/{*path}";

        string rootPath;
        
        if (configSection == null)
        {
            rootPath = AppSettings.DefaultRootUrlSegment;
        }
        else
        {
            var rootUrlSegment = configSection[AppSettings.RootUrlSegmentName];

            if (!string.IsNullOrWhiteSpace(rootUrlSegment))
            {
                var newRootPath = rootUrlSegment.MakeValidUrlSegment();
                rootPath = !string.IsNullOrWhiteSpace(newRootPath)
                    ? newRootPath
                    : AppSettings.DefaultRootUrlSegment;
            }
            else
            {
                rootPath = AppSettings.DefaultRootUrlSegment;
            }
        }

        if (!string.IsNullOrWhiteSpace(rootPath))
            rootPath = $"/{rootPath}";

        var pattern = $"{rootPath}{pathPlaceholder}";

        // Configure the dynamic route with and without extension
        endpoints.MapControllerRoute(
            name: "DynamicVirtualPage",
            pattern: pattern,
            defaults: new { controller, action });
    }
}