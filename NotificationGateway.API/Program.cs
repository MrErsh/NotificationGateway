using FluentValidation;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using NotificationGateway.API.Hangfire;
using NotificationGateway.Application.DTOs;
using NotificationGateway.Application.Interfaces;
using NotificationGateway.Application.Services;
using NotificationGateway.Application.Validators;
using NotificationGateway.Domain.Interfaces;
using NotificationGateway.Infrastructure.Data;
using NotificationGateway.Infrastructure.Data.Repositories;
using NotificationGateway.Infrastructure.Interfaces;
using NotificationGateway.Infrastructure.Services;
using NotificationGateway.Infrastructure.Services.Senders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "NotificationService_";
});

// Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "critical", "default" };
    options.WorkerCount = Environment.ProcessorCount * 2;
});

// HttpClients
builder.Services.AddHttpClient("Telegram", client =>
{
    client.BaseAddress = new Uri("https://api.telegram.org/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("Webhook", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
});

// Application Layer
builder.Services.AddScoped<INotificationAppService, NotificationAppService>();
builder.Services.AddScoped<IValidator<NotificationRequestDto>, NotificationRequestValidator>();

// Domain Interfaces -> Infrastructure Implementations
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationProcessingService, NotificationProcessingService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

// Notification Senders
builder.Services.AddScoped<TelegramNotificationSender>();
builder.Services.AddScoped<SmtpNotificationSender>();
builder.Services.AddScoped<WebhookNotificationSender>();
builder.Services.AddScoped<INotificationSenderFactory, NotificationSenderFactory>();

// Hangfire Services
builder.Services.AddScoped<INotificationHandler, NotificationHandler>();

// Recurring Jobs
builder.Services.AddScoped<IRecurringJobsService, RecurringJobsService>();
builder.Services.AddScoped<IStuckNotificationsService, StuckNotificationsService>();
builder.Services.AddScoped<INotificationCleanupService, NotificationCleanupService>();

var app = builder.Build();

// Initialize recurring jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobsService = scope.ServiceProvider.GetRequiredService<IRecurringJobsService>();
    recurringJobsService.SetupRecurringJobs();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("Hangfire:Dashboard:Enabled"))
{
    var dashboardPath = app.Configuration["Hangfire:Dashboard:Path"] ?? "/hangfire";
    var dashboardApiKey = app.Configuration["Hangfire:Dashboard:ApiKey"];
    var allowLoopback = app.Environment.IsDevelopment();

    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        DashboardTitle = "Notification Service Jobs",
        StatsPollingInterval = 10000,
        Authorization = new[] { new ApiKeyOrLocalDashboardFilter(dashboardApiKey, allowLoopback) }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
