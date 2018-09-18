using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using PagedList;
using PagedList.Mvc;
using System.Web.Security;
using System.IO;

namespace QLVB.Controllers
{
    public class ND_TaiKhoanController : Controller
    {
        QuanLyVanBanEntities db = new QuanLyVanBanEntities();

        [HttpPost]
        public ActionResult DangNhap(string TenDangNhap, string MatKhau)
        {
            CauHinh chkTenDanhNhap = db.CauHinhs.SingleOrDefault(n => n.MaCauHinh == "EMAIL-CONGTY");
            NhanVien ktEmail = db.NhanViens.SingleOrDefault(n => n.Email == TenDangNhap);
            if (ktEmail == null)
            {
                if (TenDangNhap.Contains(chkTenDanhNhap.DuLieu) == true)
                {
                    NhanVien themNV = new NhanVien();
                    themNV.HoTen = TenDangNhap;
                    themNV.TenDangNhap = TenDangNhap;
                    themNV.Email = TenDangNhap;
                    themNV.MatKhau = Tools.md5(TenDangNhap);
                    themNV.KichHoat = true;
                    db.NhanViens.Add(themNV);
                    db.SaveChanges();

                    Session["DangNhap"] = themNV;
                    return Content("1");
                }
            }
            string md5MatKhau = Tools.md5(MatKhau);
            NhanVien ktNV = db.NhanViens.SingleOrDefault(n => (n.Email == TenDangNhap || n.TenDangNhap == TenDangNhap) && n.MatKhau == md5MatKhau);
            if (ktNV != null)
            {
                if (ktNV.KichHoat.Value == false)
                {
                    return Content("2");
                }
                Session["DangNhap"] = ktNV;

                string sRoles = "";
                //gán quyền
                //lay nhom tu tai khoan
                IEnumerable<Nhom_Nhanvien> ieNhanVienVaNhom = db.Nhom_Nhanvien.Where(n => n.IdNhanVien == ktNV.Id);
                foreach (var item in ieNhanVienVaNhom)
                {
                    //lay quyen tu nhom
                    IEnumerable<Nhom_Quyen> ieNhomVaVaiTro = db.Nhom_Quyen.Where(n => n.MaNhom == item.MaNhom);
                    //them quyen
                    foreach (var item2 in ieNhomVaVaiTro)
                    {
                        sRoles += item2.MaQuyen + ",";
                    }
                }
                //bo dau , cuoi
                if (sRoles.Length != 0)
                    sRoles = sRoles.Substring(0, sRoles.Length - 1);
                GanQuyen(ktNV.Id.ToString(), sRoles);

                return Content("1");
            }

            return Content("0");
        }
        public void GanQuyen(string sUserName, string sRoles)
        {

            FormsAuthentication.Initialize();

            var ticket = new FormsAuthenticationTicket(1,
                                          sUserName, //user
                                          DateTime.Now, //begin
                                          DateTime.Now.AddYears(20), //timeout
                                          false, //remember?
                                          Tools.NenChuoi(sRoles), // permission.. "admin" or for more than one  "admin,marketing,sales"
                                          FormsAuthentication.FormsCookiePath);
            // int abc=System.Text.ASCIIEncoding.ASCII.GetByteCount(FormsAuthentication.Encrypt(ticket));
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));

            if (ticket.IsPersistent) cookie.Expires = ticket.Expiration;

            Response.Cookies.Add(cookie);
        }
        // GET: ND_TaiKhoan
        [HttpGet]
        public ActionResult DangKy()
        {

            return View();
        }
        [HttpGet]
        public ActionResult ThongTinTaiKhoan()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("index", "trangchinh");
            NhanVien nv = (NhanVien)Session["DangNhap"];
            return View(nv);
        }
        [HttpPost]
        public ActionResult ThongTinTaiKhoan(NhanVien nv,FormCollection c)
        {
            int ktTonTai = db.NhanViens.Where(n => n.Email.ToLower().Trim() == nv.Email.ToLower().Trim() && n.TenDangNhap!=nv.TenDangNhap).Count();

            NhanVien nvupdate = db.NhanViens.Single(n => n.TenDangNhap == nv.TenDangNhap);
            nvupdate.Ldap = nvupdate.Ldap;
            nvupdate.HoTen = nv.HoTen;
            nvupdate.NgaySinh = nv.NgaySinh;
            if (!string.IsNullOrEmpty(c["NgaySinhs"]))
            {
                nvupdate.NgaySinh = DateTime.ParseExact(c["NgaySinhs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                nv.NgaySinh = nvupdate.NgaySinh;
            }
            nvupdate.DiaChi = nv.DiaChi;
            nvupdate.Email = nv.Email;
            nvupdate.DienThoai = nv.DienThoai;

            db.SaveChanges();
            TempData["thongbao1"] = "<script> $('#pthongbao1').text('Thay đổi thông tin thành công'); $('#btn-thongbao1').trigger('click');</script>";
            Session["DangNhap"] = nvupdate;
            return View(nvupdate);
        }
        [HttpPost]
        public ActionResult DangKy(NhanVien nv,FormCollection c)
        {
            int ktTonTai = db.NhanViens.Where(n => n.TenDangNhap == nv.TenDangNhap).Count();
            if (ktTonTai != 0)
            {
                ViewBag.ktTenDangNhap = "Tên đăng nhập đã tồn tại";
                return View(nv);
            }
            ktTonTai = db.NhanViens.Where(n => n.Email.ToLower().Trim() == nv.Email.ToLower().Trim()).Count();
            if (ktTonTai != 0)
            {
                ViewBag.ktEmail = "Email đã tồn tại";
                return View(nv);
            }
            if (ModelState.IsValid)
            {
                nv.KichHoat = false;
                nv.MatKhau = Tools.md5(nv.MatKhau);
                if (!string.IsNullOrEmpty(c["NgaySinhs"]))
                    nv.NgaySinh = DateTime.ParseExact(c["NgaySinhs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                db.NhanViens.Add(nv);
                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#pthongbao').text('Tạo tài khoản thành công'); $('#btn-thongbao').trigger('click');</script>";
                Session["DangNhap"] = nv;
            }
            return RedirectToAction("index","trangchinh");
        }
        [HttpGet]
        public ActionResult DoiMatKhau(string MatKhauCu,string  MatKhauMoi)
        {
            if (KiemTraSession() == true)
                return Content("NO");
            NhanVien nv = (NhanVien)Session["DangNhap"];
            string MatKhauXN = Tools.md5(MatKhauCu);
            NhanVien nvUpdate = db.NhanViens.SingleOrDefault(n => n.TenDangNhap == nv.TenDangNhap&&n.MatKhau==MatKhauXN);
            if (nvUpdate == null)
            {
                return Content("NO");
            }
            nvUpdate.MatKhau = Tools.md5(MatKhauMoi);
            db.SaveChanges();
            return Content("OK");
        }
        public ActionResult DangXuat()
        {
            Session["DangNhap"] = null;
            FormsAuthentication.SignOut();
            System.Collections.IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                HttpRuntime.Cache.Remove(enumerator.Key.ToString());
            }
            return RedirectToAction("index", "trangchinh");
        }

        // van ban luu tru ca nhan
        public ActionResult VanBanLuu(int? page, string LoaiVanBan, string NgayBanHanhfrom, string NgayBanHanhto, string NgayHieuLucfrom, string NgayHieuLucto)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("index", "trangchinh");

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            NhanVien nv = (NhanVien)Session["DangNhap"];
            ViewBag.LoaiVanBan = new SelectList(db.LoaiTaiLieux, "MaLoaiTL", "TenLoaiTL", LoaiVanBan);

            IEnumerable<VBLuuTru> lstVBLT = db.VBLuuTrus.Where(n => n.IdNhanVien == nv.Id);
            ViewBag.LVB = db.LoaiTaiLieux;
            ViewBag.lstVanBanLuu = db.VBLuuTrus.Where(n => n.IdNhanVien == nv.Id);

            ViewBag.NgayBanHanhfrom = NgayBanHanhfrom;
            ViewBag.NgayBanHanhto = NgayBanHanhto;
            ViewBag.NgayHieuLucfrom = NgayHieuLucfrom;
            ViewBag.NgayHieuLucto = NgayHieuLucto;
            ViewBag.LoaiVanBani = LoaiVanBan;

            if (!string.IsNullOrEmpty(NgayBanHanhfrom) && !string.IsNullOrEmpty(NgayBanHanhto))
                lstVBLT = lstVBLT.Where(n => n.TaiLieu.NgayBanHanh != null && (n.TaiLieu.NgayBanHanh.Value >= DateTime.ParseExact(NgayBanHanhfrom, "dd/MM/yyyy", null) && n.TaiLieu.NgayBanHanh.Value <= DateTime.ParseExact(NgayBanHanhto, "dd/MM/yyyy", null)));

            if (!string.IsNullOrEmpty(NgayHieuLucfrom) && !string.IsNullOrEmpty(NgayHieuLucto))
                lstVBLT = lstVBLT.Where(n => n.TaiLieu.NgayHieuLuc != null && (n.TaiLieu.NgayHieuLuc.Value >= DateTime.ParseExact(NgayHieuLucfrom, "dd/MM/yyyy", null) && n.TaiLieu.NgayHieuLuc.Value <= DateTime.ParseExact(NgayHieuLucto, "dd/MM/yyyy", null)));

            if (!string.IsNullOrEmpty(LoaiVanBan))
            {
                int iLoaiVanBan = int.Parse(LoaiVanBan);
                lstVBLT = lstVBLT.Where(n => (n.TaiLieu.LoaiVanBan != null && n.TaiLieu.LoaiVanBan.Value == iLoaiVanBan));
            }


            return View(lstVBLT.ToList().ToPagedList(pageNumber, pageSize));
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult VanBanLuu(FormCollection c)
        {
            string sLoaiVanBan = c["LoaiVanBan"];
            string sNgayBanHanhfrom = c["NgayBanHanhfrom"];
            string sNgayBanHanhto = c["NgayBanHanhto"];
            string sNgayHieuLucfrom = c["NgayHieuLucfrom"];
            string sNgayHieuLucto = c["NgayHieuLucto"];

            return RedirectToAction("VanBanLuu", "ND_TaiKhoan", new { @LoaiVanBan = sLoaiVanBan, @NgayBanHanhfrom = sNgayBanHanhfrom, @NgayBanHanhto = sNgayBanHanhto, @NgayHieuLucfrom = sNgayHieuLucfrom, @NgayHieuLucto = sNgayHieuLucto });
        }

        public ActionResult XoaVBLTtheoID(int id)
        {
            if (Session["DangNhap"] != null)
            {
                VBLuuTru xoaVBLT = db.VBLuuTrus.SingleOrDefault(n => n.Id==id);
                db.VBLuuTrus.Remove(xoaVBLT);
                db.SaveChanges();

                TempData["thongbao"] = "<script> $('#btn-dongy').hide(); $('#pthongbao').text('Đã xóa khỏi văn bản cá nhân !'); $('#btn-thongbao2').trigger('click');</script>";
            }

            return RedirectToAction("VanBanLuu");
        }

        public ActionResult XoaVBLTtheoLoai(int id)
        {
            if (Session["DangNhap"] != null)
            {
                NhanVien nv = (NhanVien)Session["DangNhap"];
                IEnumerable<VBLuuTru> xoaVBLT = db.VBLuuTrus.Where(n => n.TaiLieu.LoaiVanBan.Value == id && n.IdNhanVien==nv.Id);
                db.VBLuuTrus.RemoveRange(xoaVBLT);
                db.SaveChanges();

                TempData["thongbao"] = "<script> $('#btn-dongy').hide(); $('#pthongbao').text('Đã xóa khỏi văn bản cá nhân !'); $('#btn-thongbao2').trigger('click');</script>";
            }

            return RedirectToAction("VanBanLuu");
        }

        [HttpPost]
        public ActionResult saveimgDaiDien_ajax() // save hinh danh muc
        {
            string subPath = "/Upload/Avatar/";

            bool exists = System.IO.Directory.Exists(Server.MapPath(subPath));

            if (!exists)
                System.IO.Directory.CreateDirectory(Server.MapPath(subPath));

            if (Session["DangNhap"] != null)
            {
                NhanVien nv = (NhanVien)Session["DangNhap"];
                string sFileName = nv.TenDangNhap;

                if (Request.Files.Count > 0)
                {
                    try
                    {
                        //  Get all files from Request object  
                        HttpFileCollectionBase files = Request.Files;
                        for (int i = 0; i < files.Count; i++)
                        {

                            HttpPostedFileBase file = files[i];
                            string fname;

                            // Checking for Internet Explorer  
                            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                            {
                                string[] testfiles = file.FileName.Split(new char[] { '\\' });
                                fname = testfiles[testfiles.Length - 1];
                            }
                            else
                            {
                                fname = file.FileName;
                            }
                            sFileName += fname.Substring(fname.LastIndexOf('.'));
                            // Get the complete folder path and store the file inside it.  
                            fname = Path.Combine(Server.MapPath("~/Upload/Avatar/"), sFileName);
                            file.SaveAs(fname);


                        }

                        // luu vao csdl
                        int iMaNhanVien = nv.Id;

                        NhanVien editNV = db.NhanViens.SingleOrDefault(n => n.Id == iMaNhanVien);

                        string KTAnh = editNV.HinhDaiDien;

                        editNV.HinhDaiDien = sFileName;
                        db.SaveChanges();

                        if (string.IsNullOrEmpty(KTAnh))
                        {
                            return Json("-1");
                        }

                        // Returns message that successfully uploaded  
                        return Json(sFileName);
                    }
                    catch (Exception ex)
                    {
                        return Json("0");
                    }
                }
            }
            return Json("0");
        }

        public bool KiemTraSession()
        {
            if (Session["DangNhap"] == null)
            {
                return true;
            }
            return false;
        }

    }
}