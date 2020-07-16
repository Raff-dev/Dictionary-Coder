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

        [HttpPost("api/upload")]
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Form.Files.Count > 0)
            {
                IFormFileCollection files = context.Request.Form.Files;
                foreach (IFormFile file in files)
                {
                    using (System.IO.Stream fsSource = file.OpenReadStream())
                    {
                        using (StreamWriter outputFile = new StreamWriter(Path.Combine("kek", ".txt")))
                        {
                            while (true)
                            {
                                byte[] arr = new byte[fsSource.Length];
                                int bytesToRead = (int)fsSource.Length;
                                if (fsSource.Read(arr, 0, bytesToRead) <= 0) break;
                                foreach (var content in arr) Console.Write((char)content);
                                //run coder here (outputFile/fssource)
                            }
                        }
                    }
                }
            }
            context.Response.ContentType = "text/plain";
            context.Response.WriteAsync("File(s) uploaded successfully!");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
