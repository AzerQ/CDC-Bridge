// Название задачи: Веб-приложение для приема и отображения изменений CDC
// Описание задачи: Приложение принимает webhook-уведомления об изменениях данных и отображает их
// Чек-лист выполнения задачи:
// - [x] Создание эндпоинта для приема webhook-уведомлений
// - [x] Создание эндпоинта для отображения полученных данных
// - [x] Хранение данных в памяти

using CdcWebhookReceiver.Models;
using CdcWebhookReceiver.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация сервиса для хранения данных в памяти
builder.Services.AddSingleton<IChangeDataCaptureService, ChangeDataCaptureService>();

// Добавление CORS для разработки
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();