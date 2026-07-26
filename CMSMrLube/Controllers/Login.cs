using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MrLubeCMS.Models;

namespace MrLubeCMS.Controllers
{
    public class Login : Controller
    {

        const string Sessions = "_Login";
        const string SessionAge = "_password";
        private readonly UserDbContextcs _context;

        public Login(UserDbContextcs context)
        {
            _context = context;
        }

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Logins()
        {
            HttpContext.Session.Remove(Sessions);
            HttpContext.Session.Clear();
            //HttpContext.Session.Keys("");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Logins(UserLoginModel userLoginModel)
        {
            if (ModelState.IsValid)
            {

                _context.Add(userLoginModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userLoginModel);
        }

        // GET: Login/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Login/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Login/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Login/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Login/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Login/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Login/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
