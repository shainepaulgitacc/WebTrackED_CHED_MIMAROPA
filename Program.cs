
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Hubs;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation;
using WebTrackED_CHED_MIMAROPA.Model.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var EmailSettingsConfig = builder.Configuration.GetSection("EmailSettings");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(connectionString);
});
builder.Services.AddAutoMapper(typeof(ModelMapper));
builder.Services.Configure<EmailSettings>(EmailSettingsConfig);

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

builder.Services.AddScoped<IDocumentAttachmentRepository, DocumentAttachmentRepository>();
builder.Services.AddScoped<IDocumentTrackingRepository, DocumentTrackingRepository>();
builder.Services.AddScoped<ICHEDPersonelRepository, CHEDPersonelRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ISenderRepository, SenderRepository>();
builder.Services.AddScoped<FileUploader>();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddSignalR();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
	.AddDefaultIdentity<AppIdentityUser>(options =>
	{
		options.SignIn.RequireConfirmedEmail = true;
		options.SignIn.RequireConfirmedAccount = true;
	})
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure security stamp validation interval
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
	options.ValidationInterval = TimeSpan.FromSeconds(1); 
});
builder.Services.AddRazorPages();
var app = builder.Build();



//seeding
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var roles = new[] { "Admin", "Reviewer", "Sender" };
	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole(role));
		}
	}
	var settingsRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<Settings>>();
	var officeRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<Office>>();
	var settingsRecords = await settingsRepo.GetAll();
	var officeRecords = await officeRepo.GetAll();	

	if (settingsRecords.Count() == 0)
	{
		await settingsRepo.Add(new Settings
		{
			LogoFileName = "CHEDLogo.jpg",
			AddedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,
			DocumentNotif = true,
			RegisteredUserNotif = true,
			PasswordRequiredLength = 6,
			EnableRegistration = true,
			EmailDomain = "ched.gov.ph"
		});
	}
	if(officeRecords.Count() == 0)
	{
		await officeRepo.Add(new Office
		{
			OfficeName = "Records Office",
			AddedAt = DateTime.Now,
			UpdatedAt = DateTime.Now,

		});
	}
};


// Configure the HTTP request pipeline.


if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.MapHub<NotificationHub>("/notification");
app.MapHub<MessageHub>("/message");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ensure authentication middleware is used
app.UseAuthorization();

app.MapRazorPages();

app.Run();
