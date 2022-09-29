using HITs_classroom;
using HITs_classroom.Jobs;
using HITs_classroom.Models.User;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using System.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.

builder.Services.AddScoped<GoogleClassroomService>();
builder.Services.AddScoped<ICoursesService, CoursesService>();
builder.Services.AddScoped<IInvitationsService, InvitationsService>();
builder.Services.AddScoped<ICourseMembersService, CourseMembersService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServicesAccountsService, ServicesAccountsService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddCors();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));

builder.Services.AddIdentity<ClassroomAdmin, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<ClassroomAdmin>>()
    .AddUserManager<UserManager<ClassroomAdmin>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

var app = builder.Build();

using var serviceScope = app.Services.CreateScope();
var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

//InvitationsScheduler.Start();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
