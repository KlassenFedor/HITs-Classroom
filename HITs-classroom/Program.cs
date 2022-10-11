using HITs_classroom;
using HITs_classroom.Helpers;
using HITs_classroom.Models.TsuAccount;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.

builder.Services.AddScoped<GoogleClassroomServiceForServiceAccount>();
builder.Services.AddScoped<ICoursesService, CoursesService>();
builder.Services.AddScoped<IInvitationsService, InvitationsService>();
builder.Services.AddScoped<ICourseMembersService, CourseMembersService>();
builder.Services.AddScoped<ICourseWorksService, CourseWorksService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICoursesListService, CoursesListService>();
builder.Services.AddScoped<ITeachersSearchservice, TeachersSearchServcie>();

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
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connection));

builder.Services.AddIdentity<TsuAccountUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<TsuAccountUser>>()
    .AddUserManager<UserManager<TsuAccountUser>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

var app = builder.Build();

using var serviceScope = app.Services.CreateScope();
var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

await app.ConfigureIdentityAsync();

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
