using System.ComponentModel.DataAnnotations;
using CineCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CineCore.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Phone(ErrorMessage = "Ingresá un número de teléfono válido.")]
            [Display(Name = "Teléfono")]
            public string? PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName ?? string.Empty;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
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

        public async Task<IActionResult> OnPostAsync()
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
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (Input.PhoneNumber != phoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        StatusMessage = "Error al actualizar el número de teléfono.";
                        result = RedirectToPage();
                    }
                    else
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        StatusMessage = "Tu perfil fue actualizado correctamente.";
                        result = RedirectToPage();
                    }
                }
                else
                {
                    await _signInManager.RefreshSignInAsync(user);
                    StatusMessage = "Tu perfil fue actualizado correctamente.";
                    result = RedirectToPage();
                }
            }

            return result;
        }
    }
}