﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ToSic.Sxc.Blocks;
using ToSic.Sxc.Mvc;
using ToSic.Sxc.Mvc.Dev;
using ToSic.Sxc.Mvc.Engines;
using ToSic.Sxc.Mvc.RazorPages;
using ToSic.Sxc.Mvc.Web;

namespace Website.Pages.RenderPartial
{
    public class Render2Model : PageModel
    {
        private readonly IRenderRazor _renderer;
        private readonly SxcMvc _sxcMvc;
        public Render2Model(IRenderRazor renderer, SxcMvc sxcMvc)
        {
            _renderer = renderer;
            _sxcMvc = sxcMvc;
        }

        public async Task OnGetAsync()
        {
            var dynCode = _sxcMvc.CreateDynCode(TestIds.Blog, null);

            var path = $"/{MvcConstants.WwwRoot}2sxc/Blog App/_1 Main blog view.cshtml";

            InnerRender = await _renderer.RenderToStringAsync(
                path,
                new ContactForm
                {
                    Email = "something@somewhere",
                    Message = "Hello message!",
                    Name = "The Dude",
                    Priority = Priority.Medium,
                    Subject = "This is the subject"
                }, rzv =>
                {
                    if (rzv.RazorPage is IIsSxcRazorPage asSxc)
                    {
                        asSxc.DynCode = dynCode;
                        asSxc.VirtualPath = path;
                        asSxc.Purpose = Purpose.WebView;
                    }
                });
        }


        public string InnerRender;
    }
    
}