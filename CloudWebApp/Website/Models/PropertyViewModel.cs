using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class PropertyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PropertyPicture { get; set; }
        public string Location { get; set; }
        public string Username { get; set; }
        public HttpPostedFileBase File { get; set; }
        
    }
}