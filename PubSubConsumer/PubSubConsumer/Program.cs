using Newtonsoft;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Mail;

namespace PubSubConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string fullPath = "ProgrammingForTheCloud-d5293430572d.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
            PubSubRepository psp = new PubSubRepository();
            while (true)
            {
                string message = psp.PullMessage("TestTopic_2", "TestTopicSub_2");
                if(message != null)
                {
                    var msg = JsonConvert.DeserializeObject<PropertyNotificationMessage>(message);
                    Console.WriteLine(message);

                    MailMessage mail = new MailMessage("cloudpEmail@gmail.com", msg.Email);
                    var client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("cloudpEmail@gmail.com", "xxxx"),
                        EnableSsl = true
                    };
                    mail.Subject = msg.PropertyName;
                    mail.Body = "Dear " + msg.Fullname + ",<br/>" + "This is to confirm that you have registered interest in the following property " +
                        ": " + msg.PropertyName + "Location: " + msg.Location + ", we will contact you shortly ";
                    client.Send(mail);
                }
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
}
