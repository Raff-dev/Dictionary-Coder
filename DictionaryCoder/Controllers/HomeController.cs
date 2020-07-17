using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DictionaryCoder.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DictionaryCoder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult React()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Sources()
        {
            return View();
        }

        public HttpResponseMessage Upload()
        {
            var files = Request.Form.Files.ToArray();
            Console.WriteLine("FILEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEES");
            foreach (var file in files) Console.WriteLine(file.ToString());
            Console.WriteLine("FILEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEES");

            if (Request.Form.Files.Count > 0)
            {
                //IFormFileCollection files = context.Request.Form.Files;
                //foreach (IFormFile file in files)
                //{
                //    using (System.IO.Stream fsSource = file.OpenReadStream())
                //    {
                //        using (StreamWriter outputFile = new StreamWriter(Path.Combine("kek", ".txt")))
                //        {
                //            while (true)
                //            {
                //                byte[] arr = new byte[fsSource.Length];
                //                int bytesToRead = (int)fsSource.Length;
                //                if (fsSource.Read(arr, 0, bytesToRead) <= 0) break;
                //                foreach (var content in arr) Console.Write((char)content);
                //                //run coder here (outputFile/fssource)
                //            }
                //        }
                //    }
                //}
            }
            var stream = new MemoryStream();

            //Response.ContentType = "text/plain";
            //Response.WriteAsync("File uploaded successfully!");
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };
            //response.Content = new StringContent(token, Encoding.Unicode);
            return response;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
