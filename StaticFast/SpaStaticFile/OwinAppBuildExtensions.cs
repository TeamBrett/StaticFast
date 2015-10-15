using Owin;

namespace SpaStaticFile {
    public static class OwinAppBuildExtensions {
        public static void UseSpaStaticFile(this IAppBuilder app, SpaStaticFileOptions options) {
            app.Use(typeof(SpaStaticFile), options);
        }
    }
}