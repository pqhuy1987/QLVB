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
    [Authorize(Roles = "QL-NHOM")]
    public class QL_NhomController : Controller
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
            return View(db.NhomTVs.Where(n=>n.MaNhom!=1).ToList().ToPagedList(pageNumber, pageSize));
        }


        public ActionResult ThemMoi()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            var vNhanVien = db.NhanViens.Where(n => n.Id != 1).Select(s => new { TenDangNhap = s.TenDangNhap, TenNhanVien = s.TenDangNhap + " (" + s.HoTen + ")" }).ToList();
            ViewBag.NhanVien = new SelectList(vNhanVien, "TenDangNhap", "TenNhanVien");
            ViewBag.Quyen = db.Quyens;
            IEnumerable<FAQ> lstFAQ= db.FAQs.OrderBy(n=>n.STT);
            sHtml = "";
            DuyetFAQ(0,lstFAQ,0,0);
            ViewBag.faq = sHtml;

            return View();
        }
        string sHtml = "";
        public void DuyetFAQ(int iMaFAQ, IEnumerable<FAQ> lstFAQ, int i, int idNhom)
        {
            if (i == 0)
            {
                sHtml += "<ul class='treeFAQ'>";
            }
            else
                sHtml += "<ul class='mn-child'>";
            foreach (var item in lstFAQ.Where(n => n.Parent_Id == iMaFAQ))
            {

                sHtml += "<li>";
                if(idNhom != 0 && KTNhom_FAQ(idNhom,item.Id))
                    sHtml += "<input type='checkbox' class='cheChild' name='idFAQ' checked value='" + item.Id + "' />";
                else
                    sHtml += "<input type='checkbox' class='cheChild' name='idFAQ' value='" + item.Id + "' />";
                sHtml += " <span class='mn-parent span-tieude'> " + item.CauHoi + " </span> ";
                sHtml += " <span class='span-soluong'></span> ";

                DuyetFAQ(item.Id, lstFAQ, 1,idNhom);
            }

            sHtml += "</ul>";
        }
        public bool KTNhom_FAQ(int idNhom,int idFAQ)
        {
            int iKT = db.Nhom_FAQ.Where(n => n.MaNhom == idNhom && n.MaFAQ == idFAQ).Count();
            if (iKT > 0)
                return true;
            return false;
        }
        [HttpPost]
        public ActionResult ThemMoi(NhomTV nhom, string[] TaiKhoan, string[] Quyen , string[] idFAQ)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            if (ModelState.IsValid)
            {
                if (nhom.MacDinh == null)
                    nhom.MacDinh = false;

                db.NhomTVs.Add(nhom);
                db.SaveChanges();

                if(TaiKhoan!=null && TaiKhoan.Length!=0) // them nhom_nhanvien
                {
                    for (int i = 0; i < TaiKhoan.Length; i++)
                    {
                        string sTaiKhoan = TaiKhoan[i];
                        NhanVien layNV = db.NhanViens.SingleOrDefault(n => n.TenDangNhap == sTaiKhoan);
                        int iKT=db.Nhom_Nhanvien.Where(n=>n.MaNhom==nhom.MaNhom && n.IdNhanVien==layNV.Id).Count();
                        if (iKT == 0)
                        {
                            Nhom_Nhanvien themN_NV = new Nhom_Nhanvien();
                            themN_NV.MaNhom = nhom.MaNhom;
                            themN_NV.IdNhanVien = layNV.Id;
                            db.Nhom_Nhanvien.Add(themN_NV);                           
                        }
                    }      
                }

                if (Quyen !=null && Quyen.Length != 0) // them nhom_quyen
                {
                    for (int i = 0; i < Quyen.Length; i++)
                    {
                        string sQuyen = Quyen[i];
                        int iKT = db.Nhom_Quyen.Where(n => n.MaNhom == nhom.MaNhom && n.MaQuyen == sQuyen).Count();
                        if (iKT == 0)
                        {
                            Nhom_Quyen themN_Q = new Nhom_Quyen();
                            themN_Q.MaNhom = nhom.MaNhom;
                            themN_Q.MaQuyen = Quyen[i];
                            db.Nhom_Quyen.Add(themN_Q);
                        }
                    }
                }

                if (idFAQ != null && idFAQ.Length != 0) // them nhom_faq
                {
                    for (int i = 0; i < idFAQ.Length; i++)
                    {
                        int iidFaq = int.Parse(idFAQ[i]);
                        int iKT = db.Nhom_FAQ.Where(n => n.MaNhom == nhom.MaNhom && n.MaFAQ == iidFaq).Count();
                        if (iKT == 0)
                        {
                            Nhom_FAQ themN_FAQ = new Nhom_FAQ();
                            themN_FAQ.MaNhom = nhom.MaNhom;
                            themN_FAQ.MaFAQ = iidFaq;
                            db.Nhom_FAQ.Add(themN_FAQ);
                        }
                    }
                }

                db.SaveChanges();

                TempData["thongbao"] = "<script>$('#div-pthongbao').text('Tạo nhóm thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }
       
        public ActionResult ChinhSua(int id)
        {
            if(id==1)
                return RedirectToAction("Index", "TrangChinh");
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            NhomTV nhom = db.NhomTVs.SingleOrDefault(n => n.MaNhom == id);
            var vNhanVien = db.NhanViens.Where(n => n.Id != 1).Select(s => new { TenDangNhap = s.TenDangNhap, TenNhanVien = s.TenDangNhap + " (" + s.HoTen + ")" }).ToList();
            ViewBag.NhanVien = new SelectList(vNhanVien, "TenDangNhap", "TenNhanVien");
            ViewBag.Quyen = db.Quyens;

            ViewBag.nhom_nhanvien = db.Nhom_Nhanvien.Where(n => n.MaNhom == id);
            ViewBag.nhom_quyen = db.Nhom_Quyen.Where(n => n.MaNhom == id);

            //lay faq
            IEnumerable<FAQ> lstFAQ = db.FAQs.OrderBy(n => n.STT);
            sHtml = "";
            DuyetFAQ(0, lstFAQ, 0, id);
            ViewBag.faq = sHtml;

            return View(nhom);
        }
        [HttpPost]
        public ActionResult ChinhSua(NhomTV nhom, string[] TaiKhoan, string[] Quyen, string[] idFAQ)
         {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            if (ModelState.IsValid)
            {
                if (nhom.MacDinh == null)
                    nhom.MacDinh = false;

                db.Entry(nhom).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                //xoa du lieu cu
                IEnumerable<Nhom_Nhanvien> xoaN_NV = db.Nhom_Nhanvien.Where(n => n.MaNhom == nhom.MaNhom);
                db.Nhom_Nhanvien.RemoveRange(xoaN_NV);
                IEnumerable<Nhom_Quyen> xoaN_Q = db.Nhom_Quyen.Where(n => n.MaNhom == nhom.MaNhom);
                db.Nhom_Quyen.RemoveRange(xoaN_Q);
                IEnumerable<Nhom_FAQ> xoaN_FAQ = db.Nhom_FAQ.Where(n => n.MaNhom == nhom.MaNhom);
                db.Nhom_FAQ.RemoveRange(xoaN_FAQ);
                db.SaveChanges();

                if(TaiKhoan!=null && TaiKhoan.Length!=0) // them nhom_nhanvien
                {
                    for (int i = 0; i < TaiKhoan.Length; i++)
                    {
                        string sTaiKhoan = TaiKhoan[i];
                        NhanVien layNV = db.NhanViens.SingleOrDefault(n => n.TenDangNhap == sTaiKhoan);
                        int iKT=db.Nhom_Nhanvien.Where(n=>n.MaNhom==nhom.MaNhom && n.IdNhanVien==layNV.Id).Count();
                        if (iKT == 0)
                        {
                            Nhom_Nhanvien themN_NV = new Nhom_Nhanvien();
                            themN_NV.MaNhom = nhom.MaNhom;
                            themN_NV.IdNhanVien = layNV.Id;
                            db.Nhom_Nhanvien.Add(themN_NV);                           
                        }
                    }      
                }

                if (Quyen !=null && Quyen.Length != 0) // them nhom_quyen
                {
                    for (int i = 0; i < Quyen.Length; i++)
                    {
                        string sQuyen = Quyen[i];
                        int iKT = db.Nhom_Quyen.Where(n => n.MaNhom == nhom.MaNhom && n.MaQuyen == sQuyen).Count();
                        if (iKT == 0)
                        {
                            Nhom_Quyen themN_Q = new Nhom_Quyen();
                            themN_Q.MaNhom = nhom.MaNhom;
                            themN_Q.MaQuyen = Quyen[i];
                            db.Nhom_Quyen.Add(themN_Q);
                        }
                    }
                }

                if (idFAQ != null && idFAQ.Length != 0) // them nhom_faq
                {
                    for (int i = 0; i < idFAQ.Length; i++)
                    {
                        int iidFaq = int.Parse(idFAQ[i]);
                        int iKT = db.Nhom_FAQ.Where(n => n.MaNhom == nhom.MaNhom && n.MaFAQ == iidFaq).Count();
                        if (iKT == 0)
                        {
                            Nhom_FAQ themN_FAQ = new Nhom_FAQ();
                            themN_FAQ.MaNhom = nhom.MaNhom;
                            themN_FAQ.MaFAQ = iidFaq;
                            db.Nhom_FAQ.Add(themN_FAQ);
                        }
                    }
                }
                db.SaveChanges();

                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Cập nhật thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DanhSach");
        }

        public ActionResult Xoa(int id)
        {
            if (id == 1)
                return RedirectToAction("Index", "TrangChinh");

            NhomTV xoaNhom = db.NhomTVs.SingleOrDefault(n => n.MaNhom == id);
            db.NhomTVs.Remove(xoaNhom);
            db.SaveChanges();
            TempData["thongbao"] = "<script>$('#div-pthongbao').text('Xóa thành công!'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
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