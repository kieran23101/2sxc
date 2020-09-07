﻿using System;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Caching;
using ToSic.Eav.ImportExport.Persistence.File;
using ToSic.Eav.LookUp;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Plumbing.Booting;
using ToSic.Eav.Repositories;
using ToSic.Eav.Run;
using ToSic.SexyContent.Dnn920;
using ToSic.Sxc.Adam;
using ToSic.Sxc.Apps.ImportExport;
using ToSic.Sxc.Code;
using ToSic.Sxc.Conversion;
using ToSic.Sxc.Dnn.Code;
using ToSic.Sxc.Dnn.ImportExport;
using ToSic.Sxc.Dnn.Install;
using ToSic.Sxc.Dnn.LookUp;
using ToSic.Sxc.Dnn.Run;
using ToSic.Sxc.Dnn.Web;
using ToSic.Sxc.Dnn.WebApi;
using ToSic.Sxc.Interfaces;
using ToSic.Sxc.Web;
using ToSic.Sxc.Polymorphism;
using ToSic.Sxc.Run;
using ToSic.Sxc.WebApi.Adam;

namespace ToSic.SexyContent
{
    /// <inheritdoc />
    /// <summary>
    /// this configures unity (the IoC container)
    /// Never call this directly! always go through Settings.Ensure...
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class ConfigurationLoader: IConfigurationLoader
    {
        private static bool _alreadyConfigured;

        /// <summary>
        /// Configure IoC for 2sxc. If it's already configured, do nothing.
        /// </summary>
        public void Configure()
        {
            if (_alreadyConfigured)
                return;

            ConfigureConnectionString();
            var appsCache = GetAppsCacheOverride();
            ConfigureIoC(appsCache);
            SharpZipLibRedirect.RegisterSharpZipLibRedirect();
            ConfigurePolymorphResolvers();
            _alreadyConfigured = true;
        }


        private void ConfigureConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString;
            Eav.Repository.Efc.Implementations.Configuration.SetConnectionString(connectionString);
            Eav.Repository.Efc.Implementations.Configuration.SetFeaturesHelpLink("https://2sxc.org/help?tag=features", "https://2sxc.org/r/f/");
        }

        /// <summary>
        /// Expects something like "ToSic.Sxc.Dnn.DnnAppsCacheFarm, ToSic.Sxc.Dnn.Enterprise" - namespaces + class, DLL name without extension
        /// </summary>
        /// <returns></returns>
        private string GetAppsCacheOverride()
        {
            var farmCacheName = ConfigurationManager.AppSettings["EavAppsCache"];
            if (string.IsNullOrWhiteSpace(farmCacheName)) return null;
            return farmCacheName;
        }

        /// <summary>
        /// Configure all the known polymorph resolvers
        /// </summary>
        private void ConfigurePolymorphResolvers()
        {
            Polymorphism.Add(new Koi());
            Polymorphism.Add(new Permissions());
        }

        private static void ConfigureIoC(string appsCacheOverride)
        {
            Eav.Factory.ActivateNetCoreDi(sc =>
            {
                sc.AddTransient<Eav.Conversion.EntitiesToDictionary, DataToDictionary>();
                sc.AddTransient<IValueConverter, DnnValueConverter>();
                sc.AddTransient<IUser, DnnUser>();

                sc.AddTransient<XmlExporter, DnnXmlExporter>();
                sc.AddTransient<IImportExportEnvironment, DnnImportExportEnvironment>();

                sc.AddTransient<IRuntime, Runtime>();

                // new for .net standard
                sc.AddScoped<ITenant, DnnTenant>();
                sc.AddTransient<IContainer, DnnContainer>();
                sc.AddScoped<IHttp, HttpAbstraction>();
                sc.AddTransient<IAppFileSystemLoader, DnnAppFileSystemLoader>();
                sc.AddTransient<IAppRepositoryLoader, DnnAppFileSystemLoader>();
                sc.AddScoped<IEnvironment, DnnEnvironment>();
                sc.AddScoped<IAppEnvironment, DnnEnvironment>();
                sc.AddTransient<IZoneMapper, DnnZoneMapper>();

                // The file-importer - temporarily itself
                sc.AddTransient<XmlImportWithFiles, XmlImportFull>();

                sc.AddTransient<IClientDependencyOptimizer, DnnClientDependencyOptimizer>();
                sc.AddTransient<AppPermissionCheck, DnnPermissionCheck>();

                sc.AddTransient<DynamicCodeRoot, DnnDynamicCode>();
                sc.AddTransient<IRenderingHelper, DnnRenderingHelper>();
                sc.AddTransient<IEnvironmentConnector, DnnMapAppToInstance>();
                sc.AddTransient<IEnvironmentInstaller, InstallationController>();
                sc.AddTransient<IEnvironmentFileSystem, DnnFileSystem>();
                // new #2160
                sc.AddTransient<SecurityChecksBase, DnnAdamSecurityChecks>();

                sc.AddTransient<IGetEngine, GetDnnEngine>();
                sc.AddTransient<IFingerprint, DnnFingerprint>();

                // add page publishing
                sc.AddTransient<IPagePublishing, Sxc.Dnn.Cms.PagePublishing>();

                if (appsCacheOverride != null)
                {
                    try
                    {
                        var appsCacheType = Type.GetType(appsCacheOverride);
                        sc.TryAddSingleton(typeof(IAppsCache), appsCacheType);
                    }
                    catch {  /* ignore */ }
                }

                new Eav.DependencyInjection().ConfigureNetCoreContainer(sc);
            });
        }
    }
}