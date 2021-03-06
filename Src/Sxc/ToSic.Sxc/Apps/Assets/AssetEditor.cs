﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using ToSic.Eav;
using ToSic.Eav.Logging;
using ToSic.Sxc.Blocks;
using ToSic.Sxc.Engines;
using ToSic.Sxc.Web;


namespace ToSic.Sxc.Apps.Assets
{
    internal class AssetEditor : HasLog
    {
        public const string TokenHtmlExtension = ".html";
        public const string DefaultTokenHtmlBody = @"<p>
    You successfully created your own template.
    Start editing it by hovering the ""Manage"" button and opening the ""Edit Template"" dialog.
</p>";


        public const string CshtmlExtension = ".cshtml";
        public const string CodeCshtmlExtension = ".code.cshtml";
        public const string CshtmlPrefix = "_";

        public const string DefaultCshtmlBody = @"@inherits ToSic.Sxc.Dnn.RazorComponent

<div @Edit.TagToolbar(Content)>
    Put your content here
</div>";

        public const string DefaultCodeCshtmlBody = @"@inherits ToSic.Sxc.Dnn.RazorComponentCode

@functions {
  public string Hello() {
    return ""Hello from inner code"";
  }
}

@helper ShowDiv(string message) {
  <div>@message</div>
}
";

        public const string CsExtension = ".cs";

        public const string CsApiFolder = "api";

        public const string CsApiTemplateControllerName = "PleaseRenameController";
        // copied from the razor tutorial
        public const string DefaultCsBody = @"using System.Web.Http;		// this enables [HttpGet] and [AllowAnonymous]
using DotNetNuke.Web.Api;	// this is to verify the AntiForgeryToken

[AllowAnonymous]			// define that all commands can be accessed without a login
[ValidateAntiForgeryToken]	// protects the API from users not on your site (CSRF protection)
// Inherit from ToSic...ApiController to get features like App, Data or Dnn - see https://r.2sxc.org/CustomWebApi
public class " + CsApiTemplateControllerName + @" : ToSic.Sxc.Dnn.ApiController
{

	[HttpGet]				// [HttpGet] says we're listening to GET requests
	public string Hello()
	{
		return ""Hello from the controller with ValidateAntiForgeryToken in /api"";
	}

}
";


        public AssetEditInfo EditInfo { get; }

        private readonly bool _userIsSuperUser;
        private readonly bool _userIsAdmin;


        private readonly IApp _app;

        public AssetEditor(IApp app, int templateId, bool isSuperUser, bool isAdmin, ILog parentLog)
            : base("Sxc.AstEdt", parentLog)
        {
            _app = app;
            _userIsSuperUser = isSuperUser;
            _userIsAdmin = isAdmin;

            // todo: 2dm Views - see if we can get logger to flow
            var template = new CmsRuntime(app, Log, true, false).Views.Get(templateId);
            EditInfo = TemplateAssetsInfo(template);
        }

        public AssetEditor(IApp app, string path, bool isSuperUser, bool isAdmin, bool global, ILog parentLog)
            : base("Sxc.AstEdt", parentLog)
        {
            _app = app;
            _userIsSuperUser = isSuperUser;
            _userIsAdmin = isAdmin;

            EditInfo = new AssetEditInfo(_app.AppId, _app.Name, path, global);
        }

        public AssetEditInfo EditInfoWithSource

        {
            get
            {
                EditInfo.Code = Source; // do this later, because it relies on the edit-info to exist
                return EditInfo;
            }
        }

        /// <summary>
        /// Check permissions and if not successful, give detailed explanation
        /// </summary>
        public void EnsureUserMayEditAssetOrThrow(string fullPath = null)
        {
            // check super user permissions - then all is allowed
            if (_userIsSuperUser)
                return;

            // ensure current user is admin - this is the minimum of not super-user
            if (!_userIsAdmin)
                throw new AccessViolationException("current user may not edit templates, requires admin rights");

            // if not super user, check if razor (not allowed; super user only)
            if (!EditInfo.IsSafe)
                throw new AccessViolationException("current user may not edit razor templates - requires super user");

            // if not super user, check if cross-portal storage (not allowed; super user only)
            if (EditInfo.LocationScope != Settings.TemplateLocations.PortalFileSystem)
                throw new AccessViolationException(
                    "current user may not edit templates in central storage - requires super user");

            // optionally check if the file is really in the portal
            if (fullPath == null) return;

            var path = new FileInfo(fullPath);
            if (path.Directory == null)
                throw new AccessViolationException("path is null");

            if (path.Directory.FullName.IndexOf(_app.PhysicalPath, StringComparison.InvariantCultureIgnoreCase) != 0)
                throw new AccessViolationException("current user may not edit files outside of the app-scope");
        }

        private AssetEditInfo TemplateAssetsInfo(IView view)
        {
            var t = new AssetEditInfo(_app.AppId, _app.Name, view.Path,
                view.Location == Settings.TemplateLocations.HostFileSystem)
            {
                // Template specific properties, not really available in other files
                LocationScope = view.Location,
                Type = view.Type,
                Name = view.Name,
                HasList = view.UseForList,
                TypeContent = view.ContentType,
                TypeContentPresentation = view.PresentationType,
                TypeList = view.HeaderType,
                TypeListPresentation = view.HeaderPresentationType
            };
            return t;
        }

        public string InternalPath => Factory.Resolve<IHttp>().MapPath( //  netPlumbing.HostingEnvironment_MapPath(
            Path.Combine(
                TemplateHelpers.GetTemplatePathRoot(EditInfo.LocationScope, _app),
                EditInfo.FileName));


        /// <summary>
        /// Read / Write the source code of the template file
        /// </summary>
        public string Source
        {
            get
            {
                EnsureUserMayEditAssetOrThrow(InternalPath);
                if (File.Exists(InternalPath))
                    return File.ReadAllText(InternalPath);

                throw new FileNotFoundException("could not find file"
                                                + (_userIsSuperUser
                                                    ? " for superuser - file tried '" + InternalPath + "'"
                                                    : "")
                );
            }
            set
            {
                EnsureUserMayEditAssetOrThrow(InternalPath);

                if (File.Exists(InternalPath))
                    File.WriteAllText(InternalPath, value);
                else
                    throw new FileNotFoundException("could not find file"
                                                    + (_userIsSuperUser
                                                        ? " for superuser - file tried '" + InternalPath + "'"
                                                        : "")
                    );

            }
        }

        public bool Create(string contents)
        {
            // todo: maybe add some security for special dangerous file names like .cs, etc.?
            EditInfo.FileName = Regex.Replace(EditInfo.FileName, @"[?:\/*""<>|]", "");
            var absolutePath = InternalPath;

            // don't create if it already exits
            if (File.Exists(absolutePath)) return false;

            // ensure the web.config exists (usually missing in the global area)
            new TemplateHelpers(_app).EnsureTemplateFolderExists(EditInfo.LocationScope);

            // check if the folder to it already exists, or create it...
            var foundFolder = absolutePath.LastIndexOf("\\", StringComparison.InvariantCulture);
            if (foundFolder > -1)
            {
                var folderPath = absolutePath.Substring(0, foundFolder);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            // now create the file
            var stream = new StreamWriter(File.Create(absolutePath));
            stream.Write(contents);
            stream.Flush();
            stream.Close();

            return true;
        }

    }
}