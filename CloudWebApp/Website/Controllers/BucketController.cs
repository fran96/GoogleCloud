using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
    public class BucketController : Controller
    { // GET: Bucket
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(HttpPostedFileBase data)
        {
            try
            {
                var myStorageManager = StorageClient.Create();

                var guid = Guid.NewGuid();
                var mynewobj = myStorageManager.UploadObject("programmingcloud", guid.ToString() + Path.GetExtension(data.FileName), data.ContentType, data.InputStream,
                    options: new UploadObjectOptions() { PredefinedAcl = PredefinedObjectAcl.PublicRead });
               

                ViewBag.Success = "Item uploaded successfully. " + "<a href=\"\\Bucket\\Download?id=" + guid.ToString() + Path.GetExtension(data.FileName) + "\">Download</a>";
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex + " -(Item was not uploaded)";
            }

            return View();
        }

        public ActionResult Download(string id)
        {
            var myStorageManager = StorageClient.Create();
            MemoryStream ms = new MemoryStream();
            myStorageManager.DownloadObject("programmingcloud", id, ms);

            return File(ms.ToArray(), System.Net.Mime.MediaTypeNames.Application.Octet, id);
        }
    }
}