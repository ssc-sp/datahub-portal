using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Maui.Uploader
{
    public class WebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get => "/"; set => throw new NotImplementedException(); }
        public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ApplicationName { get => App.Current.ClassId; set => throw new NotImplementedException(); }
        public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ContentRootPath { get => "/"; set => throw new NotImplementedException(); }
        public string EnvironmentName { get => "MAUI"; set => throw new NotImplementedException(); }
    }
}
