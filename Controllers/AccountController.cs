using Microsoft.AspNetCore.Mvc;
using app.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using app.Data;

namespace Assessment.Controllers
{
    public class AccountController : Controller
    {
        private readonly BaseDbContext _context;

        public AccountController(BaseDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Usuarios.SingleOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    var hashedPassword = PasswordHelper.HashPassword(model.Password, user.Salt);
                    if (hashedPassword == user.PasswordHash)
                    {
                        // Autenticación exitosa, crear claims y principal
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Email),
                            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                        // Redirigir a Home/Index
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Contraseña incorrecta
                        ModelState.AddModelError(string.Empty, "La contraseña es incorrecta.");
                    }
                }
                else
                {
                    // Correo no encontrado
                    ModelState.AddModelError(string.Empty, "El correo no ha sido registrado");
                }

                // Si llegamos aquí, la autenticación falló
                //ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
