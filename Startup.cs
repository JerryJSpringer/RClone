using RClone.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using System;
using System.Threading.Tasks;
using RClone.Models;
using Microsoft.AspNetCore.Authorization;
using RClone.Authorization;
using Microsoft.Extensions.Logging;

namespace RClone
{
	public class Startup
	{

		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			// Configures cookie policy
			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			// Adds the database context
			services.AddDbContext<RCloneDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
			// Adds the default identity
			services.AddDefaultIdentity<ApplicationUser>()
				.AddDefaultUI(UIFramework.Bootstrap4)
				.AddRoles<IdentityRole>()
				.AddRoleManager<RoleManager<IdentityRole>>()
				.AddEntityFrameworkStores<RCloneDbContext>()	
				.AddDefaultTokenProviders();
			// Adds authorization
			services.AddAuthorization();

			// Configure identity options
			services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequiredLength = 6;
				options.Password.RequireDigit = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireNonAlphanumeric = false;

				// Lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 6;
				options.Lockout.AllowedForNewUsers = true;

				// User settings
				options.User.AllowedUserNameCharacters =
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
				options.User.RequireUniqueEmail = true;
			});

			// Configures the application cookie
			services.ConfigureApplicationCookie(options =>
			{
				// Cookie settings
				options.Cookie.HttpOnly = true;
				options.ExpireTimeSpan = TimeSpan.FromMinutes(10);

				options.LoginPath = "/Identity/Account/Login";
				options.AccessDeniedPath = "/Identity/Account/AccessDeined";
				options.SlidingExpiration = true;
			});

			// Adds MVC and sets the compatibility version to 2.2
			services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

			// Authorization handlers
			services.AddScoped<IAuthorizationHandler,
				UserIsPosterAuthorizationHandler>();
			services.AddScoped<IAuthorizationHandler,
				UserIsCommenterAuthorizationHandler>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env,
			ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseAuthentication();

			// Note we are using attribute routing
			// as opposed to conventional routing
			app.UseMvc();

			// Create roles
			CreateRoles(serviceProvider).Wait();
		}

		private async Task CreateRoles(IServiceProvider serviceProvider)
		{
			var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			string[] roleNames = { Constants.RCloneAdminRole, 
				Constants.RCloneModRole,
				Constants.RCloneNormalRole};

			foreach (var roleName in roleNames)
			{
				var roleExist = await RoleManager.RoleExistsAsync(roleName);
				IdentityResult roleResult;

				if (!roleExist)
				{
					roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
				}
			}
		}
	}
}
