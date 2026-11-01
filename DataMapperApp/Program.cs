using DataMapperApp.DataMappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDataMapper(builder.Configuration);

var app = builder.Build();

app.Logger.LogInformation(
    "Active data mapper is {DataMapper}",
    app.Configuration["AppSettings:DataMapperClassName"]);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
