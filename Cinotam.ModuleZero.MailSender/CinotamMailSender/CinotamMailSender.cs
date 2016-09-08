﻿using Abp.Domain.Services;
using Abp.Net.Mail;
using Cinotam.MailSender.SendGrid.SendGrid;
using Cinotam.MailSender.SendGrid.SendGrid.Inputs;
using Cinotam.ModuleZero.MailSender.CinotamMailSender.Outputs;
using CInotam.MailSender.Contracts;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Cinotam.ModuleZero.MailSender.CinotamMailSender
{
    public class CinotamMailSender : DomainService, ICinotamMailSender
    {
        private readonly IEmailSender _emailSender;
        private readonly ISendGridService _sendGridService;
        public CinotamMailSender(IEmailSender emailSender, ISendGridService sendGridService)
        {
            _emailSender = emailSender;
            _sendGridService = sendGridService;
        }

        public async Task<IMailServiceResult> SendMail(IMail input)
        {
            foreach (var mailServiceProvider in MailSenderAbpModule.MailServiceProviders)
            {
                var result = await mailServiceProvider.DeliverMail(input);
                if (result.MailSent) return new EmailSentResult()
                {
                    MailSent = true
                };
            }
            throw new InvalidOperationException(nameof(MailSenderAbpModule));
            //var result = new EmailSentResult();

            //var resultSmtp = await SendViaSmtp(input.MailMessage);
            //result.SentWithSmtp = resultSmtp;
            ////If was sent via smtp just return
            //if (resultSmtp) return result;
            ////Implement httpServices here

            //var resultHttp = await SendViaHttp(input);
            //result.SentWithHttp = resultHttp;

            //if (result.SentWithHttp || result.SentWithSmtp)
            //{
            //    input.Sent = true;
            //}
            //return result;
        }

        async Task<bool> SendViaHttp(IMail input)
        {

            var firstOrDefault = input.MailMessage.To.FirstOrDefault();
            if (firstOrDefault == null) return false;
            var mailAddress = input.MailMessage.To.FirstOrDefault();
            if (mailAddress == null) return false;
            var response = await _sendGridService.SendViaHttp(new SendGridMessageInput()
            {
                Body = input.Body,
                EncodeType = input.EncodeType,
                Message = input.MailMessage,
                Subject = input.MailMessage.Subject,
                To = mailAddress.Address,
                TemplateId = input.ExtraParams.TemplateId,
                Substitutions = input.ExtraParams.Substitutions,
            });
            return response.Success;
        }
        async Task<bool> SendViaSmtp(MailMessage message)
        {
            try
            {
                var useSmtp = bool.Parse((await SettingManager.GetSettingValueAsync("UseSmtp")));

                if (!useSmtp) return false;
                await _emailSender.SendAsync(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IMailServiceResult> DeliverMail(IMail mail)
        {
            var result = (EmailSentResult)(await SendMail(mail));
            return new EmailSentResult()
            {
                MailSent = result.MailSent,
                SentWithSmtp = result.SentWithSmtp,
                SentWithHttp = result.SentWithHttp
            };
        }
    }
}
