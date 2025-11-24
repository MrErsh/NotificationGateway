using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Application.Interfaces;

namespace NotificationGateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;
        private readonly IValidator<NotificationRequestDto> _validator;
        private readonly INotificationAppService _appService;

        public NotificationsController(
            ILogger<NotificationsController> logger,
            IValidator<NotificationRequestDto> validator,
            INotificationAppService appService)
        {
            _logger = logger;
            _validator = validator;
            _appService = appService;
        }

        [HttpPost("notify")]
        [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationResponseDto>> Notify(
            [FromBody] NotificationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validation failed for notification request: {Errors}",
                        string.Join(", ", validationResult.Errors));
                    return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
                }

                _logger.LogInformation(
                    "Processing notification request: Channel={Channel}, Type={MessageType}, Recipient={Recipient}",
                    request.Channel, request.MessageType, request.Recipient);

                var result = await _appService.NotifyAsync(request, cancellationToken);

                return Accepted(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification request");
                return Problem(
                    title: "Internal server error",
                    detail: "An error occurred while processing your request",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
