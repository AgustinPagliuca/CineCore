using Microsoft.AspNetCore.Identity;
using CineCore.Models;

namespace CineCore.Data
{
    public static class SeedData
    {
        public static async Task InicializarRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Empleado", "Cliente" };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                    await roleManager.CreateAsync(new IdentityRole(rol));
            }

            // Crear empleado admin por defecto
            var adminEmail = "admin@cinecore.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nombre = "Admin",
                    Apellido = "CineCore",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Empleado");
            }
        }
    }
}
