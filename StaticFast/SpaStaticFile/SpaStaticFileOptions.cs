using Microsoft.Owin;

namespace SpaStaticFile {
    public class SpaStaticFileOptions {
        public string DefaultFile { get; set; } = "index.html";
        public string RootPath { get; set; } = "./";
        public PathString IgnorePath = PathString.FromUriComponent("/Api");
        public bool ShouldCache { get; set; } = true;
    }
}