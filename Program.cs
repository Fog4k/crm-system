using CrmSystem.Api.Data;
using CrmSystem.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Явно указываем слушать на 80 порту
builder.WebHost.UseUrls("http://0.0.0.0:80");

// Db
builder.Services.AddDbContext<CrmDbContext>(opt =>
    opt.UseSqlite("Data Source=crm.db"));

// Services
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<PdfService>();

// 🔐 Basic Auth
builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);
builder.Services.AddAuthorization();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 🔐 Добавляем поддержку Basic Auth в Swagger
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Enter your username and password"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            new string[] {}
        }
    });
});

// Поддержка wwwroot (HTML/JS/CSS фронт)
builder.Services.AddDirectoryBrowser(); // если нужно прямой доступ к папкам

var app = builder.Build();

// DB migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    db.Database.Migrate();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Static files (frontend)
app.UseDefaultFiles(); // ищет index.html
app.UseStaticFiles();  // обслуживает из wwwroot

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
