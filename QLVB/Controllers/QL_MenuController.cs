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
    public class QL_MenuController : Controller
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
        public ActionResult DanhSach()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

           
            return View();
        }

        [HttpPost]
        public ActionResult ThemMoi(DMPhongBan pb)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");         

            if (ModelState.IsValid)
            {
                db.DMPhongBans.Add(pb);
                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#btn-dongy').hide(); $('#pthongbao').text('Tạo mới thành công'); $('#btn-thongbao').trigger('click');</script>";
            }
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public ActionResult ChinhSua(DMPhongBan pb)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            
            if (ModelState.IsValid)
            {
                db.Entry(pb).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#btn-dongy').hide(); $('#pthongbao').text('Cập nhật thành công'); $('#btn-thongbao').trigger('click');</script>";
            }
            return RedirectToAction("DanhSach");
        }

        public ActionResult Xoa(int id)
        {
            DMPhongBan xoaPhong = db.DMPhongBans.SingleOrDefault(n => n.Id == id);
            db.DMPhongBans.Remove(xoaPhong);
            db.SaveChanges();
            TempData["thongbao"] = "<script> $('#btn-dongy').hide(); $('#pthongbao').text('Xóa thành công'); $('#btn-thongbao').trigger('click');</script>";
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