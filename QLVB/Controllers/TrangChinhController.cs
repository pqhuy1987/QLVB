using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using QLVB.Models;

namespace QLVB.Controllers
{
     [Authorize]
    public class TrangChinhController : Controller
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

        public ActionResult Index(int? page, string TenVanBan, string SoHieu, string LoaiVanBan, string NoiBanHanh, string NguoiKy, string NgayBanHanhfrom, string NgayBanHanhto, string NgayHieuLucfrom, string NgayHieuLucto, string TinhTrang, string PhongBan)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();
            DateTime dtNow=DateTime.Now;

            ViewBag.LoaiVanBans = db.LoaiTaiLieux.OrderBy(n=>n.ThuTu);
            ViewBag.VanBans = db.TaiLieux;

            int pageSize = 10;
            int pageNumber = (page ?? 1);


            IEnumerable<TaiLieu> lstTaiLieu = db.TaiLieux.Where(n => n.TenTaiLieu != null);
            ViewBag.DMTinTuc = db.DanhMucs.Where(n=>n.DanhMucCha==Tools.MaDanhMucTin).OrderBy(n=>n.ThuTu);
            ViewBag.TinTuc = db.TinTucs.Where(n=>n.TrangThai==true && n.NgayDang <=dtNow ).OrderByDescending(n=>n.NgayDang).Take(4);
            ViewBag.dsTinTuc = db.TinTucs.Where(n => n.TrangThai == true && n.NgayDang <= dtNow).OrderByDescending(n => n.NgayDang);

            //xu ly lich hop
            ViewBag.lstPhong = db.PhongHops.Where(n=>n.Status==true).OrderBy(n=>n.Ordering);

            ViewBag.lstBanner = db.Banners.OrderBy(n => n.Order);

            IEnumerable<LichHop> lstLichHop = db.LichHops.Where(n =>n.Status==true && n.Start.Value.Day== dtNow.Day && n.Start.Value.Month == dtNow.Month && n.Start.Value.Year == dtNow.Year);

            List<DuyetLichHop> lstDuyetLich = new List<DuyetLichHop>();
            lstDuyetLich = DuyetLichHop.ThemLichDinhKy(DateTime.Now, DateTime.Now);

            foreach(var item in lstLichHop.Where(n=>n.RecurrenceRule==null))
            {
                DuyetLichHop lichhop = new DuyetLichHop(item.Subject,item.RoomId.Value,item.Start.Value, item.End.Value, item.Start.Value, false);
                lstDuyetLich.Add(lichhop);

            }

            ViewBag.lstLichHop = lstDuyetLich;

            //return View(lstTaiLieu.OrderByDescending(n=>n.NgayHieuLuc).ToList().ToPagedList(pageNumber, pageSize));
            return RedirectToAction("dsTinTuc", "TinTuc");
        }

        public ActionResult IndexVanBan(int? page, string TenVanBan, string SoHieu, string LoaiVanBan1, string NoiBanHanh, string NguoiKy, string NgayBanHanhfrom, string NgayBanHanhto, string NgayHieuLucfrom, string NgayHieuLucto, string TinhTrang, string PhongBan, string MaHieu, string LanBanHanh)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");
     

           Tools tool = new Tools();

            ViewBag.TinhTrang = tool.DMTinhTrang(!string.IsNullOrEmpty(TinhTrang) ? Convert.ToInt32(TinhTrang) : 0);
            ViewBag.LoaiVanBan = new SelectList(db.LoaiTaiLieux.OrderBy(n => n.CapTaiLieu), "MaLoaiTL", "TenLoaiTL", LoaiVanBan1);
            ViewBag.PhongBan = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", PhongBan);
            ViewBag.NoiBanHanh = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", NoiBanHanh);

            ViewBag.LoaiVanBans = db.LoaiTaiLieux.OrderBy(n => n.ThuTu);
            ViewBag.VanBans = db.TaiLieux;

            int pageSize = 10;
            int pageNumber = (page ?? 1);

 
            //tim kiem
            ViewBag.TenVanBan = TenVanBan;
            ViewBag.SoHieu = SoHieu;
            ViewBag.NoiBanHanhi = NoiBanHanh;
            ViewBag.NguoiKy = NguoiKy;
            ViewBag.NgayBanHanhfrom = NgayBanHanhfrom;
            ViewBag.NgayBanHanhto = NgayBanHanhto;
            ViewBag.NgayHieuLucfrom = NgayHieuLucfrom;
            ViewBag.NgayHieuLucto = NgayHieuLucto;
            ViewBag.LoaiVanBani = LoaiVanBan1;
            ViewBag.TinhTrangi = TinhTrang;
            ViewBag.PhongBani = PhongBan;
            ViewBag.MaHieu = MaHieu;
            ViewBag.LanBanHanh = LanBanHanh;

            IEnumerable<TaiLieu> lstTaiLieu = db.TaiLieux.Where(n=>n.TenTaiLieu!=null).OrderByDescending(n => n.NgayHieuLuc);
            if (!string.IsNullOrEmpty(TenVanBan))
            {
                List<TaiLieu> lstTam = new List<TaiLieu>();  // lst tam dung de loc du lieu tu lst chinh
                List<TaiLieu> lstDaLoc = new List<TaiLieu>(); // lst chua du lieu da loc ra

                //tim theo cum tu

                //dung chinh xac
                lstTam = lstTaiLieu.Where(n => n.TenTaiLieu != null && n.TenTaiLieu.Contains(TenVanBan.Trim())).ToList();
                lstDaLoc.AddRange(lstTam);

                // gan dung
                lstTam = lstTaiLieu.Where(n => n.TenTaiLieu != null && Tools.RemoveDiacritics(n.TenTaiLieu).Contains(Tools.RemoveDiacritics(TenVanBan.Trim()))).ToList();
                lstDaLoc.AddRange(lstTam);

                // tim theo tung tu
                string[] sTenVanBan = TenVanBan.Split(' ');

                //dung chinh xac
                foreach (var item in sTenVanBan)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        lstTam = lstTaiLieu.Where(n => n.TenTaiLieu != null && n.TenTaiLieu.Contains(item.Trim())).ToList();

                        lstDaLoc.AddRange(lstTam);
                    }
                }

                //gan dung
                foreach (var item in sTenVanBan)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        lstTam = lstTaiLieu.Where(n => n.TenTaiLieu != null && Tools.RemoveDiacritics(n.TenTaiLieu).Contains(Tools.RemoveDiacritics(item.Trim()))).ToList();

                        lstDaLoc.AddRange(lstTam);
                    }
                }

                //tim theo noi dung tom tat
                lstTam = lstTaiLieu.Where(n => (n.NoiDungTomTat != null && Tools.RemoveDiacritics(System.Net.WebUtility.HtmlDecode(n.NoiDungTomTat)).Contains(Tools.RemoveDiacritics(TenVanBan.Trim())))).ToList();
                lstDaLoc.AddRange(lstTam);

                //gan lst da loc vao lst chinh, bo cac khoa trung
                lstTaiLieu = lstDaLoc.GroupBy(t => t.MaTaiLieu).Select(g => g.First()).ToList();
            }

            if (!string.IsNullOrEmpty(SoHieu))
                lstTaiLieu = lstTaiLieu.Where(n => (n.SoHieu != null && n.SoHieu.ToLower().Contains(SoHieu.ToLower().Trim())));

            if (!string.IsNullOrEmpty(NoiBanHanh))
            {
                int iNoiBanHanh = int.Parse(NoiBanHanh);
                lstTaiLieu = lstTaiLieu.Where(n => (n.NoiBanHanh != null && n.NoiBanHanh == iNoiBanHanh));
            }

            if (!string.IsNullOrEmpty(NguoiKy))
                lstTaiLieu = lstTaiLieu.Where(n => (n.NguoiKy != null && Tools.RemoveDiacritics(n.NguoiKy).Contains(Tools.RemoveDiacritics(NguoiKy.Trim()))));

            if (!string.IsNullOrEmpty(NgayBanHanhfrom) && !string.IsNullOrEmpty(NgayBanHanhto))
                lstTaiLieu = lstTaiLieu.Where(n => n.NgayBanHanh != null && (n.NgayBanHanh.Value >= DateTime.ParseExact(NgayBanHanhfrom, "dd/MM/yyyy", null) && n.NgayBanHanh.Value <= DateTime.ParseExact(NgayBanHanhto, "dd/MM/yyyy", null)));

            if (!string.IsNullOrEmpty(NgayHieuLucfrom) && !string.IsNullOrEmpty(NgayHieuLucto))
                lstTaiLieu = lstTaiLieu.Where(n => n.NgayHieuLuc != null && (n.NgayHieuLuc.Value >= DateTime.ParseExact(NgayHieuLucfrom, "dd/MM/yyyy", null) && n.NgayHieuLuc.Value <= DateTime.ParseExact(NgayHieuLucto, "dd/MM/yyyy", null)));


            ViewBag.titLoaiVB = "Văn bản mới";
            if (!string.IsNullOrEmpty(LoaiVanBan1))
            {
                int iLoaiVanBan = int.Parse(LoaiVanBan1);
                lstTaiLieu = lstTaiLieu.Where(n => (n.LoaiVanBan != null && n.LoaiVanBan.Value == iLoaiVanBan));

                ViewBag.titLoaiVB = db.LoaiTaiLieux.SingleOrDefault(n => n.MaLoaiTL == iLoaiVanBan).TenLoaiTL;
            }
            if (!string.IsNullOrEmpty(TinhTrang))
            {
                lstTaiLieu = lstTaiLieu.Where(n => (n.TinhTrang != null && n.TinhTrang.Trim() == TinhTrang.Trim()));
            }
            if (!string.IsNullOrEmpty(PhongBan))
            {
                int iPhongBan = int.Parse(PhongBan);
                lstTaiLieu = lstTaiLieu.Where(n => (n.PhongBan != null && n.PhongBan.Value == iPhongBan));
            }
            if (!string.IsNullOrEmpty(MaHieu))
            {
                lstTaiLieu = lstTaiLieu.Where(n => (n.MaHieu != null && n.MaHieu.Contains(MaHieu)));
            }
            if (!string.IsNullOrEmpty(LanBanHanh))
            {
                int iLanBanHanh = int.Parse(LanBanHanh);
                lstTaiLieu = lstTaiLieu.Where(n => (n.LanBanHanh != null && n.LanBanHanh.Value == iLanBanHanh));
            }

            return View(lstTaiLieu.ToList().ToPagedList(pageNumber, pageSize));
            //return RedirectToAction("dsTinTuc", "TinTuc");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult IndexVanBan(FormCollection c)
        {
            string sTenVanBan = c["TenVanBan"];
            string sSoHieu = c["SoHieu"];
            string sLoaiVanBan = c["LoaiVanBan"];
            string sNoiBanHanh = c["NoiBanHanh"];
            string sNguoiKy = c["NguoiKy"];
            string sNgayBanHanhfrom = c["NgayBanHanhfrom"];
            string sNgayBanHanhto = c["NgayBanHanhto"];
            string sNgayHieuLucfrom = c["NgayHieuLucfrom"];
            string sNgayHieuLucto = c["NgayHieuLucto"];
            string sTinhTrang = c["TinhTrang"];
            string sPhongBan = c["PhongBan"];
            string sMaHieu = c["Mahieu"];
            string sLanBanHanh = c["LanBanHanh"];

            return RedirectToAction("IndexVanBan", "TrangChinh", new { @TenVanBan = sTenVanBan, @SoHieu = sSoHieu, @NoiBanHanh = sNoiBanHanh, @LoaiVanBan = sLoaiVanBan, @NguoiKy = sNguoiKy, @NgayBanHanhfrom = sNgayBanHanhfrom, @NgayBanHanhto = sNgayBanHanhto, @NgayHieuLucfrom = sNgayHieuLucfrom, @NgayHieuLucto = sNgayHieuLucto, @TinhTrang = sTinhTrang, @PhongBan = sPhongBan, @MaHieu = sMaHieu, @LanBanHanh = sLanBanHanh });
        }

        public ActionResult XemVanBan(int id)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            NhanVien nv=(NhanVien)Session["DangNhap"];
            DateTime dNow=DateTime.Now.Date;
            LuotXemTai lxt = db.LuotXemTais.SingleOrDefault(n => n.MaVanBan == id && n.NgayXT.Value == dNow);
            if (lxt == null)
            {
                LuotXemTai themLXT = new LuotXemTai();
                themLXT.MaVanBan = id;
                themLXT.NgayXT = dNow;
                themLXT.LuotXem = 1;
                themLXT.LuotTai = 0;
                db.LuotXemTais.Add(themLXT);

                CT_LuotXemTai themct_luotxemtai = new CT_LuotXemTai(); // them tai lieu xem lan dau trong ngay
                themct_luotxemtai.MaLuotXemTai = themLXT.Id;
                themct_luotxemtai.IDNguoiDung = nv.Id;
                themct_luotxemtai.Xem = 1;
                themct_luotxemtai.Tai = 0;
                db.CT_LuotXemTai.Add(themct_luotxemtai);
                db.SaveChanges();
            }
            else
            {
                lxt.LuotXem += 1;

                CT_LuotXemTai ct_luotxemtai = db.CT_LuotXemTai.SingleOrDefault(n => n.MaLuotXemTai == lxt.Id && n.IDNguoiDung == nv.Id);
                if(ct_luotxemtai==null) // them nguoi xem thu 2 >
                {
                    CT_LuotXemTai themct_luotxemtai = new CT_LuotXemTai();
                    themct_luotxemtai.MaLuotXemTai = lxt.Id;
                    themct_luotxemtai.IDNguoiDung = nv.Id;
                    themct_luotxemtai.Xem = 1;
                    themct_luotxemtai.Tai = 0;
                    db.CT_LuotXemTai.Add(themct_luotxemtai);
                }
                else // cap nhat lan xem nguoi da xem
                {
                    ct_luotxemtai.Xem += 1;
                }

                db.SaveChanges();
            }

            TaiLieu layTaiLieu = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == id);
            string sUrl = "/van-ban/" + Tools.RemoveDiacritics(layTaiLieu.TenTaiLieu) + "-" + id;

            return Redirect(sUrl);
            //return RedirectToAction("ChiTietVanBan", "TrangChinh", new { @id = id });
        }

        public ActionResult TaiVanBan(int id,string ng)
        {
            DateTime dNow = DateTime.Now;
            Tools tool = new Tools();
            int iGioiHanDown = int.Parse(db.CauHinhs.SingleOrDefault(n => n.MaCauHinh == "GH-DOWN").DuLieu);

            TaiLieu layTaiLieu = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == id);
            string sUrl = "/van-ban/" + Tools.RemoveDiacritics(layTaiLieu.TenTaiLieu) + "-" + id;
            
            if (Session["DangNhap"] != null)
            {
                NhanVien nv = (NhanVien)Session["DangNhap"];

                int KT_ctluotXT = db.CT_LuotXemTai.Count(n => n.LuotXemTai.NgayXT == dNow.Date && n.IDNguoiDung == nv.Id && n.Tai > 0); // kiem tra trong ngay, nguoi nay da tai bao nhieu van ban
                int KT_daTai = db.CT_LuotXemTai.Count(n => n.LuotXemTai.NgayXT == dNow.Date && n.IDNguoiDung == nv.Id && n.LuotXemTai.MaVanBan == id && n.Tai > 0); // kiem tra xem nguoi nay da tai tai lieu nay chua

                // BEGIN
                //var tmp = db.CT_LuotXemTai.Where(n => n.LuotXemTai.NgayXT == dNow.Date && n.IDNguoiDung == nv.Id && n.Tai > 0).FirstOrDefault();
                var tmp = db.CT_LuotXemTai.Where(n => n.LuotXemTai.NgayXT == dNow.Date && n.IDNguoiDung == nv.Id && n.Tai > 0).Select(x => x.Tai).Sum();
                if (tmp != null)
                {
                    //KT_ctluotXT = tmp.Tai.Value;
                    KT_ctluotXT = tmp.Value;
                }
                // END

                //if (KT_ctluotXT >= iGioiHanDown && KT_daTai == 0)
                if (KT_ctluotXT >= iGioiHanDown)
                {
                    if (Session["MailDown"] == null)
                    {
                        string sNguoiNhan = System.Configuration.ConfigurationManager.AppSettings["ReportDownMan"];
                        string sTieuDe = System.Configuration.ConfigurationManager.AppSettings["ReportDownSubject"];
                        string html = tool.RenderViewToString(ControllerContext, "~/Views/Shared/_MailThongBaoDownLoad.cshtml", nv, true);

                        tool.SendMail(html, sNguoiNhan, sTieuDe);
                        Session["MailDown"] = nv;
                    }
                
                    TempData["thongbao"] = "<script>$('#pthongbao').text('Vượt quá số lần tải cho phép!'); $('#btn-thongbao2').trigger('click');</script>";

                    return Redirect(sUrl);
                }
                
                LuotXemTai lxt = db.LuotXemTais.SingleOrDefault(n => n.MaVanBan == id && n.NgayXT.Value == dNow.Date);
                if (lxt != null)
                {
                    lxt.LuotTai += 1;

                    CT_LuotXemTai ct_luotxemtai = db.CT_LuotXemTai.SingleOrDefault(n => n.MaLuotXemTai == lxt.Id && n.IDNguoiDung == nv.Id);
                    if (ct_luotxemtai != null) // sua nguoi tai
                    {
                        ct_luotxemtai.Tai += 1;
                    }

                    db.SaveChanges();
                }
                return Redirect(ng);
            }
            TempData["thongbao"] = "<script>$('#pthongbao').text('Bạn chưa đăng nhập !'); $('#btn-thongbao2').trigger('click');</script>";
            
            return Redirect(sUrl);
            //return RedirectToAction("ChiTietVanBan", "TrangChinh", new { @id = id });
        }

        public FileResult DownloadFile(string fileName)
        {
            var filepath = System.IO.Path.Combine(Server.MapPath("/Files/"), fileName);
            return File(filepath, MimeMapping.GetMimeMapping(filepath), fileName);
        }

        public ActionResult ChiTietVanBan(int id)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();
            ViewBag.TinhTrang = tool.DMTinhTrang(0);
            ViewBag.LoaiVanBan = new SelectList(db.LoaiTaiLieux.OrderBy(n => n.CapTaiLieu), "MaLoaiTL", "TenLoaiTL");
            ViewBag.PhongBan = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong");
            ViewBag.NoiBanHanh = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong");

            TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == id);

            //lay cac van ban lien quan
            ViewBag.VBCC = db.VB_VBCC.Where(n => n.MaVanBan == id);
            ViewBag.VBSD = db.VB_VBSD.Where(n => n.MaVanBan == id);
            ViewBag.VBHD = db.VB_VBHD.Where(n => n.MaVanBan == id);
            ViewBag.VBLQ = db.VB_VBLQ.Where(n => n.MaVanBan == id);
            ViewBag.Revision = db.Revisions.Where(n => n.MaVanBan == id);
            ViewBag.CauHinh = db.CauHinhs;

            // BEGIN
            ViewBag.BMLQ = db.VB_BMLQ.Where(n => n.MaVanBan == id);
            // END

            //lay ghi chu van ban
            if (Session["DangNhap"] != null)
            {              
                NhanVien nv=(NhanVien)Session["DangNhap"];
                GhiChu gGhiChu = db.GhiChus.SingleOrDefault(n => n.MaVanBan == tl.MaTaiLieu && n.IdNhanVien == nv.Id);
                if(gGhiChu!=null)
                    ViewBag.NoiDungGhiChu = gGhiChu.NoiDung;

                 //kiem tra van ban lu tru
                ViewBag.VBLuuTru = db.VBLuuTrus.Count(n => n.IdNhanVien == nv.Id && n.MaVanBan == tl.MaTaiLieu);
            }

            if (tl.BaoMat == true && !System.Web.HttpContext.Current.User.IsInRole("XEM-VB-MAT"))
                return RedirectToAction("IndexVanBan");
            return View(tl);
        }

        public ActionResult dsFAQ()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            IEnumerable<FAQ> lstfaq = db.FAQs.OrderBy(n=>n.STT);
            IEnumerable<FAQ_NoiDung> lstfaq_ND = db.FAQ_NoiDung;
            foreach(var item in lstfaq)
            {
                if (!KTNhom_FAQ(item.Id))
                    lstfaq = lstfaq.Where(n => n.Id != item.Id);
            }
            ViewBag.faq = lstfaq;
            ViewBag.faq_ND = lstfaq_ND;

            return View(lstfaq.Where(n => n.Parent_Id == 0));
        }

        public ActionResult duyetFAQ_ajax(int idCha)
        {
            IEnumerable<FAQ> lstFAQ = db.FAQs.OrderBy(n=>n.STT);
            IEnumerable<FAQ_NoiDung> lstFAQ_ND = db.FAQ_NoiDung.OrderBy(n => n.STT);
            DuyetFAQ(idCha, lstFAQ , 0,lstFAQ_ND);
            return Content(sHtml);
        }

        string sHtml = "";
        public void DuyetFAQ(int iMaFAQ, IEnumerable<FAQ> lstFAQ , int i , IEnumerable<FAQ_NoiDung> lstFAQ_ND)
        {
            if (i == 0)
            {
                sHtml += "<ul class='treeFAQ'>";
            }
            else
                sHtml += "<ul class='mn-child'>";

            if (KTNhom_FAQ(iMaFAQ) && lstFAQ_ND.Where(n => n.MaDanhMuc == iMaFAQ).Count() > 0)
            {
                foreach (var item_ND in lstFAQ_ND.Where(n => n.MaDanhMuc == iMaFAQ))
                {
                    string sUrl = "faq/" + Tools.RemoveDiacritics(item_ND.CauHoi) + "-" + item_ND.Id;

                    sHtml += "<li>";
                    sHtml += " <span class='span-tieude'><i class='fa fa-file-text-o'></i></i>";
                    sHtml += " <a target='_blank' href='"+sUrl+"'>" + item_ND.CauHoi + "</a>";
                    sHtml += "</span> <span class='text-success span-soluong'></span> ";
                }
            }

            foreach (var item in lstFAQ.Where(n => n.Parent_Id == iMaFAQ))
            {

                if (KTNhom_FAQ(item.Id))
                {
                 
                        sHtml += "<li class='li-danhmuc'>";
                        sHtml += " <span class='mn-parent span-tieude'><i class='fa fa-folder' style='display:none'></i><i class='fa fa-folder-open' ></i> " + item.CauHoi + " </span> ";
                        sHtml += " <span class='span-soluong'></span> ";
                    
                }
                DuyetFAQ(item.Id, lstFAQ,1,lstFAQ_ND);
            }

            sHtml += "</ul>";
        }

        public static int SL_FAQ = 0;
        public static int SL_ND_FAQ(int iMaFAQ, IEnumerable<FAQ> lstFAQ, IEnumerable<FAQ_NoiDung> lstFAQ_ND)
        {       
            SL_FAQ += lstFAQ_ND.Where(n => n.MaDanhMuc == iMaFAQ).Count();
            foreach (var item in lstFAQ.Where(n => n.Parent_Id == iMaFAQ))
            {
                SL_ND_FAQ(item.Id, lstFAQ, lstFAQ_ND);
            }
            return SL_FAQ;
        }

        public ActionResult ChiTietFAQ(int id)
         {
             if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

             FAQ_NoiDung seFAQ = db.FAQ_NoiDung.SingleOrDefault(n => n.Id == id);

             if (!KTNhom_FAQ(seFAQ.MaDanhMuc.Value))
                 return RedirectToAction("IndexVanBan");

             return PartialView(seFAQ);
         }

         public bool KTNhom_FAQ(int idFAQ)
         {
             NhanVien layNV = (NhanVien)Session["DangNhap"];
             IEnumerable<Nhom_FAQ> lstN_FAQ = db.Nhom_FAQ;
            //lay nhom 
            IEnumerable<Nhom_Nhanvien> ieNhanVienVaNhom = db.Nhom_Nhanvien.Where(n => n.IdNhanVien == layNV.Id);
            foreach (var item in ieNhanVienVaNhom)
            {
                if (lstN_FAQ.Where(n => n.MaFAQ == idFAQ && n.MaNhom == item.MaNhom).Count() > 0)
                    return true;
            }
             return false;
         }

        public ActionResult _pDanhMuc()
        {
            ViewBag.DanhMuc = db.DanhMucs.Where(n => n.DanhMucCha == 0 && n.SuDung==true).OrderBy(n=>n.ThuTu) ;

            return PartialView();
        }

        public ActionResult _pLoaiVanBan()
        {
            ViewBag.LVB = db.LoaiTaiLieux.OrderBy(n => n.CapTaiLieu);
            ViewBag.VB = db.TaiLieux;

            return PartialView();
        }

        public ActionResult _pThongTinFooter()
        {
            List<CauHinh> ch = db.CauHinhs.ToList();
            ViewBag.ftEmail = ch.SingleOrDefault(n => n.MaCauHinh == "EMAIL-FEEDBACK").DuLieu;
            ViewBag.ftDienThoai = ch.SingleOrDefault(n => n.MaCauHinh == "HT-DIENTHOAI").DuLieu;
            ViewBag.ftFax = ch.SingleOrDefault(n => n.MaCauHinh == "HT-FAX").DuLieu;
            ViewBag.ftDiaChi = ch.SingleOrDefault(n => n.MaCauHinh == "HT-DIA-CHI").DuLieu;

            return PartialView();
        }

        [HttpPost]
        public ActionResult GhiChu(string NoiDung, int MaVanBan)
        {
             if (Session["DangNhap"] != null)
            {
                NhanVien nv=(NhanVien)Session["DangNhap"];
                GhiChu ktGhiChu = db.GhiChus.SingleOrDefault(n => n.MaVanBan == MaVanBan && n.IdNhanVien == nv.Id);
                if (ktGhiChu == null)
                {
                    GhiChu themGC = new GhiChu();
                    themGC.MaVanBan = MaVanBan;
                    themGC.IdNhanVien = nv.Id;
                    themGC.NoiDung = NoiDung;
                    db.GhiChus.Add(themGC);
                    db.SaveChanges();
                }
                else
                {
                    ktGhiChu.NoiDung = NoiDung;
                    db.SaveChanges();
                }
                   
             }
            return Content("");
        }

        public ActionResult LuuTruVanBan(int id)
        {
            if (Session["DangNhap"] != null)
            {
                NhanVien nv = (NhanVien)Session["DangNhap"];
                int iKT = db.VBLuuTrus.Count(n => n.MaVanBan == id && n.IdNhanVien == nv.Id);
                if (iKT == 0)
                {
                    VBLuuTru themVBLT = new VBLuuTru();
                    themVBLT.MaVanBan = id;
                    themVBLT.IdNhanVien = nv.Id;
                    db.VBLuuTrus.Add(themVBLT);
                    db.SaveChanges();

                    TempData["thongbao"] = "<script>$('#pthongbao').text('Văn bản đã được lưu !'); $('#btn-thongbao2').trigger('click');</script>";
                }
            }

            TaiLieu layTaiLieu = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == id);
            string sUrl = "/van-ban/" + Tools.RemoveDiacritics(layTaiLieu.TenTaiLieu) + "-" + id;

            return Redirect(sUrl);
           // return RedirectToAction("ChiTietVanBan", "TrangChinh", new { @id = id });
        }

        public ActionResult XoaLuuTruVanBan(int id)
        {
            if (Session["DangNhap"] != null)
            {
                NhanVien nv = (NhanVien)Session["DangNhap"];
                VBLuuTru xoaVBLT = db.VBLuuTrus.SingleOrDefault(n => n.MaVanBan==id && n.IdNhanVien==nv.Id);
                db.VBLuuTrus.Remove(xoaVBLT);
                db.SaveChanges();

                TempData["thongbao"] = "<script>$('#pthongbao').text('Đã xóa khỏi văn bản cá nhân !'); $('#btn-thongbao2').trigger('click');</script>";
            }

            TaiLieu layTaiLieu = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == id);
            string sUrl = "/van-ban/" + Tools.RemoveDiacritics(layTaiLieu.TenTaiLieu) + "-" + id;

            return Redirect(sUrl);
           // return RedirectToAction("ChiTietVanBan", "TrangChinh", new { @id = id });
        }

        public ActionResult GuiYKien(int MaVanBan,string HoTen,string Email, string DienThoai,string NoiDung)
        {
            string sMailGui = System.Configuration.ConfigurationManager.AppSettings["MailSend"];
            string sPass = System.Configuration.ConfigurationManager.AppSettings["MailPass"];
            string sHost = System.Configuration.ConfigurationManager.AppSettings["MailHost"];
            string sPort = System.Configuration.ConfigurationManager.AppSettings["MailPort"];
            string sTieuDe = System.Configuration.ConfigurationManager.AppSettings["MailSubject"];
            // luu y kien 
            FeedBack themFeedBack = new FeedBack();
            themFeedBack.HoTenGui = HoTen;
            themFeedBack.Email = Email;
            themFeedBack.DienThoai = DienThoai;
            themFeedBack.NoiDung = NoiDung;
            themFeedBack.NgayGui = DateTime.Now;
            db.FeedBacks.Add(themFeedBack);
            db.SaveChanges();
            TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVanBan);
            //lay mail ban he thong
            CauHinh ch = db.CauHinhs.SingleOrDefault(n => n.MaCauHinh == "EMAIL-FEEDBACK");

            // gui mail
            var fromAddress = new MailAddress(sMailGui, "Hệ thống Coteccons");
            var toAddress = new MailAddress(ch.DuLieu, "To Name");
            string fromPassword = sPass;
            string subject = sTieuDe;
             string body = "<b>Tên tài liệu: </b>"+ tl.TenTaiLieu;
             body += "<br/ > <b>Họ tên người gửi: </b>"+HoTen;
             body += "<br /> <b>Email: </b>" + Email;
             body += "<br /> <b>Điện thoại: </b>" + DienThoai;
             body += "<br /> <b>Nội dung: </b> <br/>" + NoiDung;

            var smtp = new SmtpClient
            {
                Host = sHost,
                Port =int.Parse(sPort),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                IsBodyHtml=true,
                Subject = subject,
                Body =   body
            })
            {
                smtp.Send(message);
            }
            return Content("1");
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