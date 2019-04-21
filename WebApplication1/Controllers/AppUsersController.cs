using HZC.MyOrm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;
using HZC.MyOrm.Commons;
using HZC.MyOrm.Expressions;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "admin")]
    public class AppUsersController : Controller
    {
        private readonly MyDb _db = new MyDb();

        public async Task<ActionResult> Index()
        {
            var expr = LinqExtensions.True<AppUser>();
            expr = expr.And(u => !u.IsDelete);

            var data = await _db.Query<AppUser>()
                .Where(expr)
                .Select<AppUserListDto>(u => new AppUserListDto
                {
                    Id = u.Id,
                    No = u.No,
                    Name = u.Name,
                    DepartmentName = u.Department.Name
                }).ToListAsync();
            return View(data);
        }

        // GET: AppUsers/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var entity = await _db.Query<AppUser>().Include(u => u.Department).Where(u => u.Id == id).FirstOrDefaultAsync();
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        // GET: AppUsers/Create
        public async Task<ActionResult> Create()
        {
            var entity = new AppUser();
            await InitUi();
            return View(entity);
        }

        // POST: AppUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            var entity = new AppUser();
            await TryUpdateModelAsync(entity);

            if (ModelState.IsValid)
            {
                try
                {
                    var result = _db.InsertIfNotExists(entity, u => u.No == entity.No);
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError(string.Empty, "创建失败");
                }
                catch(Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            await InitUi();
            return View(entity);
        }

        // GET: AppUsers/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var entity = _db.Load<AppUser>(id);
            if (entity == null)
            {
                return NotFound();
            }

            await InitUi();
            return View(entity);
        }

        // POST: AppUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            var entity = new AppUser();
            await TryUpdateModelAsync(entity);

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _db.UpdateIfNotExitsAsync(entity, u => u.No == entity.No && u.Id != entity.Id);
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "更新失败");
                    }
                }
                catch(Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            await InitUi();
            return View(entity);
        }

        // GET: AppUsers/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _db.Query<AppUser>().Where(u => u.Id == id).Include(u => u.Department).FirstOrDefaultAsync();
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        // POST: AppUsers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            var entity = new AppUser();
            await TryUpdateModelAsync(entity);
            try
            {
                var result = await _db.UpdateAsync<AppUser>(id, DbKvs.New().Add("IsDelete", true));

                if (result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "删除失败");
                }
            }
            catch(Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
            }

            return View(entity);
        }

        private async Task InitUi()
        {
            var departments = await _db.Query<Department>().ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
        }
    }
}