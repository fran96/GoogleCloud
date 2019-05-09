using Website.Models;
using Google.Cloud.Datastore.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
    public class DataStoreController : Controller
    {
        // GET: DataStore
        [Authorize]
        public ActionResult Index()
        {
            var dbStore = DatastoreDb.Create("programming-for-the-cloud"); //getting an instance of the database

            Query q = new Query("Username");

            var resultOfQuery = dbStore.RunQuery(q);
            List<UserLog> userlogs = new List<UserLog>();
            foreach (var user in resultOfQuery.Entities)
            {
                UserLog log = new UserLog()
                {
                    Id = user.Key.Path.First().Id,
                    Key = user.Key.ToString(),
                    Email = user["email"].StringValue,
                    LoggedOn = user["loggedOn"].TimestampValue.ToDateTime(),

                };
                try
                {
                    log.LoggedOut = user["loggedOut"].IsNull ? (DateTime?)null : user["loggedOut"].TimestampValue.ToDateTime();
                }
                catch
                {

                }
                userlogs.Add(log);
            }
            return View(userlogs);
        }

        public void LogOut(string username)
        {
            var dbStore = DatastoreDb.Create("programming-for-the-cloud"); //getting an instance of the database

            Query q = new Query("Username");
            q.Filter = Google.Cloud.Datastore.V1.Filter.Property("email", username, PropertyFilter.Types.Operator.Equal);

            var resultOfQuery = dbStore.RunQuery(q);

            if (resultOfQuery.Entities.Count != 0)
            {
                var userLoggedIn = resultOfQuery.Entities.FirstOrDefault();
                userLoggedIn["loggedOut"] = DateTime.UtcNow;

                dbStore.Update(userLoggedIn);
            }

        }

        public ActionResult Create()
        {
            var dbStore = DatastoreDb.Create("programming-for-the-cloud"); //getting an instance of the database
            var objectManager = dbStore.CreateKeyFactory("Username"); //creating or getting an instance of the user table

            var primaryKey = objectManager.CreateIncompleteKey(); //generating a new auto primary key
            Entity myNewUser = new Entity() //creating a new instand of User on the fly
            {
                Key = primaryKey,
                ["email"] = User.Identity.Name,
                ["loggedOn"] = DateTime.UtcNow
            };

            dbStore.Insert(myNewUser);

            return RedirectToAction("Index");
        }

        public ActionResult Delete(long id)
        {
            var dbStore = DatastoreDb.Create("programming-for-the-cloud"); //getting an instance of the database
            var objectManager = dbStore.CreateKeyFactory("Username"); //creating or getting an instance of the user table

            Entity userToDelete = dbStore.Lookup(objectManager.CreateKey(id)); //get existing user 
            dbStore.Delete(userToDelete);

            return RedirectToAction("Index");
        }
    }
}