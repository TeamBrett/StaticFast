namespace SpaStaticFile {
    public class SpaStaticFileOptions {
        public string DefaultFile { get; set; } = "index.html";
        public string RootPath { get; set; } = "./";
        public string IgnorePath { get; set; } = "/api";
        public bool ShouldCache { get; set; } = true;
    }
}