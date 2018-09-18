using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using PagedList;
using PagedList.Mvc;

namespace QLVB.Controllers
{
    [Authorize(Roles = "QL-PHONG-HOP")]
    public class QL_DMPhongHopController : Controller
    {
        QuanLyVanBanEntities db = new QuanLyVanBanEntities();
        
        public bool KiemTraSession()
        {
            if (Session["DangNhap"] == null)
            {
               return true;
            }
            return false;
        }
        public ActionResult DanhSach(int? page)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(db.PhongHops.OrderBy(n=>n.Ordering).ToList().ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult ThemMoi(PhongHop ph)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");         

            if (ModelState.IsValid)
            {
                if (ph.Status == null)
                    ph.Status = false;
                db.PhongHops.Add(ph);
                db.SaveChanges();
                TempData["thongbao"] = "<script>$('#div-pthongbao').text('Tạo mới thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public ActionResult ChinhSua(PhongHop ph)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            
            if (ModelState.IsValid)
            {
                if (ph.Status == null)
                    ph.Status = false;
                db.Entry(ph).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["thongbao"] = "<script>$('#div-pthongbao').text('Cập nhật thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }

        public ActionResult KichHoat(int id, bool id2)
        {
            PhongHop dmPhong = db.PhongHops.SingleOrDefault(n => n.Id == id);
            dmPhong.Status = id2;
            db.SaveChanges();
            return RedirectToAction("DanhSach");
        }

        public ActionResult Xoa(int id)
        {
            PhongHop xoaPhong = db.PhongHops.SingleOrDefault(n => n.Id == id);
            db.PhongHops.Remove(xoaPhong);
            db.SaveChanges();
            TempData["thongbao"] = "<script> $('#div-pthongbao').text('Xóa thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            return RedirectToAction("DanhSach");
        }      

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
	}
}