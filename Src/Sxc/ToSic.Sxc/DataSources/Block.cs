﻿using System;
using ToSic.Eav;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.LookUp;
using ToSic.Sxc.Blocks;
using ToSic.Sxc.Compatibility;
using ToSic.Sxc.Data;

namespace ToSic.Sxc.DataSources
{
    /// <summary>
    /// The main data source for Blocks. Internally often uses <see cref="CmsBlock"/> to find what it should provide.
    /// It's based on the <see cref="PassThrough"/> data source, because it's just a coordination-wrapper.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public class Block : PassThrough, IBlockDataSource
    {
        [PrivateApi]
        public override string LogId => "Sxc.BlckDs";

        [PrivateApi("older use case, probably don't publish")]
        public DataPublishing Publish { get; }= new DataPublishing();

        [Obsolete]
        [PrivateApi]
        public CacheWithGetContentType Cache 
            => _cache ?? (_cache = new CacheWithGetContentType(Eav.Apps.State.Get(this)));
        [Obsolete]
        private CacheWithGetContentType _cache;

        
        [PrivateApi]
        internal static IBlockDataSource GetBlockDataSource(IBlock block, IView view, ILookUpEngine configurationProvider, ILog parentLog)
        {
            var log = new Log("DS.CreateV", parentLog, "will create view data source");
            var showDrafts = block.EditAllowed;

            log.Add($"mid#{block.Context.Container.Id}, draft:{showDrafts}, template:{view?.Name}");
            // Get ModuleDataSource
            var dsFactory = new DataSource(log);
            //var block = builder.Block;
            var initialSource = dsFactory.GetPublishing(block, showDrafts, configurationProvider);
            var moduleDataSource = dsFactory.GetDataSource<CmsBlock>(initialSource);
            //moduleDataSource.InstanceId = instanceId;

            moduleDataSource.OverrideView = view;
            moduleDataSource.UseSxcInstanceContentGroup = true;

            // If the Template has a Data-Pipeline, use an empty upstream, else use the ModuleDataSource created above
            var viewDataSourceUpstream = view?.Query == null
                ? moduleDataSource
                : null;
            log.Add($"use pipeline upstream:{viewDataSourceUpstream != null}");

            var viewDataSource = dsFactory.GetDataSource<Block>(block, viewDataSourceUpstream, configurationProvider);

            // Take Publish-Properties from the View-Template
            if (view != null)
            {
                viewDataSource.Publish.Enabled = view.PublishData;
                viewDataSource.Publish.Streams = view.StreamsToPublish;

                log.Add($"use template, & pipe#{view.Query?.Id}");
                // Append Streams of the Data-Pipeline (this doesn't require a change of the viewDataSource itself)
                if (view.Query != null)
                {
                    log.Add("Generate query");
                    var query = new Query(block.App.ZoneId, block.App.AppId, view.Query.Entity, configurationProvider, showDrafts, viewDataSource, parentLog);
                    log.Add("attaching");
                    viewDataSource.Out = query.Out;
                }
            }
            else
                log.Add("no template override");

            return viewDataSource;
        }
    }
}