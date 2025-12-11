using Adm_Notificaciones.Entities;
using System.Net;
using System.Net.Mail;

namespace Adm_Notificaciones.Service
{
    public class NotificacionService
    {
        private readonly IConfiguration _config;
        private readonly AutenticacionService _auth;
        private readonly BitacoraConsumer _bitacora;

        public NotificacionService(IConfiguration config, AutenticacionService auth, BitacoraConsumer bitacora)
        {
            _config = config;
            _auth = auth;
            _bitacora = bitacora;
        }

        public async Task<BusinessLogicResponse> EnviarCorreoAsync(NotificacionRequest request, string? token)
        {
            if (!await _auth.ValidarTokenAsync(token))
                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

            string usuario = await _auth.ObtenerUsuarioDelTokenAsync(token) ?? "sistema";

            try
            {
                string fromEmail = _config["Gmail:Usuario"];
                string fromName = _config["Gmail:FromName"];
                string password = _config["Gmail:Password"];

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential(fromEmail, password);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, fromName),
                        Subject = request.Asunto,
                        Body = GenerarHtmlCorreo(request.Asunto, request.Mensaje),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(request.Email);
                    await smtpClient.SendMailAsync(mailMessage);
                }

                // REGISTRAR EN BITÁCORA CON FORMATO ESPECÍFICO
                await _bitacora.RegistrarAccionAsync(usuario, "NOTIFICACION_ENVIADA", new
                {
                    accion = "Notificación Enviada",
                    descripcion = $"Correo enviado a {request.Email}: {request.Asunto}",
                    destinatario = request.Email,
                    asunto = request.Asunto,
                    fecha = DateTime.Now,
                    estado = "Enviado"
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 200,
                    Message = "Correo enviado correctamente.",
                    ResponseObject = new { request.Email, request.Asunto }
                };
            }
            catch (SmtpException smtpEx)
            {
                await _bitacora.RegistrarAccionAsync(usuario, "ERROR_SMTP", new
                {
                    accion = "Error al Enviar Notificación",
                    descripcion = $"Error SMTP al enviar correo a {request.Email}: {smtpEx.Message}",
                    destinatario = request.Email,
                    asunto = request.Asunto,
                    error = smtpEx.Message
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error SMTP al enviar correo: {smtpEx.Message}"
                };
            }
            catch (Exception ex)
            {
                await _bitacora.RegistrarAccionAsync(usuario, "ERROR_GENERAL", new
                {
                    accion = "Error al Enviar Notificación",
                    descripcion = $"Error general al enviar correo a {request.Email}: {ex.Message}",
                    error = ex.Message
                });

                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error general al enviar correo: {ex.Message}"
                };
            }
        }

        private string GenerarHtmlCorreo(string titulo, string mensaje)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>Notificación - Sistema Avatar</title>
    <style>
        body {{
            font-family: 'Poppins', Arial, sans-serif;
            background-color: #f5f8ff;
            color: #333;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 100%;
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 16px;
            box-shadow: 0 6px 20px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #3A59D1, #3D90D7);
            color: white;
            padding: 25px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            letter-spacing: 1px;
        }}
        .content {{
            padding: 30px;
        }}
        .content h2 {{
            color: #3A59D1;
            margin-top: 0;
        }}
        .content p {{
            font-size: 15px;
            line-height: 1.7;
            margin-bottom: 15px;
        }}
        .footer {{
            background-color: #f0f3ff;
            text-align: center;
            padding: 15px;
            font-size: 12px;
            color: #777;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='content'>
            <h2>{titulo}</h2>
            <p>{mensaje}</p>
            <p>Este mensaje ha sido generado automáticamente por el módulo de notificaciones del <strong>Sistema Avatar</strong>.</p>
        </div>
        <div class='footer'>
            © {DateTime.Now.Year} Sistema Avatar - Los Hokages.<br/>
             CUC · Programación V
        </div>
    </div>
</body>
</html>";
        }
    }
}









//using Adm_Notificaciones.Entities;
//using System.Net;
//using System.Net.Mail;

//namespace Adm_Notificaciones.Service
//{
//    public class NotificacionService
//    {
//        private readonly IConfiguration _config;
//        private readonly AutenticacionService _auth;
//        private readonly BitacoraConsumer _bitacora;

//        public NotificacionService(IConfiguration config, AutenticacionService auth, BitacoraConsumer bitacora)
//        {
//            _config = config;
//            _auth = auth;
//            _bitacora = bitacora;
//        }

//        public async Task<BusinessLogicResponse> EnviarCorreoAsync(NotificacionRequest request, string? token)
//        {
//            // Validar token
//            if (!await _auth.ValidarTokenAsync(token))
//                return new BusinessLogicResponse { StatusCode = 401, Message = "No autorizado" };

//            string usuario = await _auth.ObtenerUsuarioDelTokenAsync(token) ?? "sistema";

//            try
//            {
//                // Leer parámetros desde appsettings.json
//                string fromEmail = _config["Gmail:Usuario"];
//                string fromName = _config["Gmail:FromName"];
//                string password = _config["Gmail:Password"];

//                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
//                {
//                    smtpClient.Port = 587;
//                    smtpClient.Credentials = new NetworkCredential(fromEmail, password);
//                    smtpClient.EnableSsl = true;

//                    var mailMessage = new MailMessage
//                    {
//                        From = new MailAddress(fromEmail, fromName),
//                        Subject = request.Asunto,
//                        Body = GenerarHtmlCorreo(request.Asunto, request.Mensaje),
//                        IsBodyHtml = true
//                    };

//                    mailMessage.To.Add(request.Email);
//                    await smtpClient.SendMailAsync(mailMessage);
//                }

//                await _bitacora.RegistrarAccionAsync(usuario, "SEND_MAIL", new
//                {
//                    Destinatario = request.Email,
//                    Asunto = request.Asunto
//                });

//                return new BusinessLogicResponse
//                {
//                    StatusCode = 200,
//                    Message = "Correo enviado correctamente.",
//                    ResponseObject = new { request.Email, request.Asunto }
//                };
//            }
//            catch (SmtpException smtpEx)
//            {
//                await _bitacora.RegistrarAccionAsync(usuario, "ERROR_SMTP", new { error = smtpEx.Message });

//                return new BusinessLogicResponse
//                {
//                    StatusCode = 500,
//                    Message = $"Error SMTP al enviar correo: {smtpEx.Message}"
//                };
//            }
//            catch (Exception ex)
//            {
//                await _bitacora.RegistrarAccionAsync(usuario, "ERROR_GENERAL", new { error = ex.Message });

//                return new BusinessLogicResponse
//                {
//                    StatusCode = 500,
//                    Message = $"Error general al enviar correo: {ex.Message}"
//                };
//            }
//        }

//        private string GenerarHtmlCorreo(string titulo, string mensaje)
//        {
//            return $@"
//<!DOCTYPE html>
//<html lang='es'>
//<head>
//    <meta charset='UTF-8'>
//    <title>Notificación - Sistema Avatar</title>
//    <style>
//        body {{
//            font-family: 'Poppins', Arial, sans-serif;
//            background-color: #f5f8ff;
//            color: #333;
//            margin: 0;
//            padding: 0;
//        }}
//        .container {{
//            width: 100%;
//            max-width: 600px;
//            margin: 40px auto;
//            background-color: #ffffff;
//            border-radius: 16px;
//            box-shadow: 0 6px 20px rgba(0,0,0,0.1);
//            overflow: hidden;
//        }}
//        .header {{
//            background: linear-gradient(135deg, #3A59D1, #3D90D7);
//            color: white;
//            padding: 25px;
//            text-align: center;
//        }}
//        .header h1 {{
//            margin: 0;
//            font-size: 24px;
//            letter-spacing: 1px;
//        }}
//        .content {{
//            padding: 30px;
//        }}
//        .content h2 {{
//            color: #3A59D1;
//            margin-top: 0;
//        }}
//        .content p {{
//            font-size: 15px;
//            line-height: 1.7;
//            margin-bottom: 15px;
//        }}
//        .footer {{
//            background-color: #f0f3ff;
//            text-align: center;
//            padding: 15px;
//            font-size: 12px;
//            color: #777;
//        }}
//        .logo {{
//            width: 80px;
//            margin-bottom: 10px;
//        }}
//        .cta {{
//            display: inline-block;
//            padding: 12px 24px;
//            background: #3A59D1;
//            color: white !important;
//            text-decoration: none;
//            border-radius: 30px;
//            font-weight: bold;
//            margin-top: 20px;
//        }}
//        .cta:hover {{
//            background: #2b46b2;
//        }}
//    </style>
//</head>
//<body>
//    <div class='container'>
//        <div class='content'>
//            <h2>{titulo}</h2>
//            <p>{mensaje}</p>
//            <p>Este mensaje ha sido generado automáticamente por el módulo de notificaciones del <strong>Sistema Avatar</strong>.</p>
//        </div>
//        <div class='footer'>
//            © {DateTime.Now.Year} Sistema Avatar - Los Hokages.<br/>
//             CUC · Programación V
//        </div>
//    </div>
//</body>
//</html>";
//        }

//    }
//}
