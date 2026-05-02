using System.ComponentModel.DataAnnotations;
using CineCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CineCore.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña actual")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
            [StringLength(100, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nueva contraseña")]
            public string NewPassword { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nueva contraseña")]
            [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            IActionResult result;
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                result = NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }
            else
            {
                var hasPassword = await _userManager.HasPasswordAsync(user);
                if (!hasPassword)
                {
                    result = RedirectToPage("./SetPassword");
                }
                else
                {
                    result = Page();
                }
            }

            return result;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IActionResult result;

            if (!ModelState.IsValid)
            {
                result = Page();
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    result = NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
                }
                else
                {
                    var changePasswordResult = await _userManager.ChangePasswordAsync(
                        user,
                        Input.OldPassword,
                        Input.NewPassword);

                    if (!changePasswordResult.Succeeded)
                    {
                        foreach (var error in changePasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        result = Page();
                    }
                    else
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        _logger.LogInformation("Usuario cambió su contraseña.");
                        StatusMessage = "Tu contraseña fue cambiada correctamente.";
                        result = RedirectToPage();
                    }
                }
            }

            return result;
        }
    }
}