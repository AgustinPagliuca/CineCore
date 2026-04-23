using System.ComponentModel.DataAnnotations;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CineCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El apellido es obligatorio.")]
            [Display(Name = "Apellido")]
            public string Apellido { get; set; } = string.Empty;

            [Phone(ErrorMessage = "El teléfono no es válido.")]
            [Display(Name = "Teléfono")]
            public string? Telefono { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Fecha de nacimiento")]
            public DateTime? FechaNacimiento { get; set; }

            [Required(ErrorMessage = "El email es obligatorio.")]
            [EmailAddress(ErrorMessage = "El email no es válido.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            IActionResult result;

            if (!ModelState.IsValid)
            {
                result = Page();
            }
            else
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Nombre = Input.Nombre,
                    Apellido = Input.Apellido,
                    Telefono = Input.Telefono,
                    FechaNacimiento = Input.FechaNacimiento,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, Input.Password);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    result = Page();
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, Roles.Cliente);
                    _logger.LogInformation("Nuevo cliente registrado: {Email}", user.Email);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    result = LocalRedirect(returnUrl);
                }
            }

            return result;
        }
    }
}