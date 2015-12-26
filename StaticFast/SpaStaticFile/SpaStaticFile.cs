using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Microsoft.Owin;

namespace SpaStaticFile {
    using System.Net;
    using System.Reflection;

    public class SpaStaticFile : OwinMiddleware {
        private readonly IDictionary<string, byte[]> site = new Dictionary<string, byte[]>();
        private readonly SpaStaticFileOptions options;

        public SpaStaticFile(OwinMiddleware next, SpaStaticFileOptions options)
            : base(next) {
            this.options = options;
        }

        public static string AssemblyDirectory {
            get {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public async override Task Invoke(IOwinContext context) {
            var path = Uri.UnescapeDataString(context.Request.Uri.AbsolutePath);
            if (this.ShouldIgnoreRequest(path)) {
                await this.Next.Invoke(context);
                return;
            }

            byte[] page;
            try {
                page = this.options.ShouldCache ? this.GetCachedPage(path) : this.GetPage(path);
            } catch (FileNotFoundException) {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

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
            var pathString = PathString.FromUriComponent(path);
            foreach (var ignorePath in this.options.IgnorePaths) {
                if (pathString.StartsWithSegments(ignorePath)) {
                    return true;
                }
            }

            return false;
        }

        private byte[] GetPage(string path) {
            var file = Path.Combine(this.options.RootPath, path.Trim('/'));
            if (!string.IsNullOrWhiteSpace(this.options.ZipPath)) {
                var zipFile = Path.Combine(AssemblyDirectory, this.options.ZipPath);
                if(!File.Exists(zipFile)) {
                    throw new FileNotFoundException("Could not find site zip", zipFile);
                }

                var zip = new ZipArchive(File.OpenRead(zipFile), ZipArchiveMode.Read, false);
                var entry = zip.GetEntry(path.TrimStart('.').TrimStart('/'));
                if (entry == null) {
                    return this.GetDefaultFile(path);
                }

                using (var str = entry.Open()) {
                    return ReadFully(str);
                }
            }

            if (File.Exists(file)) {
                File.ReadAllBytes(file);
            }

            return this.GetDefaultFile(path);
        }

        private byte[] GetDefaultFile(string path) {
            var defaultFile = Path.Combine(this.options.RootPath, this.options.DefaultFile.Trim('/'));
            if (path != defaultFile) {
                return this.GetPage(defaultFile);
            }

            throw new FileNotFoundException("default file not found", defaultFile);
        }

        public static byte[] ReadFully(Stream input) {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream()) {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
