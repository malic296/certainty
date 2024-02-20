using certainty.Injections;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "MyCookieAuth";
        options.LoginPath = "/AccessDenied";

    }
);



builder.Services.AddScoped<ISqlClient, SqlClient>();
builder.Services.AddTransient<IEmailSenderService, EmailSenderService>();
builder.Services.AddTransient<IAPIService, APIService>();
builder.Services.AddTransient<IOnPostVerification, OnPostVerification>();



builder.Services.AddSession(o => {
    o.IdleTimeout = TimeSpan.FromMinutes(30);
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
