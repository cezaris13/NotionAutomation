using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MimeKit;

namespace NotionTaskAutomation;

public class EmailClient : IEmailClient
{
    private readonly IConfiguration m_configuration;
    private readonly GmailService m_gmailService;

    public EmailClient(IConfiguration configuration)
    {
        m_configuration = configuration;

        // Initialize the Gmail service during the constructor
        m_gmailService = InitializeGmailService();
    }

    private GmailService InitializeGmailService()
    {
        using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read)) //Secret generated from Google (OAuth)
        {
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { GmailService.Scope.GmailSend },
                //"notionAutomation6@gmail.com",    //In this case change line 91: "me" -> "notionAutomation6@gmail.com"
                "user",
                System.Threading.CancellationToken.None).Result;

            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "notionAutomation",
            });
        }
    }

    private string Base64UrlEncode(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(inputBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    public async Task SendEmail()
    {
        //var sendGridApiKey = m_configuration.GetValue<string>("SendGridApiKey");
        //Console.WriteLine($"Email has been sent {sendGridApiKey}");

        //var sendGmailAPI = m_configuration.GetValue<string>("gmailAPIkey");
        //Console.WriteLine(sendGmailAPI);

        try
        {
            var email = new MimeMessage();
            //I don;t get it ... Why the line 65 exists since the mail is sent from the user based on line 35
            //email.From.Add(new MailboxAddress("Mr. Notion Button", "notionAutomation6@gmail.com"));
            email.To.Add(new MailboxAddress("Leon", "iraklara.ira@gmail.com"));
            email.Subject = "Testing the Button";

            var body = new TextPart("plain")
            {
                Text = "Hello, this is the email body. A test from notionAutomation Butoon"
            };

            var multipart = new Multipart("mixed")
            {
                body
            };
            email.Body = multipart;

            var rawMessage = Base64UrlEncode(email.ToString());

            var message = new Message
            {
                Raw = rawMessage
            };

            //There is a relationship/connection between the lines 91 and 35.
            //They define the sender and they must agree. "me" is the authenticated user
            var result = m_gmailService.Users.Messages.Send(message, "me").Execute();
            Console.WriteLine("Message sent. Message id: " + result.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending E-mail: " + ex.Message);
        }


    }
}