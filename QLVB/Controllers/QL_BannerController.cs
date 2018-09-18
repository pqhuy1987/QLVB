using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using PagedList;
using PagedList.Mvc;
using System.IO;

namespace QLVB.Controllers
{
    [Authorize(Roles = "QL-BANNER")]
    public class QL_BannerController : Controller
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
            return View(db.Banners.OrderBy(n=>n.Order).ToList().ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult ThemMoi(Banner bn, HttpPostedFileBase ImageBanner)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");         

            if (ModelState.IsValid)
            {
              
                //luu hinh banner
                if (ImageBanner != null)
                {
                    string sFileName = Tools.md5(DateTime.Now.ToString());
                    var fileName = Path.GetFileName(ImageBanner.FileName);
                    sFileName += fileName.Substring(fileName.LastIndexOf('.'));
                    var path = Path.Combine(Server.MapPath("~/Upload/"), sFileName);
                    ImageBanner.SaveAs(path);

                    bn.Image = sFileName;
                }

                db.Banners.Add(bn);
                db.SaveChanges();
                TempData["thongbao"] = "<script>$('#div-pthongbao').text('Tạo mới thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public ActionResult ChinhSua(Banner bn, HttpPostedFileBase ImageBanner)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            
            if (ModelState.IsValid)
            {   
                //luu hinh banner
                if (ImageBanner != null)
                {
                    string sFileName = Tools.md5(DateTime.Now.ToString());
                    var fileName = Path.GetFileName(ImageBanner.FileName);
                    sFileName += fileName.Substring(fileName.LastIndexOf('.'));
                    var path = Path.Combine(Server.MapPath("~/Upload/"), sFileName);
                    ImageBanner.SaveAs(path);

                    bn.Image = sFileName;
                }

                db.Entry(bn).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["thongbao"] = "<script>$('#div-pthongbao').text('Cập nhật thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }

     
        public ActionResult Xoa(int id)
        {
            Banner xoaBanner = db.Banners.SingleOrDefault(n => n.Id == id);
            db.Banners.Remove(xoaBanner);
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