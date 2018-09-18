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
    [Authorize(Roles = "QL-TAI-KHOAN")]
    public class QL_TaiKhoanController : Controller
    {
        QuanLyVanBanEntities db = new QuanLyVanBanEntities();
        
        public bool KiemTraSession()
        {
            if (Session["DangNhap"] == null )
            {
               return true;
            }
            return false;
        }
        public ActionResult DanhSach(int? page, string HoTen, string DienThoai, string Email)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            ViewBag.HoTen = HoTen;
            ViewBag.DienThoai = DienThoai;
            ViewBag.Email = Email;
            IEnumerable<NhanVien> lstNhanVien = db.NhanViens;

            if (!string.IsNullOrEmpty(HoTen))
                lstNhanVien = lstNhanVien.Where(n => (n.HoTen != null && Tools.RemoveDiacritics(n.HoTen).Contains(Tools.RemoveDiacritics(HoTen.Trim())) ));

            if (!string.IsNullOrEmpty(DienThoai))
                lstNhanVien = lstNhanVien.Where(n => (n.DienThoai != null && n.DienThoai==DienThoai.Trim()));

            if (!string.IsNullOrEmpty(Email))
                lstNhanVien = lstNhanVien.Where(n => (n.Email != null && n.Email.ToLower().Contains(Email.ToLower().Trim())));

            return View(lstNhanVien.Where(n => n.TenDangNhap!="1").ToList().ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult DanhSach(FormCollection c)
        {
            string sHoTen = c["HoTen"];
            string sDienThoai = c["DienThoai"];
            string sEmail = c["Email"];

            return RedirectToAction("DanhSach", "QL_TaiKhoan", new { @HoTen = sHoTen, @DienThoai = sDienThoai, @Email = sEmail });
        }

        public ActionResult ThemMoi()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            ViewBag.NhomTV = db.NhomTVs.Where(n => n.MaNhom != 1).OrderBy(n => n.TenNhom);

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemMoi(NhanVien nv,FormCollection c,string[] Nhom)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            ViewBag.NhomTV = db.NhomTVs.Where(n => n.MaNhom != 1).OrderBy(n => n.TenNhom);

            int ktTonTai = db.NhanViens.Where(n => n.TenDangNhap == nv.TenDangNhap).Count();
            if (ktTonTai != 0)
            {
                ViewBag.ktTenDangNhap = "Tên đăng nhập đã tồn tại";
                return View();
            }
            if (!string.IsNullOrEmpty(nv.Email))
            {
                ktTonTai = db.NhanViens.Where(n => n.Email.ToLower().Trim() == nv.Email.ToLower().Trim()).Count();
                if (ktTonTai != 0)
                {
                    ViewBag.ktEmail = "Email đã tồn tại";
                    return View();
                }
            }
           
            if (ModelState.IsValid)
            {
                if (nv.Ldap == null)
                    nv.Ldap = false;

                nv.MatKhau = Tools.md5(nv.MatKhau);
                if (!string.IsNullOrEmpty(c["NgaySinhs"]))
                    nv.NgaySinh = DateTime.ParseExact(c["NgaySinhs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                db.NhanViens.Add(nv);

                // them tai khoan vao nhom
                if(Nhom!=null && Nhom.Length !=0)
                {
                    for(int i=0 ; i< Nhom.Length ; i++)
                    {
                        Nhom_Nhanvien themN_NV = new Nhom_Nhanvien();
                        themN_NV.IdNhanVien = nv.Id;
                        themN_NV.MaNhom = int.Parse(Nhom[i]);
                        db.Nhom_Nhanvien.Add(themN_NV);
                    }
                }

                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Tạo tài khoản thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return View();
        }

        public ActionResult ChinhSua(int id)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            ViewBag.NhomTV = db.NhomTVs.Where(n => n.MaNhom != 1).OrderBy(n => n.TenNhom);
            ViewBag.Nhom_NV = db.Nhom_Nhanvien.Where(n => n.IdNhanVien == id);

            NhanVien nv = db.NhanViens.SingleOrDefault(n => n.Id == id);
            return View(nv);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ChinhSua(NhanVien nv,FormCollection c, string[] Nhom)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            ViewBag.NhomTV = db.NhomTVs.Where(n => n.MaNhom != 1).OrderBy(n => n.TenNhom);
            ViewBag.Nhom_NV = db.Nhom_Nhanvien.Where(n => n.IdNhanVien == nv.Id);

            NhanVien nvcu = db.NhanViens.AsNoTracking().SingleOrDefault(n => n.TenDangNhap == nv.TenDangNhap);

            if (nv.MatKhau != null) // xu ly mat khau
                nv.MatKhau = Tools.md5(nv.MatKhau);
            else
                nv.MatKhau = nvcu.MatKhau;

            if (nv.Ldap == null)
                nv.Ldap = false;

            if (!string.IsNullOrEmpty(nv.Email))
            {
                int ktTonTai = db.NhanViens.Where(n => n.Email.ToLower().Trim() == nv.Email.ToLower().Trim()).Count();
                if (ktTonTai != 0 && nv.Email.ToLower().Trim() != nvcu.Email.ToLower().Trim()) // kiem tra email co ton tai
                {
                    ViewBag.ktEmail = "Email đã tồn tại";
                    return View(nv);
                }
            }
            
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(c["NgaySinhs"]))
                    nv.NgaySinh = DateTime.ParseExact(c["NgaySinhs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                db.Entry(nv).State = System.Data.Entity.EntityState.Modified;

                IEnumerable<Nhom_Nhanvien> xoaN_NV = db.Nhom_Nhanvien.Where(n => n.IdNhanVien == nv.Id);
                db.Nhom_Nhanvien.RemoveRange(xoaN_NV);

                db.SaveChanges();

                // them tai khoan vao nhom
                if (Nhom != null && Nhom.Length != 0)
                {
                    for (int i = 0; i < Nhom.Length; i++)
                    {
                        Nhom_Nhanvien themN_NV = new Nhom_Nhanvien();
                        themN_NV.IdNhanVien = nv.Id;
                        themN_NV.MaNhom = int.Parse(Nhom[i]);
                        db.Nhom_Nhanvien.Add(themN_NV);
                    }
                }
                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Cập nhật tài khoản thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return View(nv);
        }

        public ActionResult Xoa(int id)
        {
            NhanVien xoaNhanVien = db.NhanViens.SingleOrDefault(n => n.Id == id);
            db.NhanViens.Remove(xoaNhanVien);
            db.SaveChanges();
            TempData["thongbao"] = "<script> $('#div-pthongbao').text('Xóa tài khoản thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            return RedirectToAction("DanhSach");
        }

        public ActionResult KichHoat(string id,bool id2)
        {
            NhanVien khNhanVien = db.NhanViens.SingleOrDefault(n => n.TenDangNhap == id);
            khNhanVien.KichHoat = id2;
            db.SaveChanges();
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