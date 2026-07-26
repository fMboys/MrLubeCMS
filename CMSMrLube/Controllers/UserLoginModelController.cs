using System.Security.Claims;
using CMS.Core.Entities;
using CMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MrLubeCMS.CustomClasses;

namespace MrLubeCMS.Controllers
{
    [Authorize]
    public class UserLoginModelController : Controller
    {
        private readonly CMSDbContext _context;
        const string SessionName = "_admin";
        users_manage users;

        public UserLoginModelController(CMSDbContext context)
        {
            _context = context;
        }

        // GET: UserLoginModel
        
        public async Task<IActionResult> Index()
        {
            //return _context.users_manage != null ? 
            //            View(await _context.users_manage.ToListAsync()) :
            //            Problem("Entity set 'CMSDbContext.users_manage'  is null.");
            return View();
        }

        // GET: UserLoginModel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.users_manages == null)
            {
                return NotFound();
            }

            var userLoginModel = await _context.users_manages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userLoginModel == null)
            {
                return NotFound();
            }

            return View(userLoginModel);
        }

        // GET: UserLoginModel/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserLoginModel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Login,Password,Email,Phone,Status,last_user,date_created,date_updated")] users_manage userLoginModel)
        {
            if (ModelState.IsValid)
            {
                _context.users_manages.Add(userLoginModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userLoginModel);
        }

        // GET: UserLoginModel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.users_manages == null)
            {
                return NotFound();
            }

            var userLoginModel = await _context.users_manages.FindAsync(id);
            if (userLoginModel == null)
            {
                return NotFound();
            }
            return View(userLoginModel);
        }

        // POST: UserLoginModel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Login,Password,Email,Phone,Status,last_user,date_created,date_updated")] users_manage userLoginModel)
        {
            if (id != userLoginModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.users_manages.Add(userLoginModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserLoginModelExists(userLoginModel.Id))
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
            return View(userLoginModel);
        }

        // GET: UserLoginModel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.users_manages == null)
            {
                return NotFound();
            }

            var userLoginModel = await _context.users_manages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userLoginModel == null)
            {
                return NotFound();
            }

            return View(userLoginModel);
        }

        // POST: UserLoginModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.users_manages == null)
            {
                return Problem("Entity set 'CMSDbContext.users_manage'  is null.");
            }
            var userLoginModel = await _context.users_manages.FindAsync(id);
            if (userLoginModel != null)
            {
                _context.users_manages.Remove(userLoginModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserLoginModelExists(int id)
        {
          return (_context.users_manages?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        //ADK login
        public async Task<IActionResult> Login(users_manage userLoginModel)
        {
           
            if (ModelState.IsValid)
            {
                try
                {
                    if (userLoginModel.Password == null) { userLoginModel.Password = string.Empty; }
                    userLoginModel.Password = Encryption.Encrypt(userLoginModel.Password, "Admin@123");

                    users = _context.users_manages.SingleOrDefault(usr => ((usr.Email == userLoginModel.Email) && (usr.Password == userLoginModel.Password)));
                    if (users != null)
                    {
                        var userClaim = new List<Claim>()
                        {
                            new Claim("admin",users.Login),
                            new Claim(ClaimTypes.Email,users.Email),
                            new Claim(ClaimTypes.Role, "admin")
                        };
                        
                        var userIdentity = new ClaimsIdentity(userClaim,"User Identity");
                        var userPrincipal = new ClaimsPrincipal(new[] { userIdentity });
                        HttpContext.SignInAsync(userPrincipal);
                        HttpContext.Session.SetString("user", userLoginModel.Email);
                        ViewBag.userEmail= users;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["message"] = "Invalid User Or Password..!";
                        return RedirectToAction("Index", "UserLoginModel");
                    }

                    //_context.Update(userLoginModel);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserLoginModelExists(userLoginModel.Id))
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
            return View(userLoginModel);
        }

        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Items.Remove(User);
            HttpContext.Session.Remove("MrLubeCMS");
            HttpContext.SignOutAsync();
            
            return RedirectToAction("Index");
;        }
    }
}
