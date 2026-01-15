using Azure.Core;
using Grpc.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace api
{
    public class EnviarMailDesdeLandingPage
    {
        private readonly ILogger _logger;
        private readonly MailServiceOptions _mailOptions;
        public EnviarMailDesdeLandingPage(ILoggerFactory loggerFactory, IOptions<MailServiceOptions> options)
        {
            _logger = loggerFactory.CreateLogger<EnviarMailDesdeLandingPage>();
            _mailOptions = options.Value;
        }

        [Function("EnviarMailDesdeLandingPage")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "EnviarMailDesdeLandingPage")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var body = await JsonSerializer.DeserializeAsync<MailLandingPageRequest>(req.Body);

            if (body == null || string.IsNullOrEmpty(body.Token))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            string asunto = $"Contacto por ALUAR - {body.Nombre}";
            string cuerpo = $@"
                    <h3>Nuevo mensaje desde el formulario web</h3>
                    <p><strong>Nombre:</strong> {body.Nombre}</p>
                    <p><strong>Empresa:</strong> {body.Empresa}</p>
                    <p><strong>Email:</strong> {body.Email}</p>
                    <p><strong>Teléfono:</strong> {body.Telefono}</p>
                    <p><strong>Mensaje:</strong></p>
                    <p>{body.Mensaje}</p>
                ";

            var mMailMessage = new MailMessage
            {
                From = new MailAddress(_mailOptions.MailFrom, "Landing Page"),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = _mailOptions.IsBodyHtml,
                Priority = MailPriority.Normal
            };
            string to = "rpalma@codes.com.ar";
            mMailMessage.To.Add(new MailAddress(to));

      
            var smtp = new SmtpClient(_mailOptions.SmtpClient)
            {
                Port = _mailOptions.SmtpClientPort,
                EnableSsl = _mailOptions.SmtpClientEnableSSL,
                UseDefaultCredentials = _mailOptions.SmtpClientUseDefaultCredentials,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            try
            {
                await smtp.SendMailAsync(mMailMessage);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new { ok = true });
                _logger.LogInformation($"Ejecutando {nameof(EnviarMailDesdeLandingPage)} Fin");
                return ok;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new { error = ex.Message });
                return error;
            }


        }
    }
}
