﻿using System;
using System.Data.Entity.Core;
using System.Data.Objects;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using PagedList;
using PagedList.Mvc;

namespace QLVB.Controllers
{
    [Authorize(Roles = "QL-LOAIVB")]
    public class QL_DMCongTruongController : Controller
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
            return View(db.LoaiCongTruongs.ToList().ToPagedList(pageNumber, pageSize));
        }


        [HttpPost]
        public ActionResult ThemMoi(LoaiCongTruong loaitl)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            if (ModelState.IsValid)
            {
                db.LoaiCongTruongs.Add(loaitl);
                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Tạo loại văn bản thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public ActionResult ChinhSua(LoaiCongTruong loaitl)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            if (ModelState.IsValid)
            {
                db.Entry(loaitl).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["thongbao"] = "<script>$('#div-pthongbao').text('Cập nhật thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }

        public ActionResult Xoa(int id)
        {
            LoaiCongTruong xoaLoaiTL = db.LoaiCongTruongs.SingleOrDefault(n => n.MaLoaiCT == id);
            db.LoaiCongTruongs.Remove(xoaLoaiTL);
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