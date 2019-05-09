using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common;
using DataAccess;
using Npgsql;
using Google.Cloud.Storage.V1;
using System.IO;
using Newtonsoft.Json;
using Website.Models;
using System.Text;

namespace Website.Controllers
{
    public class PropertyController : Controller
    {

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase data)
        {
            try
            {
                var myStorageManager = StorageClient.Create();

                var guid = Guid.NewGuid();
                var mynewobj = myStorageManager.UploadObject("programmingcloud", guid.ToString() + Path.GetExtension(data.FileName), data.ContentType, data.InputStream,
                    options: new Google.Cloud.Storage.V1.UploadObjectOptions() { PredefinedAcl = PredefinedObjectAcl.PublicRead });

                //imgName = guid.ToString() + Path.GetExtension(data.FileName);

                ViewBag.Success = "Item uploaded successfully.";
            }
            catch (Exception ex)
            {
                LoggingRepository.ReportError(ex);
                ViewBag.Error = ex + " -(Item was not uploaded)";
            }

            return RedirectToAction("Create", "Property");
        }

        public ActionResult PubSub(int id)
        {
            //get prop
            PropertyRepository p = new PropertyRepository();
            var prop = p.GetPropertyById(id);

            PubSubRepository psp = new PubSubRepository();
            var msg = new PropertyNotificationMessage() { Email = User.Identity.Name, Location = prop.Location, Fullname = prop.Username, PropertyName = prop.Name };
            psp.PublishMessage(JsonConvert.SerializeObject(msg), "TestTopic_2");

            ViewBag.Success = "success";
            return RedirectToAction("Index", "Property");
        }

        public ActionResult DeleteItems(List<int> items)
        {
            if (items.Count > 0)
            {

                PropertyRepository pr = new PropertyRepository();
                List<Property> myList = new List<Property>();
                try
                {
                    if (pr.MyConnection.State == System.Data.ConnectionState.Closed)
                    {
                        pr.MyConnection.Open();
                        pr.MyTransaction = pr.MyConnection.BeginTransaction();

                    }

                    //delete all items
                    foreach (int id in items)
                    {
                        pr.DeleteProperty(id);
                    }

                    pr.MyTransaction.Commit();

                    if (items.Count == 1)
                        ViewBag.Success = "Property was deleted";
                    else
                        ViewBag.Success = "Properties were deleted";
                }
                catch (Exception ex)
                {
                    pr.MyTransaction.Rollback();

                    //log exception
                    LoggingRepository.ReportError(ex);
                    ViewBag.Error = "Error occurred. Nothing was deleted; Try again later";
                }
                finally
                {
                    myList = pr.GetProperty().ToList(); //to refresh the now updated list

                    if (pr.MyConnection.State == System.Data.ConnectionState.Open)
                        pr.MyConnection.Close();
                }

                return View("Index", myList);

            }

            return RedirectToAction("Index");
        }




        // GET: Property
        [Authorize]
        public ActionResult Index()
        {
            PropertyRepository pr = new PropertyRepository();
            try
            {
                //1. Load items from cache
                RedisRepository rr = new RedisRepository();
                var items = rr.LoadItems();

                //2. hash the serialized value of items 
                pr.MyConnection.Open();
                var itemsFromDb = pr.GetProperty();
                pr.MyConnection.Close();

                if (items == null)
                {
                    rr.StoreItems(itemsFromDb);
                    items = rr.LoadItems();
                }

                //3. compare the digest produced with the digest produced earlier while stored in application variable
                if (rr.HashValue(JsonConvert.SerializeObject(items)) != rr.HashValue(JsonConvert.SerializeObject(itemsFromDb)))
                {
                    //4. if they do not match
                    //storeincache method and re-produce a new hashcode
                    rr.StoreItems(itemsFromDb);
                    items = rr.LoadItems();
                }

                return View(items);
            }
            catch (Exception ex)
            {
                LoggingRepository.ReportError(ex);
                ViewBag.Error = ex + " - (Error occurred while querying items)";
                return View(new List<Property>());
            }
            finally
            {
                if (pr.MyConnection.State == System.Data.ConnectionState.Open)
                {
                    pr.MyConnection.Close();
                }
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(PropertyViewModel p)
        {
            PropertyRepository cc = new PropertyRepository();
            try
            {
                if (cc.MyConnection.State == System.Data.ConnectionState.Closed)
                {
                    cc.MyConnection.Open();
                }

                //save in bucket
                var myStorageManager = StorageClient.Create();
                var guid = Guid.NewGuid();
                var mynewobj = myStorageManager.UploadObject("programmingcloud", guid.ToString() + Path.GetExtension(p.File.FileName), p.File.ContentType, p.File.InputStream,
                    options: new Google.Cloud.Storage.V1.UploadObjectOptions() { PredefinedAcl = Google.Cloud.Storage.V1.PredefinedObjectAcl.PublicRead });

                string imgName = guid.ToString() + Path.GetExtension(p.File.FileName);
                string us = User.Identity.Name;
                string u = us.Substring(0, us.IndexOf("@"));
                p.Username = u;
                var prop = new Property()
                {
                    Username = p.Username,
                    Location = p.Location,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    PropertyPicture = imgName
                };
                cc.AddProperty(prop);


                ViewBag.Success = "Property was created successfully";
                return View();
            }
            catch (Exception ex)
            {
                LoggingRepository.ReportError(ex);
                ViewBag.Error = ex + " - (Error occurred. Property was not added)";
                return View();
            }
            finally
            {
                if (cc.MyConnection.State == System.Data.ConnectionState.Open)
                {
                    cc.MyConnection.Close();
                }
            }
        }

        public ActionResult DeleteAll(List<int> items)
        {
            PropertyRepository ir = new PropertyRepository();
            List<Property> myList = new List<Property>();
            try
            {
                if (ir.MyConnection.State == System.Data.ConnectionState.Closed)
                {
                    ir.MyConnection.Open();
                    ir.MyTransaction = ir.MyConnection.BeginTransaction(); //pending until committed
                }

                foreach (int id in items)
                {
                    ir.DeleteProperty(id);
                }

                if (items.Count > 1)
                    ViewBag.Success = "Items were deleted successfully";
                else ViewBag.Success = "Item was deleted successfully";

                ir.MyTransaction.Commit(); //committed

                //this doesnt need to participate in transaction cos its not changing anything in database, its just getting.
                myList = ir.GetProperty();

                return View("Index", myList);
            }
            catch (Exception ex)
            {
                ir.MyTransaction.Rollback();

                if (items.Count > 1)
                    ViewBag.Error = ex + " - (Error occurred. Items were not deleted)";
                else ViewBag.Error = ex + " - (Error occurred. Item was not deleted)";

                myList = ir.GetProperty();
                return View("Index", myList);
            }
            finally
            {
                if (ir.MyConnection.State == System.Data.ConnectionState.Open)
                    ir.MyConnection.Close();
            }
        }

        public ActionResult StoreInRedis()
        {
            PropertyRepository ir = new PropertyRepository();

            try
            {
                if (ir.MyConnection.State == System.Data.ConnectionState.Closed)
                {
                    ir.MyConnection.Open();
                }

                var list = ir.GetProperty();
                //foreach (var item in list)
                //{
                //    var name = ir.GetPropertyName(item.Name);
                //    item.Name = name;
                //}

                RedisRepository rr = new RedisRepository();

                return View("Index", list);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex + " - (Error occurred. Items were not deleted)";
                return View(new List<Property>());
            }
            finally
            {
                if (ir.MyConnection.State == System.Data.ConnectionState.Open)
                    ir.MyConnection.Close();
            }
        }

        public ActionResult IndexFromRedis()
        {
            RedisRepository rr = new RedisRepository();

            return View("Index", rr.LoadItems());
        }

        public ActionResult RefreshCache()
        {
            //1. Load items from cache
            RedisRepository rr = new RedisRepository();
            var items = rr.LoadItems();

            //2. hash the serialized value of items 
            PropertyRepository pr = new PropertyRepository();
            pr.MyConnection.Open();
            var itemsFromDb = pr.GetProperty();
            pr.MyConnection.Close();

            if (items == null)
            {
                rr.StoreItems(itemsFromDb);
                return Content("done");
            }

            //3. compare the digest produced with the digest produced earlier while stored in application variable
            if (rr.HashValue(JsonConvert.SerializeObject(items)) != rr.HashValue(JsonConvert.SerializeObject(itemsFromDb)))
            {
                //4. if they do not match; storeincache method and re-produce a new hashcode
                rr.StoreItems(itemsFromDb); return Content("done");
            }

            return Content("properties were not updated since they are still the same");
        }


    }
}

public class PropertyNotificationMessage
{
    public string Location { get; set; }
    public string PropertyName { get; set; }
    public string Email { get; set; }
    public string Fullname { get; set; }
}