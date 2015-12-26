using Microsoft.Owin;

using System.Collections.Generic;

namespace SpaStaticFile {
    public class SpaStaticFileOptions {
        public string DefaultFile { get; set; } = "index.html";
        public string RootPath { get; set; } = "./";
        public HashSet<PathString> IgnorePaths = new HashSet<PathString> { PathString.FromUriComponent("/Api") };
        public bool ShouldCache { get; set; } = true;
        public string ZipPath { get; set; } = null;
    }
}