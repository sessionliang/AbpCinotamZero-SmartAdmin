﻿using CInotam.MailSender.Contracts;
using System.Collections.Generic;
using System.Net.Mail;

namespace Cinotam.MailSender.SendGrid.SendGrid.Inputs
{
    public class SendGridMessageInput : IMail
    {
        /*
         MailMessage message, string toMail, string body, string encodeType
         
         */
        public MailMessage Message { get; set; }
        public string To { get; set; }
        public MailMessage MailMessage { get; set; }
        public string HtmlView { get; set; }
        public string Body { get; set; }
        public string TemplateId { get; set; }
        /// <summary>
        /// text/html, text/plain, if has template id it must be text/html
        /// </summary>
        public string EncodeType { get; set; }

        public dynamic ExtraParams { get; set; }
        public bool Sent { get; set; }
        public Dictionary<string, string> Substitutions { get; set; }
        public string Subject { get; set; }
    }
}
