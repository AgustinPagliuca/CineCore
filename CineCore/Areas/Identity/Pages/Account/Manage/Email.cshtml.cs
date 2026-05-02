using System.ComponentModel.DataAnnotations;
using CineCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CineCore.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Email { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessage = "El email es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingresá un email válido.")]
            [Display(Name = "Nuevo email")]
            public string NewEmail { get; set; } = string.Empty;
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email ?? string.Empty;

            Input = new InputModel
            {
                NewEmail = email ?? string.Empty
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
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
                await LoadAsync(user);
                result = Page();
            }

            return result;
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            IActionResult result;
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                result = NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }
            else if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                result = Page();
            }
            else
            {
                var email = await _userManager.GetEmailAsync(user);
                if (Input.NewEmail != email)
                {
                    // Sin SMTP configurado, cambiamos el email directamente.
                    // En un sistema real con envío de emails, se enviaría un link de confirmación.
                    var setEmailResult = await _userManager.SetEmailAsync(user, Input.NewEmail);
                    var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.NewEmail);

                    if (!setEmailResult.Succeeded || !setUserNameResult.Succeeded)
                    {
                        StatusMessage = "No se pudo cambiar el email.";
                        result = RedirectToPage();
                    }
                    else
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        StatusMessage = "Tu email fue cambiado correctamente.";
                        result = RedirectToPage();
                    }
                }
                else
                {
                    StatusMessage = "El email no cambió.";
                    result = RedirectToPage();
                }
            }

            return result;
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            IActionResult result;
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                result = NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }
            else if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                result = Page();
            }
            else
            {
                // Sin SMTP configurado, marcamos el email como confirmado directamente.
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);

                StatusMessage = "Email confirmado.";
                result = RedirectToPage();
            }

            return result;
        }
    }
}