using System;
using Microsoft.Owin.Hosting;

using Owin;

using SpaStaticFile;

namespace StaticFast {
    class Program {
        static void Main(string[] args) {
            using (WebApp.Start<Startup>("http://localhost:7000")) {
                Console.WriteLine(args);
                Console.ReadLine();
            }
        }
    }

    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.UseSpaStaticFile(new SpaStaticFileOptions {
                // do we have to allow extensions or can we just strip out leading "../.."s and serve up anything on the file system?
                RootPath = "..\\..\\..\\..\\..\\Aklesia\\src\\WebApp\\app",
                ShouldCache = false
            });
        }
    }
}
