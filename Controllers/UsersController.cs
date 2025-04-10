using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using symitekstil.Data;
using symitekstil.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace symitekstil.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("HATA: Kullanıcı adı veya şifre boş bırakıldı.");
                ModelState.AddModelError(string.Empty, "Kullanıcı adı ve şifre gereklidir.");
                return View();
            }

            Console.WriteLine($"Giriş denemesi: Kullanıcı adı: {username}, Şifre: {password}");

            // Veritabanından kullanıcıyı arayalım
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.Password == password);

            if (user == null)
            {
                Console.WriteLine($"Giriş başarısız: Kullanıcı bulunamadı veya şifre yanlış. Kullanıcı Adı: {username}, Şifre: {password}");
                Console.WriteLine("Veritabanındaki kullanıcılar:");
                foreach (var dbUser in await _context.Users.ToListAsync())
                {
                    Console.WriteLine($"DB Kullanıcı Adı: {dbUser.Username}, DB Şifre: {dbUser.Password}");
                }

                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                return View();
            }

            // Kullanıcı kimlik doğrulama
            Console.WriteLine($"Giriş başarılı: Kullanıcı Adı: {user.Username}, Rol: {user.UserType}");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.UserType) // Kullanıcı türü (Admin veya User)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Rol bazlı yönlendirme
            if (user.UserType == "Admin")
            {
                return RedirectToAction("AdminPanel", "Users");
            }

            return RedirectToAction("ApplicationPanel", "Users");
        }

        // GET: Logout
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Users");
        }

        // GET: AdminPanel
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel()
        {
            return View();
        }

        // GET: ApplicationPanel
        [Authorize]
        public IActionResult ApplicationPanel()
        {
            return View();
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,Username,Password,Email,UserType")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user); // Şifreyi düz metin olarak kaydediyoruz.
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserID,Username,Password,Email,UserType")] User user)
        {
            if (id != user.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user); // Şifreyi düz metin olarak saklıyoruz.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}