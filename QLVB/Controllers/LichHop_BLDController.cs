using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Net.Mail;
using System.Net;

namespace QLVB.Controllers
{
    [Authorize]
    public class LichHop_BLDController : Controller
    { 
         private Scheduler_BLDTaskService taskService;

         QuanLyVanBanEntities db = new QuanLyVanBanEntities();

        public bool KiemTraSession()
        {
            if (Session["DangNhap"] == null)
            {
                return true;
            }
            return false;
        }
        public LichHop_BLDController()
        {
            this.taskService = new Scheduler_BLDTaskService();
        }

        public ActionResult DangKy()
        {
             if (KiemTraSession() == true)
               return RedirectToAction("DangNhap", "QuanTri");

            ViewBag.DMBanLanhDao = db.DMBanLanhDaos.OrderBy(n => n.Name);

            return View();
        }

        public virtual JsonResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(taskService.GetAll().ToDataSourceResult(request));
        }

        public virtual JsonResult Destroy([DataSourceRequest] DataSourceRequest request, TaskViewModel task)
        {
            NhanVien nv = (NhanVien)Session["DangNhap"];

            if (ModelState.IsValid )
            {
               taskService.Delete(task, ModelState);
            }

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        public virtual ActionResult Create([DataSourceRequest] DataSourceRequest request, TaskViewModel task)
        {
            
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");
           

            if (ModelState.IsValid )
            {

                taskService.Insert(task, ModelState);
            }


            return Json(new[] { task }.ToDataSourceResult(request, ModelState));

        }

        public virtual ActionResult Update([DataSourceRequest] DataSourceRequest request, TaskViewModel task)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            if (ModelState.IsValid )
            {
                task.UpdatedDate = DateTime.Now;
                taskService.Update(task, ModelState);
            }

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

     

        protected override void Dispose(bool disposing)
        {
            taskService.Dispose();
            base.Dispose(disposing);
        }
	}
}