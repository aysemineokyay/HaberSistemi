using App.Data.Concrete.EntityFramework.Context;
using App.Entities.OptionsModels;
using App.Service.AutoMapper.Profiles;
using App.Service.Extensions;
using App.Web.Mvc.AutoMapper.Profiles;
using App.Web.Mvc.Helpers.Abstract;
using App.Web.Mvc.Helpers.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// AddAsync services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddAutoMapper(typeof(CategoryProfile), typeof(NewsProfile),typeof(UserProfile));
builder.Services.LoadMyServices();
builder.Services.AddScoped<IImageHelper,ImageHelper>();
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = new PathString("/Auth/Login"); // Login sayfa url'si
	options.LogoutPath = new PathString("/Auth/Logout"); //Logout sayfa url'si
	options.Cookie = new CookieBuilder
	{
		Name="AspNetMvcNews", //Cookie ismi
		HttpOnly = true, // Client-side taraf�nda cookie bilgilerini gizlemek, korumak i�in
		SameSite= SameSiteMode.Strict, //Bizim cookie'lerimizi kullanarak farkl� yerden gelmesini �nleyen g�venlik �nlemi
		SecurePolicy=CookieSecurePolicy.SameAsRequest //Gelen istek http �zerinden gelirse http , https �zerinden gelirse https �zerinden d�n�� yapar. G�venlik a��s�ndan ".Always" olmal�.
	
	};
	options.SlidingExpiration = true; //Giri� yapt�ktan sonra tekrar giri� yap�l�rsa Cookie s�resi tekrar 5 g�n uzar
	options.ExpireTimeSpan = TimeSpan.FromDays(5); // Taray�c� �zerinde Cooki bilgileri 5 g�n kalacak
	options.AccessDeniedPath = new PathString("/Auth/AccessDenied"); // Giri� yapan kullan�c� yetkisi olmayan bir yere giri� yaparsa buraya y�nlendirilecek
});
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConStr"));
});
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
	opt.TokenLifespan = TimeSpan.FromHours(2); // �ifremi unuttum i�in Token 2 saat �mr� var
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
var app = builder.Build();

//Son migration yap�lmam��sa otomatik yapar.
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.Migrate();
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
app.UseSession();//Server'da session olu�mas�n� istiyoruz
app.UseStatusCodePages(); //Olmayan sayfaya gidilirse 404 hatas� sayfas� a��l�r
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
	name: "admin",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
		  );
//app.MapControllerRoute(
//    name: "settings",
//    pattern: "{PasswordChage}",
//    defaults: new { area="Admin", controller = "Setting", action = "PasswordChange" });
         
app.MapControllerRoute(
	name: "news",
	pattern: "{News}/{Details}/{title}/{id}",
	defaults: new { controller = "News", action = "Detail" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");




app.Run();
