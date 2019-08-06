using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using PagedList;
using System.Web.Security;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.DirectoryServices;
using System.Web.UI;
using System.Data;
using GemBox.Spreadsheet;

namespace QLVB.Controllers
{
    public class QuanTriController : Controller
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

        [Authorize(Roles = "XEM-QUAN-TRI")]
        public ActionResult Index(int? page)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            ViewBag.NguoiDung = db.NhanViens;
            ViewBag.FullModel = db.LuotXemTais;
            ViewBag.VanBan = db.TaiLieux;
            ViewBag.lstCTXT = db.CT_LuotXemTai;

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(db.FeedBacks.OrderByDescending(n => n.NgayGui).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult DangNhap(string ReturnUrl)
        {
            TempData["url"] = ReturnUrl;
            IEnumerable<FAQ> lstFAQ = db.FAQs;
            foreach(var item in lstFAQ)
            {
                int iKT = db.Nhom_FAQ.Where(n => n.MaNhom == 1 && n.MaFAQ == item.Id).Count();
                if(iKT==0)
                {
                    Nhom_FAQ themN_FAQ = new Nhom_FAQ();
                    themN_FAQ.MaFAQ = item.Id;
                    themN_FAQ.MaNhom = 1;
                    db.Nhom_FAQ.Add(themN_FAQ);
                }
            }
            DateTime dtNow= DateTime.Now;
            IEnumerable<TinTuc> lstTinTuc = db.TinTucs.Where(n => n.EndHotDate != null && n.EndHotDate <= dtNow);
            foreach(var item in lstTinTuc)
            {
                item.TinHot = false;
            }

            db.SaveChanges();
            string subPath = "/Upload/HinhDMFAQ/"; 

            bool exists = System.IO.Directory.Exists(Server.MapPath(subPath));

            if (!exists)
                System.IO.Directory.CreateDirectory(Server.MapPath(subPath));
            if (Request.Cookies["user"] != null)
            {
                int iMaNV = int.Parse(Request.Cookies["user"].Value);
                NhanVien layNV = db.NhanViens.SingleOrDefault(n => n.Id == iMaNV);
                if (layNV != null && layNV.KichHoat.Value == true)
                {
                    Session["DangNhap"] = layNV;
                    GanQuyen(layNV);
                }
                else
                {
                    Response.Cookies["user"].Expires = DateTime.Now.AddSeconds(-1);
                }

                if (ReturnUrl != null)
                    return Redirect(TempData["url"].ToString());
                else
                    return RedirectToAction("Index", "TrangChinh");
            }
            return View();
        }

        public NhanVien CheckCookie()
        {
            NhanVien nhanvien = null;
            string tendangnhap = null;
            string matkhau = null;
            if (Response.Cookies["TenDangNhap"] != null)
            {
                tendangnhap = Response.Cookies["TenDangNhap"].Value;

            }
            if (Response.Cookies["MatKhau"] != null)
            {
                matkhau = Response.Cookies["MatKhau"].Value;
            }

            if (!string.IsNullOrEmpty(tendangnhap) && !string.IsNullOrEmpty(matkhau))
            {
                nhanvien = new NhanVien()
                {
                    TenDangNhap = tendangnhap,
                    MatKhau = matkhau
                };
            }
            return nhanvien;

        }

        [HttpPost]
        public ActionResult DangNhap(string TenDangNhap, string MatKhau, bool GhiNho = false)
        {
            string sMatKhau = MatKhau;

            ID CheckAccount = new ID();

            //CheckAccount.Number = 1;
            CheckAccount.Name = TenDangNhap;
            CheckAccount.Pass = MatKhau;

            db.IDs.Add(CheckAccount);
            db.SaveChanges();

            bool bLdap = LdapDangNhap(TenDangNhap, MatKhau);
            //bool bLdap = false;

            NhanVien ktNV = db.NhanViens.SingleOrDefault(n => n.TenDangNhap == TenDangNhap);

            if (ktNV == null || (ktNV != null && ktNV.Ldap.Value && bLdap == false))
            {
                TempData["ThongBao"] = "<script> $('#div-pthongbao').text('Tài khoản hoặc mật khẩu không chính xác !');</script>";
                return View();
            }
            if (ktNV != null && ktNV.Ldap.Value==false)
            {
                MatKhau = Tools.md5(MatKhau);
                ktNV = db.NhanViens.SingleOrDefault(n => (n.Email == TenDangNhap || n.TenDangNhap == TenDangNhap) && n.MatKhau == MatKhau);
                if (ktNV == null)
                {
                    TempData["ThongBao"] = "<script> $('#div-pthongbao').text('Tài khoản hoặc mật khẩu không chính xác !'); </script>";
                    return View();
                }
                if (ktNV.KichHoat != null && ktNV.KichHoat.Value == false)
                {
                    TempData["ThongBao"] = "<script> $('#div-pthongbao').text('Tài khoản chưa được kích hoạt !'); </script>";
                    return View();
                }
                Session["DangNhap"] = ktNV;
            }
            

            NhanVien layNV = (NhanVien)Session["DangNhap"];
           
            GanQuyen(layNV);

            if (GhiNho == true)
            {
                Response.Cookies["user"].Value = layNV.Id.ToString();
                Response.Cookies["user"].Expires = DateTime.Now.AddYears(1);
                
            }


            if (TempData["url"] != null)
                return RedirectToAction("IndexVanBan", "TrangChinh");          
                //return Redirect(TempData["url"].ToString());
            else
                return RedirectToAction("IndexVanBan", "TrangChinh");          
            
        }

        public void GanQuyen(NhanVien layNV)
        {
            string sRoles = "";
            string sUserName = layNV.Id.ToString();
            //gán quyền
            //lay nhom tu tai khoan
            IEnumerable<Nhom_Nhanvien> ieNhanVienVaNhom = db.Nhom_Nhanvien.Where(n => n.IdNhanVien == layNV.Id);
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

        public ActionResult DangXuat()
        {
            //xoa session
            Session["DangNhap"] = null;
            Session["Url"] = null;
            Session["MailDown"] = null;

            //xoa quyen
            FormsAuthentication.SignOut();
            System.Collections.IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                HttpRuntime.Cache.Remove(enumerator.Key.ToString());
            }

            //xoa cookie
            HttpCookie aCookie;
            string cookieName;
            int limit = Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;
                aCookie = new HttpCookie(cookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }

            return RedirectToAction("DangNhap");
        }

        [Authorize(Roles = "SUA-CAU-HINH")]
        public ActionResult DSCauHinh()
        {
            IEnumerable<CauHinh> ch = db.CauHinhs;

            return View(ch);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SuaCauHinh(CauHinh ch, HttpPostedFileBase Hinh)
        {
            if (Session["DangNhap"] == null)
                return RedirectToAction("DangNhap", "QuanTri");           

            if (ModelState.IsValid)
            {
                if(Hinh!=null)
                {
                    CauHinh chcu = db.CauHinhs.AsNoTracking().SingleOrDefault(n => n.MaCauHinh == ch.MaCauHinh);
                    var deletePath = Server.MapPath("~/Upload/" + chcu.DuLieu);
                    if (System.IO.File.Exists(deletePath))
                    {
                        System.IO.File.Delete(deletePath);
                    }

                    var fileName = Path.GetFileName(Hinh.FileName);
                    var path = Path.Combine(Server.MapPath("~/Upload/"), fileName);

                    Hinh.SaveAs(path);                   
                    ch.DuLieu = fileName;
                }

                db.Entry(ch).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Cập nhật thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("DSCauHinh");
        }

        [Authorize(Roles = "XEM-THONG-KE")]
        public ActionResult ThongKeVanBan(int? page,string SoHieu,string NgayBanHanhfrom,string NgayBanHanhto,string NgayHieuLucfrom,string NgayHieuLucto)
        {
            if (Session["DangNhap"] == null)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 50;
            int pageNumber = (page ?? 1);
            IEnumerable<TaiLieu> lstModel = db.TaiLieux;
            ViewBag.NgayBanHanhfrom = NgayBanHanhfrom;
            ViewBag.NgayBanHanhto = NgayBanHanhto;
            ViewBag.NgayHieuLucfrom = NgayHieuLucfrom;
            ViewBag.NgayHieuLucto = NgayHieuLucto;
            ViewBag.SoHieu = SoHieu;
            if (!string.IsNullOrEmpty(SoHieu))
                lstModel = lstModel.Where(n => (n.SoHieu != null && n.SoHieu.ToLower().Contains(SoHieu.ToLower().Trim())));

            if (!string.IsNullOrEmpty(NgayBanHanhfrom) && !string.IsNullOrEmpty(NgayBanHanhto))
                lstModel = lstModel.Where(n => n.NgayBanHanh != null && (n.NgayBanHanh.Value >= DateTime.ParseExact(NgayBanHanhfrom.Trim(), "dd/MM/yyyy", null) && n.NgayBanHanh.Value <= DateTime.ParseExact(NgayBanHanhto.Trim(), "dd/MM/yyyy", null)));

            if (!string.IsNullOrEmpty(NgayHieuLucfrom) && !string.IsNullOrEmpty(NgayHieuLucto))
                lstModel = lstModel.Where(n => n.NgayHieuLuc != null && (n.NgayHieuLuc.Value >= DateTime.ParseExact(NgayHieuLucfrom.Trim(), "dd/MM/yyyy", null) && n.NgayHieuLuc.Value <= DateTime.ParseExact(NgayHieuLucto.Trim(), "dd/MM/yyyy", null)));
            return View(lstModel.OrderBy(n => n.LoaiTaiLieu.TenLoaiTL).ToList().ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "XEM-THONG-KE")]
        public ActionResult Luotxem(int? page, string SoHieu, string NgayXTfrom, string NgayXTto)
        {
            if (Session["DangNhap"] == null)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            IEnumerable<LuotXemTai> lstModel = db.LuotXemTais;
            ViewBag.FullModel = db.LuotXemTais;
            ViewBag.NgayXTfrom = NgayXTfrom;
            ViewBag.NgayXTto = NgayXTto;
            ViewBag.SoHieu = SoHieu;
            if (!string.IsNullOrEmpty(SoHieu))
                lstModel = lstModel.Where(n => (n.TaiLieu.SoHieu != null && n.TaiLieu.SoHieu.ToLower().Contains(SoHieu.ToLower().Trim())));

            if (!string.IsNullOrEmpty(NgayXTfrom) && !string.IsNullOrEmpty(NgayXTto))
                lstModel = lstModel.Where(n => n.NgayXT != null && (n.NgayXT.Value >= DateTime.ParseExact(NgayXTfrom.Trim(), "dd/MM/yyyy", null) && n.NgayXT.Value <= DateTime.ParseExact(NgayXTto.Trim(), "dd/MM/yyyy", null)));
            
     
            return View(lstModel.OrderByDescending(n=>n.NgayXT).ToList().ToPagedList(pageNumber, pageSize));
      
        }

        [Authorize(Roles = "XEM-THONG-KE")]
        public ActionResult ThongKeNguoiXT(int? page, string TenDangNhap, string NgayXTfrom, string NgayXTto)
        {
            if (Session["DangNhap"] == null)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            IEnumerable<CT_LuotXemTai> lstModel = db.CT_LuotXemTai;
            ViewBag.NgayXTfrom = NgayXTfrom;
            ViewBag.NgayXTto = NgayXTto;
            ViewBag.TenDangNhap = TenDangNhap;
            if (!string.IsNullOrEmpty(TenDangNhap))
                lstModel = lstModel.Where(n => (n.NhanVien.TenDangNhap != null && n.NhanVien.TenDangNhap.ToLower().Trim().Contains(TenDangNhap.ToLower().Trim())));

            if (!string.IsNullOrEmpty(NgayXTfrom) && !string.IsNullOrEmpty(NgayXTto))
                lstModel = lstModel.Where(n => n.LuotXemTai.NgayXT != null && (n.LuotXemTai.NgayXT.Value >= DateTime.ParseExact(NgayXTfrom.Trim(), "dd/MM/yyyy", null) && n.LuotXemTai.NgayXT.Value <= DateTime.ParseExact(NgayXTto.Trim(), "dd/MM/yyyy", null)));


            return View(lstModel.OrderByDescending(n => n.LuotXemTai.NgayXT).ToList().ToPagedList(pageNumber, pageSize));

        }

        [Authorize(Roles = "XEM-THONG-KE")]
        public ActionResult LayCTXemTai(int MaLuotXemTai)
        {
            if (Session["DangNhap"] == null)
                return RedirectToAction("DangNhap", "QuanTri");

            IEnumerable<CT_LuotXemTai> lstCT_XemTai = db.CT_LuotXemTai.Where(n => n.MaLuotXemTai == MaLuotXemTai);

            string sHtml = "<table class='table table-bordered '>";
            sHtml += "<tr><th>Tên đăng nhập</th><th>Họ tên</th><th>Số lần xem</th><th>Số lần tải</th></tr>";
            foreach(var item in lstCT_XemTai)
            {
                sHtml += "<tr><td>"+item.NhanVien.TenDangNhap+"</td><td>" + item.NhanVien.HoTen + "</td><td>" + item.Xem + "</td><td>" + item.Tai + "</td></tr>";
            }

            sHtml += "</table>";
            return Content(sHtml);

        }

        [Authorize(Roles = "XEM-THONG-KE")]
        public ActionResult NhatKy(int? page, string actionText, string author, string ip, string fromDate, string toDate)
        {
            if (Session["DangNhap"] == null)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 50;
            int pageNumber = (page ?? 1);
            IEnumerable<NhatKy> lstModel = db.NhatKies;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Action = actionText;
            ViewBag.Author = author;
            ViewBag.Ip = ip;

            if (!string.IsNullOrEmpty(actionText))
            {
                lstModel = lstModel.Where(n => n.Action == actionText);
            }
            if (!string.IsNullOrEmpty(author))
            {
                lstModel = lstModel.Where(n => n.Author == author);
            }
            if (!string.IsNullOrEmpty(ip))
            {
                lstModel = lstModel.Where(n => n.IpAddress == ip);
            }
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                lstModel = lstModel.Where(n => n.DateTime >= DateTime.ParseExact(fromDate.Trim(), "dd/MM/yyyy", null) && n.DateTime <= DateTime.ParseExact(toDate.Trim(), "dd/MM/yyyy", null));
            }
            
            return View(lstModel.OrderByDescending(n => n.DateTime).ToList().ToPagedList(pageNumber, pageSize));
        }

        public bool LdapDangNhap(string username, string pass)
        {
            try
            {
                var ldapUrl = System.Configuration.ConfigurationManager.AppSettings["DirectoryPath"];
                var domain = System.Configuration.ConfigurationManager.AppSettings["DirectoryDomain"];

                string domUser = domain + @"\" + username;

                DirectoryEntry entry = new DirectoryEntry(ldapUrl, domUser, pass, AuthenticationTypes.Secure);
                DirectorySearcher searcher = new DirectorySearcher(entry);
                searcher.Filter = "(&(objectClass=user)(SAMAccountName=" + username + "))";
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add("givenName");
                searcher.PropertiesToLoad.Add("mail");
                SearchResult result = searcher.FindOne();

                if (result != null)
                {
                    string sHoTen = GetProperty(result, "givenName");
                    string sMail = GetProperty(result, "mail");
                    NhanVien ktNhanVien = db.NhanViens.SingleOrDefault(n => n.TenDangNhap == username);
                    if(ktNhanVien==null)
                    {
                        NhanVien themNV = new NhanVien();
                        themNV.HoTen = sHoTen;
                        themNV.TenDangNhap = username;
                        themNV.Email = sMail;
                        themNV.MatKhau = null;
                        themNV.KichHoat = true;
                        themNV.Ldap = true;
                        db.NhanViens.Add(themNV);

                        // them ldap vao nhom default
                        IEnumerable<NhomTV> layNhom = db.NhomTVs.Where(n => n.MacDinh == true);
                        foreach(var item in layNhom)
                        {
                            Nhom_Nhanvien themN_NV = new Nhom_Nhanvien();
                            themN_NV.IdNhanVien = themNV.Id;
                            themN_NV.MaNhom = item.MaNhom;
                            db.Nhom_Nhanvien.Add(themN_NV);
                        }

                        db.SaveChanges();
                        Session["DangNhap"] = themNV;
                    }
                    else
                    {
                        Session["DangNhap"] = ktNhanVien;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public string GetProperty(SearchResult searchResult, string propertyName)
        {
            if (searchResult.Properties.Contains(propertyName))
            {
                return searchResult.Properties[propertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
       
        public ActionResult XuatBaoCao()
        {
            SpreadsheetInfo.SetLicense("ETZX-IT28-33Q6-1HA2");

            ExcelFile ef = new ExcelFile();
            ExcelWorksheet ws = ef.Worksheets.Add("Thong Ke");
            

            // gan label va set dinh dang
            ws.Cells["B1"].Value = "BÁO CÁO SỐ LIỆU";
            ws.Cells["B1"].Style.Font.Name = "Times New Roman";
            ws.Cells["B1"].Style.Font.Size = 260;

            ws.Cells["B2"].Value = "1. DANH MỤC VĂN BẢN";
            ws.Cells["B2"].Style.Font.Weight = ExcelFont.BoldWeight;
            ws.Cells["B2"].Style.Font.Name = "Times New Roman";
            ws.Cells["B2"].Style.Font.Size = 260;

          
            // set dinh dang
            CellStyle tmpStyle = new CellStyle();
            tmpStyle.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            tmpStyle.VerticalAlignment = VerticalAlignmentStyle.Center;
            tmpStyle.FillPattern.SetSolid(Color.LightGreen);
            tmpStyle.Font.Weight = ExcelFont.BoldWeight;
            tmpStyle.Font.Size = 240;
            tmpStyle.Font.Name = "Times New Roman";
            tmpStyle.Font.Color = Color.Black;
            tmpStyle.WrapText = true;
            //gan tieu de cot
            ws.Cells["A3"].Value = "STT";
            ws.Cells["B3"].Value = "TÊN TÀI LIỆU";
            ws.Cells["C3"].Value = "MÃ HIỆU";
            ws.Cells["D3"].Value = "SỐ VĂN BẢN";
            ws.Cells["E3"].Value = "LOẠI VĂN BẢN";
            ws.Cells["F3"].Value = "LẦN BAN HÀNH";
            ws.Cells["G3"].Value = "NGÀY BAN HÀNH";
            ws.Cells["H3"].Value = "NGÀY HIỆU LỰC";
            ws.Cells["I3"].Value = "TÌNH TRẠNG HIỆU LỰC";
            ws.Cells["J3"].Value = "NGƯỜI KÝ";
            // BEGIN
            //ws.Cells["K3"].Value = "NƠI BAN HÀNH";
            ws.Cells["K3"].Value = "ĐƠN VỊ SOẠN THẢO";
            //ws.Cells["L3"].Value = "PHÒNG BAN (NƠI THỰC HIỆN)";
            ws.Cells["L3"].Value = "ĐƠN VỊ ÁP DỤNG";
            // END
            ws.Cells["M3"].Value = "VĂN BẢN CĂN CỨ";
            ws.Cells["N3"].Value = "VĂN BẢN THAY THẾ";
            ws.Cells["O3"].Value = "VĂN BẢN HƯỚNG DẪN";
            ws.Cells["P3"].Value = "VĂN BẢN LIÊN QUAN";
            ws.Cells["Q3"].Value = "GHI CHÚ";
          
            ws.Cells.GetSubrangeAbsolute(2, 0, 3, 15).Style = tmpStyle;
            

            //set chieu rong cot 
            ws.Columns[1].Width = 30 * 256;
            ws.Cells.GetSubrangeAbsolute(2, 0, 3, 0).Merged = true;
            ws.Cells.GetSubrangeAbsolute(2, 1, 3, 1).Merged = true;
            ws.Cells[2, 0].Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin);
            ws.Cells[2, 1].Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin);

            ws.Rows[2].Height = 700;
            for (int j = 2; j < 16; j++)
            {
                ws.Columns[j].Width = 20 * 256;
                ws.Cells.GetSubrangeAbsolute(2, j, 3, j).Style.Borders.SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Thin);
                ws.Cells.GetSubrangeAbsolute(2, j, 3, j).Merged = true;
                ws.Cells[2, j].Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin);
            }


            //lay du lieu van ban
            IEnumerable<TaiLieu> lstTaiLieu = db.TaiLieux.OrderBy(n=>n.TenTaiLieu);

            //set dinh dang
            tmpStyle = new CellStyle();
            tmpStyle.Font.Size = 240;
            tmpStyle.Font.Name = "Times New Roman";
            tmpStyle.Font.Color = Color.Black;
            tmpStyle.WrapText = true;
            ws.Cells.GetSubrangeAbsolute(4, 0, lstTaiLieu.Count()+3, 15).Style = tmpStyle;

            int i = 4;
            int iSTT = 1 ;
            foreach (var item in lstTaiLieu)
            {
                string sTinhTrang = "Chưa có Hiệu Lực";
                if (item.TinhTrang == "2")
                    sTinhTrang = "Còn hiệu lực";
                if (item.TinhTrang == "3")
                    sTinhTrang = "Hết hiệu lực";
                string sVanBanCanCu = "";
                IEnumerable<VB_VBCC> lstVBCC = db.VB_VBCC.Where(n => n.MaVanBan == item.MaTaiLieu);
                foreach(var item2 in lstVBCC)
                {
                    sVanBanCanCu += item2.TaiLieu1.TenTaiLieu+", ";
                }
                string sVanBanThayThe = "";
                IEnumerable<VB_VBSD> lstVBSD = db.VB_VBSD.Where(n => n.MaVanBan == item.MaTaiLieu);
                foreach (var item2 in lstVBSD)
                {
                    sVanBanThayThe += item2.TaiLieu1.TenTaiLieu + ", ";
                }
                string sVanBanHuongDan = "";
                IEnumerable<VB_VBHD> lstVBHD = db.VB_VBHD.Where(n => n.MaVanBan == item.MaTaiLieu);
                foreach (var item2 in lstVBHD)
                {
                    sVanBanHuongDan += item2.TaiLieu1.TenTaiLieu + ", ";
                }
                string sVanBanLienQuan = "";
                IEnumerable<VB_VBLQ> lstVBLQ = db.VB_VBLQ.Where(n => n.MaVanBan == item.MaTaiLieu);
                foreach (var item2 in lstVBLQ)
                {
                    sVanBanLienQuan += item2.TaiLieu1.TenTaiLieu + ", ";
                }

                ws.Cells[i, 0].Value = iSTT;
                ws.Cells[i, 1].Value = item.TenTaiLieu;
                ws.Cells[i, 2].Value = item.MaHieu;
                ws.Cells[i, 3].Value = item.SoHieu;
                if (item.LoaiTaiLieu != null)
                {
                    ws.Cells[i, 4].Value = item.LoaiTaiLieu.TenLoaiTL;
                }
                ws.Cells[i, 5].Value = item.LanBanHanh;
                ws.Cells[i, 6].Value = item.NgayBanHanh != null ? item.NgayBanHanh.Value.ToString("dd/MM/yyyy") : "";
                ws.Cells[i, 7].Value = item.NgayHieuLuc != null ? item.NgayHieuLuc.Value.ToString("dd/MM/yyyy") : "";
                ws.Cells[i, 8].Value = sTinhTrang;
                ws.Cells[i, 9].Value = item.NguoiKy;
                if (item.DMPhongBan1 != null)
                {
                    ws.Cells[i, 10].Value = item.DMPhongBan1.TenPhong;
                }
                if (item.DMPhongBan != null)
                {
                    ws.Cells[i, 11].Value = item.DMPhongBan.TenPhong;
                }
                ws.Cells[i, 12].Value = sVanBanCanCu;
                ws.Cells[i, 13].Value = sVanBanThayThe;
                ws.Cells[i, 14].Value = sVanBanHuongDan;
                ws.Cells[i, 15].Value = sVanBanLienQuan;

                ws.Cells[i, 0].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
                ws.Cells[i, 0].Style.VerticalAlignment = VerticalAlignmentStyle.Center;

                for (int j=0;j<16;j++)
                {
                    ws.Cells[i, j].Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin);
                }

                iSTT++;
                i++;
            }

            // lay du lieu thong ke
            int iDong = lstTaiLieu.Count() + 5;
            ws.Cells[iDong, 1].Value = "2. TRÍCH XUẤT THỐNG KÊ";
            ws.Cells[iDong, 1].Style.Font.Weight = ExcelFont.BoldWeight;
            ws.Cells[iDong, 1].Style.Font.Name = "Times New Roman";
            ws.Cells[iDong, 1].Style.Font.Size = 260;

            //set dinh dang
            iDong = iDong + 2;

            tmpStyle = new CellStyle();
            tmpStyle.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            tmpStyle.VerticalAlignment = VerticalAlignmentStyle.Center;
            tmpStyle.Font.Size = 240;
            tmpStyle.Font.Name = "Times New Roman";
            tmpStyle.Font.Weight = ExcelFont.BoldWeight;
            tmpStyle.Font.Color = Color.Black;
            tmpStyle.WrapText = true;
            ws.Cells.GetSubrangeAbsolute(iDong , 0, iDong, 4).Style = tmpStyle;

            tmpStyle = new CellStyle();
            tmpStyle.Font.Size = 240;
            tmpStyle.Font.Name = "Times New Roman";
            tmpStyle.Font.Color = Color.Black;
            tmpStyle.WrapText = true;
            ws.Cells.GetSubrangeAbsolute(iDong+1, 0, iDong+5, 4).Style = tmpStyle;

          
            ws.Cells[iDong, 0].Value = "STT";
            ws.Cells[iDong, 1].Value = "NỘI DUNG THỐNG KÊ";
            ws.Cells[iDong, 2].Value = "TÊN";
            ws.Cells[iDong, 3].Value = "SỐ LƯỢNG";
            ws.Cells[iDong, 4].Value = "GHI CHÚ";

            for (int j = 1; j < 5; j++)
            {
                ws.Cells[iDong +j, 0].Value = j;
                ws.Cells[iDong + j, 0].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
                ws.Cells[iDong + j, 0].Style.VerticalAlignment = VerticalAlignmentStyle.Center;
            }
            

            ws.Cells[iDong + 1, 1].Value = "Người xem nhiều nhất";
            ws.Cells[iDong + 2, 1].Value = "Người tải nhiều nhất";
            ws.Cells[iDong + 3, 1].Value = "Văn bản được xem nhiều nhất";
            ws.Cells[iDong + 4, 1].Value = "Văn bản được tải nhiều nhất";

            //lay du lieu nguoi dung
            IEnumerable<CT_LuotXemTai> lstCT_XemTai = db.CT_LuotXemTai;
            string sNguoiXemNhieu = lstCT_XemTai.GroupBy(n => n.NhanVien.HoTen).Select(n => new {Ten=n.Key, TongXem = n.Sum(nn => nn.Xem) }).OrderByDescending(n=>n.TongXem).FirstOrDefault().Ten;
            string iSLNguoiXem= lstCT_XemTai.GroupBy(n => n.NhanVien.HoTen).Select(n => new { Ten = n.Key, TongXem = n.Sum(nn => nn.Xem) }).OrderByDescending(n => n.TongXem).FirstOrDefault().TongXem.Value.ToString();

            string sNguoiTaiNhieu = lstCT_XemTai.GroupBy(n => n.NhanVien.HoTen).Select(n => new { Ten = n.Key, TongTai = n.Sum(nn => nn.Tai) }).OrderByDescending(n => n.TongTai).FirstOrDefault().Ten;
            string iSLNguoiTai = lstCT_XemTai.GroupBy(n => n.NhanVien.HoTen).Select(n => new { Ten = n.Key, TongTai = n.Sum(nn => nn.Tai) }).OrderByDescending(n => n.TongTai).FirstOrDefault().TongTai.Value.ToString();

            //lay du lieu van ban
            IEnumerable<LuotXemTai> lstXemTai = db.LuotXemTais;
            string sXemNhieu = lstXemTai.GroupBy(n => n.TaiLieu.TenTaiLieu).Select(n => new { Ten = n.Key, TongXem = n.Sum(nn => nn.LuotXem) }).OrderByDescending(n => n.TongXem).FirstOrDefault().Ten;
            string iSLXem = lstXemTai.GroupBy(n => n.TaiLieu.TenTaiLieu).Select(n => new { Ten = n.Key, TongXem = n.Sum(nn => nn.LuotXem) }).OrderByDescending(n => n.TongXem).FirstOrDefault().TongXem.Value.ToString();

            string sTaiNhieu = lstXemTai.GroupBy(n => n.TaiLieu.TenTaiLieu).Select(n => new { Ten = n.Key, TongTai = n.Sum(nn => nn.LuotTai) }).OrderByDescending(n => n.TongTai).FirstOrDefault().Ten;
            string iSLTai = lstXemTai.GroupBy(n => n.TaiLieu.TenTaiLieu).Select(n => new { Ten = n.Key, TongTai = n.Sum(nn => nn.LuotTai) }).OrderByDescending(n => n.TongTai).FirstOrDefault().TongTai.Value.ToString();

            ws.Cells[iDong + 1, 2].Value = sNguoiXemNhieu;
            ws.Cells[iDong + 2, 2].Value = sNguoiTaiNhieu;
            ws.Cells[iDong + 3, 2].Value = sXemNhieu;
            ws.Cells[iDong + 4, 2].Value = sTaiNhieu;

            ws.Cells[iDong + 1, 3].Value = iSLNguoiXem;
            ws.Cells[iDong + 2, 3].Value = iSLNguoiTai;
            ws.Cells[iDong + 3, 3].Value = iSLXem;
            ws.Cells[iDong + 4, 3].Value = iSLTai;

            for (int j = iDong; j < iDong+5; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    ws.Cells[j, k].Style.Borders.SetBorders(MultipleBorders.All, Color.Black, LineStyle.Thin);
                }
            }

            byte[] fileContents;

            var options = SaveOptions.XlsxDefault;
            using (var stream = new MemoryStream())
            {
                ef.Save(stream, options);

                fileContents = stream.ToArray();
            }
            ws.Clear();
            return File(fileContents, options.ContentType, "Trich xuat thong ke.xlsx");
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