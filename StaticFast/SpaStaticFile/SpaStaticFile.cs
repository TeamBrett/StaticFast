using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Owin;

namespace SpaStaticFile {
    public class SpaStaticFile : OwinMiddleware {
        private readonly IDictionary<string, byte[]> site = new Dictionary<string, byte[]>();
        private readonly SpaStaticFileOptions options;

        public SpaStaticFile(OwinMiddleware next, SpaStaticFileOptions options)
            : base(next) {
            this.options = options;
        }

        public async override Task Invoke(IOwinContext context) {
            var path = Uri.UnescapeDataString(context.Request.Uri.AbsolutePath);
            if (this.ShouldIgnoreRequest(path)) {
                await this.Next.Invoke(context);
                return;
            }

            var page = this.options.ShouldCache ? this.GetCachedPage(path) : this.GetPage(path);

            if (path.EndsWith(".css")) {
                context.Response.ContentType = "text/css";
            } else if (path.EndsWith(".js")) {
                context.Response.ContentType = "applicaiton/javascript";
            } else if (path.EndsWith(".png")) {
                context.Response.ContentType = "image";
            } else {
                context.Response.ContentType = "text/html";
            }

            await context.Response.Body.WriteAsync(page, 0, page.Length);
        }

        private byte[] GetCachedPage(string path) {
            byte[] page;
            if (this.site.TryGetValue(path, out page)) {
                return page;
            }

            page = this.GetPage(path);

            this.site.Add(path, page);

            return page;
        }

        private bool ShouldIgnoreRequest(string path) {
            return PathString.FromUriComponent(path).StartsWithSegments(this.options.IgnorePath);
        }

        private byte[] GetPage(string path) {
            var file = Path.Combine(this.options.RootPath, path.Trim('/'));
            return File.Exists(file) ? File.ReadAllBytes(file) : this.GetDefaultFile(path);
        }

        private byte[] GetDefaultFile(string path) {
            return File.ReadAllBytes(Path.Combine(this.options.RootPath, this.options.DefaultFile.Trim('/')));
        }
    }
}
