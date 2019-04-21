using HZC.MyOrm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "admin")]
    public class DepartmentsController : Controller
    {
        private readonly MyDb _db = MyDb.New();

        public async Task<ActionResult> Index()
        {
            var data = await _db.Query<Department>().ToListAsync();
            return View(data);
        }

        // GET: Department/Details/5
        public ActionResult Details(int id)
        {
            var entity = _db.Load<Department>(id);
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            var entity = new Department();
            return View(entity);
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            var entity = new Department();
            await TryUpdateModelAsync(entity);

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _db.InsertIfNotExistsAsync(entity, d => d.Name == entity.Name);
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "创建失败");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(entity);
        }

        // GET: Department/Edit/5
        public ActionResult Edit(int id)
        {
            var entity = _db.Load<Department>(id);
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            var entity = new Department();
            await TryUpdateModelAsync(entity);

            if (ModelState.IsValid)
            {
                try
                {
                    var result =
                        await _db.UpdateIfNotExitsAsync(entity, d => d.Name == entity.Name && d.Id != entity.Id);
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "更新失败");
                    }
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(entity);
        }

        // GET: Department/Delete/5
        public ActionResult Delete(int id)
        {
            var entity = _db.Load<Department>(id);
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        // POST: Department/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            var entity = new Department();
            await TryUpdateModelAsync(entity);

            try
            {
                var result = await _db.DeleteAsync<Department>(id);
                if (result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "删除失败");
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(entity);
        }
    }
}