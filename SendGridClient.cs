using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;
using EmailService.Options;

namespace EmailService;

public class SendGridClient
{
    private readonly ILogger<SendGridClient> _logger;
    private readonly ISendGridClient _sendGridClient;
    private readonly EmailOptions _emailOptions;

    public SendGridClient(
        ILogger<SendGridClient> logger,
        ISendGridClient sendGridClient,
        IOptions<EmailOptions> emailOptions
    )
    {
        _logger = logger;
        _sendGridClient = sendGridClient;
        _emailOptions = emailOptions.Value;
    }

    public async ValueTask<(bool isSuccess, string errorMessage)> SendEmailAsync(string email, string subject, string message)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
            Subject = subject
        };

        msg.AddContent(MimeType.Text, message);
        msg.AddTo(new EmailAddress(email));

        var response = await _sendGridClient.SendEmailAsync(msg).ConfigureAwait(false);

        if (response.IsSuccessStatusCode) return (true, string.Empty);

        var responseBody = await response.Body.ReadAsStringAsync();
        _logger.LogError($"SendGrid send email failed: {response.StatusCode} {responseBody}");
        return (false, responseBody);
    }
}
