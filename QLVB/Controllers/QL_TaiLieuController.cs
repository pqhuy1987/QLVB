using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using QLVB.Models;

namespace QLVB.Controllers
{
    [Authorize(Roles = "QL-VAN-BAN")]
    public class QL_TaiLieuController : Controller
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

        public ActionResult DanhSach(int? page, string TenVanBan, string SoHieu, string LoaiVanBan, string NoiBanHanh, string NguoiKy, string NgayBanHanhfrom, string NgayBanHanhto, string NgayHieuLucfrom, string NgayHieuLucto, string TinhTrang, string PhongBan, string MaHieu, string LanBanHanh, string BaoMat)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");
            Tools tool = new Tools();

            ViewBag.TinhTrang = tool.DMTinhTrang(!string.IsNullOrEmpty(TinhTrang) ? Convert.ToInt32(TinhTrang) : 0);
            ViewBag.LoaiVanBan = new SelectList(db.LoaiTaiLieux, "MaLoaiTL", "TenLoaiTL", LoaiVanBan);
            ViewBag.PhongBan = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", PhongBan);
            ViewBag.NoiBanHanh = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", NoiBanHanh);

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            // tim kiem
            ViewBag.TenVanBan = TenVanBan;
            ViewBag.SoHieu = SoHieu;
            ViewBag.NoiBanHanhi = NoiBanHanh;
            ViewBag.NguoiKy = NguoiKy;
            ViewBag.NgayBanHanhfrom = NgayBanHanhfrom;
            ViewBag.NgayBanHanhto = NgayBanHanhto;
            ViewBag.NgayHieuLucfrom = NgayHieuLucfrom;
            ViewBag.NgayHieuLucto = NgayHieuLucto;
            ViewBag.LoaiVanBani = LoaiVanBan;
            ViewBag.TinhTrangi = TinhTrang;
            ViewBag.PhongBani = PhongBan;
            ViewBag.MaHieu = MaHieu;
            ViewBag.LanBanHanh = LanBanHanh;
            ViewBag.BaoMat = BaoMat;

            IEnumerable<TaiLieu> lstTaiLieu = db.TaiLieux;
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

            if (!string.IsNullOrEmpty(LoaiVanBan))
            {
                int iLoaiVanBan = int.Parse(LoaiVanBan);
                lstTaiLieu = lstTaiLieu.Where(n => (n.LoaiVanBan != null && n.LoaiVanBan.Value == iLoaiVanBan));
            }
            if (!string.IsNullOrEmpty(TinhTrang))
            {
                lstTaiLieu = lstTaiLieu.Where(n => (n.TinhTrang != null && n.TinhTrang.Trim() == TinhTrang.Trim()));
            }
            if (!string.IsNullOrEmpty(PhongBan))
            {
                //int iPhongBan = int.Parse(PhongBan);
                //lstTaiLieu = lstTaiLieu.Where(n => (n.PhongBan != null && n.PhongBan.Value == iPhongBan));

                // BEGIN
                lstTaiLieu = lstTaiLieu.Where(n => ("," + n.DonViApDung + ",").Contains("," + PhongBan + ","));
                // END
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
            if (!string.IsNullOrEmpty(BaoMat) && BaoMat.Trim() == "true")
            {
                lstTaiLieu = lstTaiLieu.Where(n => (n.BaoMat != null && n.BaoMat.Value == true));
            }

            return View(lstTaiLieu.ToList().ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult DanhSach(FormCollection c)
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
            string sBaoMat = c["BaoMat"];

            return RedirectToAction("DanhSach", "QL_TaiLieu", new { @TenVanBan = sTenVanBan, @SoHieu = sSoHieu, @NoiBanHanh = sNoiBanHanh, @LoaiVanBan = sLoaiVanBan, @NguoiKy = sNguoiKy, @NgayBanHanhfrom = sNgayBanHanhfrom, @NgayBanHanhto = sNgayBanHanhto, @NgayHieuLucfrom = sNgayHieuLucfrom, @NgayHieuLucto = sNgayHieuLucto, @TinhTrang = sTinhTrang, @PhongBan = sPhongBan, @MaHieu = sMaHieu, @LanBanHanh = sLanBanHanh, @BaoMat = sBaoMat });
        }

        [HttpGet]
        public ActionResult ThemMoi()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");
            Tools tool = new Tools();

            ViewBag.LoaiVanBan = new SelectList(db.LoaiTaiLieux, "MaLoaiTL", "TenLoaiTL");
            ViewBag.PhongBan = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong");
            ViewBag.NoiBanHanh = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong");
            ViewBag.TinhTrang = tool.DMTinhTrang(0);

            // BEGIN
            ViewBag.DonViApDung = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong");
            // END

            Session["lbl"] = "Tạo mới";

            return View();
        }

        [HttpPost]
        public ActionResult ThemMoi(TaiLieu tl, FormCollection c) // them moi fill trong roi nhay qua trang chinh sua
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            if (ModelState.IsValid)
            {
                if (tl.BaoMat == null)
                    tl.BaoMat = false;

                if (!string.IsNullOrEmpty(c["NgayBanHanhs"]))
                    tl.NgayBanHanh = DateTime.ParseExact(c["NgayBanHanhs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(c["NgayHieuLucs"]))
                    tl.NgayHieuLuc = DateTime.ParseExact(c["NgayHieuLucs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                tl.NgayTaiLen = DateTime.Now;
                tl.LanHieuChinh = 0;
                db.TaiLieux.Add(tl);
                db.SaveChanges();

                // BEGIN
                Tools.WriteLog("Quản lý tài liệu", "Thêm mới", string.Format("Thêm mới tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                // END

                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Lưu thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }

            return RedirectToAction("ChinhSua", "QL_TaiLieu", new { @id = tl.MaTaiLieu });
        }

        public ActionResult ChinhSua(int id, string e)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            if (!string.IsNullOrEmpty(e))
                Session["lbl"] = "Cập nhật";

            Tools tool = new Tools();
            TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == id);

            ViewBag.LoaiVanBan = new SelectList(db.LoaiTaiLieux, "MaLoaiTL", "TenLoaiTL", tl.LoaiVanBan);
            ViewBag.PhongBan = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", tl.PhongBan);
            ViewBag.NoiBanHanh = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", tl.NoiBanHanh);
            ViewBag.TinhTrang = tool.DMTinhTrang(Convert.ToInt32(tl.TinhTrang));
            var vVanBan = db.TaiLieux.Where(n => n.MaTaiLieu != id).Select(s => new { MaTaiLieu = s.MaTaiLieu, TenSoHieu = s.TenTaiLieu + " - " + s.SoHieu }).ToList();
            ViewBag.VanBan = new SelectList(vVanBan, "MaTaiLieu", "TenSoHieu");

            // Lay cac van ban lien quan
            ViewBag.VBCC = db.VB_VBCC.Where(n => n.MaVanBan == id);
            ViewBag.VBSD = db.VB_VBSD.Where(n => n.MaVanBan == id);
            ViewBag.VBHD = db.VB_VBHD.Where(n => n.MaVanBan == id);
            ViewBag.VBLQ = db.VB_VBLQ.Where(n => n.MaVanBan == id);
            ViewBag.Revision = db.Revisions.Where(n => n.MaVanBan == id);

            // BEGIN
            ViewBag.BMLQ = db.VB_BMLQ.Where(n => n.MaVanBan == id);
            ViewBag.DonViApDung = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong");
            // END

            return View(tl);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ChinhSua(TaiLieu tl, FormCollection c)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();
            ViewBag.LoaiVanBan = new SelectList(db.LoaiTaiLieux, "MaLoaiTL", "TenLoaiTL", tl.LoaiVanBan);
            ViewBag.PhongBan = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", tl.PhongBan);
            ViewBag.NoiBanHanh = new SelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", tl.NoiBanHanh);
            ViewBag.TinhTrang = tool.DMTinhTrang(Convert.ToInt32(tl.TinhTrang));

            var vVanBan = db.TaiLieux.Where(n => n.MaTaiLieu != tl.MaTaiLieu).Select(s => new { MaTaiLieu = s.MaTaiLieu, TenSoHieu = s.TenTaiLieu + " - " + s.SoHieu }).ToList();
            ViewBag.VanBan = new SelectList(vVanBan, "MaTaiLieu", "TenSoHieu");

            // lay cac van ban lien quan
            ViewBag.VBCC = db.VB_VBCC.Where(n => n.MaVanBan == tl.MaTaiLieu);
            ViewBag.VBSD = db.VB_VBSD.Where(n => n.MaVanBan == tl.MaTaiLieu);
            ViewBag.VBHD = db.VB_VBHD.Where(n => n.MaVanBan == tl.MaTaiLieu);
            ViewBag.VBLQ = db.VB_VBLQ.Where(n => n.MaVanBan == tl.MaTaiLieu);
            ViewBag.Revision = db.Revisions.Where(n => n.MaVanBan == tl.MaTaiLieu);

            // BEGIN
            ViewBag.BMLQ = db.VB_BMLQ.Where(n => n.MaVanBan == tl.MaTaiLieu);
            string[] arrDonVi = null;
            if (!string.IsNullOrEmpty(tl.DonViApDung))
            {
                if (tl.DonViApDung.Contains(","))
                {
                    arrDonVi = tl.DonViApDung.Split(',');
                }
                else
                {
                    arrDonVi = new string[] { tl.DonViApDung };
                }
            }
            ViewBag.DonViApDung = new MultiSelectList(db.DMPhongBans.Where(n => n.KichHoat == true), "Id", "TenPhong", arrDonVi);
            // END

            TaiLieu tlCu = db.TaiLieux.AsNoTracking().SingleOrDefault(n => n.MaTaiLieu == tl.MaTaiLieu);

            if (ModelState.IsValid)
            {
                if (tl.BaoMat == null)
                    tl.BaoMat = false;

                if (!string.IsNullOrEmpty(c["NgayBanHanhs"]))
                    tl.NgayBanHanh = DateTime.ParseExact(c["NgayBanHanhs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(c["NgayHieuLucs"]))
                    tl.NgayHieuLuc = DateTime.ParseExact(c["NgayHieuLucs"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                // BEGIN
                if (!string.IsNullOrEmpty(tl.DonViApDung))
                {
                    try
                    {
                        if (tl.DonViApDung.Contains(","))
                        {

                            tl.PhongBan = int.Parse(tl.DonViApDung.Split(',')[0]);
                        }
                        else
                        {
                            tl.PhongBan = int.Parse(tl.DonViApDung);
                        }
                    }
                    catch { }
                }
                // END

                tl.fileDOCA = tlCu.fileDOCA;
                tl.fileDOCV = tlCu.fileDOCV;
                tl.fileGOC = tlCu.fileGOC;
                tl.filePDF = tlCu.filePDF;
                tl.LanHieuChinh += 1;
                tl.NgayTaiLen = DateTime.Now;

                db.Entry(tl).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // BEGIN
                Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Chỉnh sửa tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                // END

                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Lưu thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
                ModelState.Clear();
            }

            return View(tl);
        }

        public ActionResult Xoa(int id)
        {
            IEnumerable<VB_VBCC> xoaVBCC = db.VB_VBCC.Where(n => n.MaVanBan == id);
            db.VB_VBCC.RemoveRange(xoaVBCC);
            IEnumerable<VB_VBSD> xoaVBSD = db.VB_VBSD.Where(n => n.MaVanBan == id);
            db.VB_VBSD.RemoveRange(xoaVBSD);
            IEnumerable<VB_VBHD> xoaVBHD = db.VB_VBHD.Where(n => n.MaVanBan == id);
            db.VB_VBHD.RemoveRange(xoaVBHD);
            IEnumerable<VB_VBLQ> xoaVBLQ = db.VB_VBLQ.Where(n => n.MaVanBan == id);
            db.VB_VBLQ.RemoveRange(xoaVBLQ);

            xoaVBCC = db.VB_VBCC.Where(n => n.MaVanBanCC == id);
            db.VB_VBCC.RemoveRange(xoaVBCC);
            xoaVBSD = db.VB_VBSD.Where(n => n.MaVanBanSD == id);
            db.VB_VBSD.RemoveRange(xoaVBSD);
            xoaVBHD = db.VB_VBHD.Where(n => n.MaVanBanHD == id);
            db.VB_VBHD.RemoveRange(xoaVBHD);
            xoaVBLQ = db.VB_VBLQ.Where(n => n.MaVanBanLQ == id);
            db.VB_VBLQ.RemoveRange(xoaVBLQ);
            db.SaveChanges();

            TaiLieu xoaTaiLieu = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == id);

            TaiLieu tl = xoaTaiLieu;

            db.TaiLieux.Remove(xoaTaiLieu);
            db.SaveChanges();

            // BEGIN
            Tools.WriteLog("Quản lý tài liệu", "Xóa", string.Format("Xóa tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
            // END

            TempData["thongbao"] = "<script> $('#div-pthongbao').text('Xóa thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public ActionResult LuuVBCanCu(int MaVBChinh, int MaVBCanCu)  // luu van ban can cu
        {
            int iKT = db.VB_VBCC.Where(n => n.MaVanBan == MaVBChinh && n.MaVanBanCC == MaVBCanCu).Count();
            if (iKT == 0)
            {
                VB_VBCC luuVBCC = new VB_VBCC();
                luuVBCC.MaVanBan = MaVBChinh;
                luuVBCC.MaVanBanCC = MaVBCanCu;
                db.VB_VBCC.Add(luuVBCC);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBCanCu);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Thêm văn bản căn cứ [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_VBCC> lstVBCanCu = db.VB_VBCC.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBCanCu)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBCC btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanCC }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBCC').text('(" + lstVBCanCu.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult XoaVBCanCu(int MaVBChinh, int idXoa)  // xoa van ban can cu
        {
            VB_VBCC xoaVBCC = db.VB_VBCC.SingleOrDefault(n => n.ID == idXoa);
            if (xoaVBCC != null)
            {
                db.VB_VBCC.Remove(xoaVBCC);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == idXoa);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Xoá văn bản căn cứ [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_VBCC> lstVBCanCu = db.VB_VBCC.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBCanCu)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBCC btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanCC }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBCC').text('(" + lstVBCanCu.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult LuuVBSuaDoi(int MaVBChinh, int MaVBSuaDoi)  // luu van ban sua doi
        {
            int iKT = db.VB_VBSD.Where(n => n.MaVanBan == MaVBChinh && n.MaVanBanSD == MaVBSuaDoi).Count();
            if (iKT == 0)
            {
                VB_VBSD luuVBSD = new VB_VBSD();
                luuVBSD.MaVanBan = MaVBChinh;
                luuVBSD.MaVanBanSD = MaVBSuaDoi;
                db.VB_VBSD.Add(luuVBSD);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBSuaDoi);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Thêm văn bản sửa đổi [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_VBSD> lstVBSuaDoi = db.VB_VBSD.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBSuaDoi)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBSD btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanSD }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBSD').text('(" + lstVBSuaDoi.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult XoaVBSuaDoi(int MaVBChinh, int idXoa)  // xoa van ban sua doi
        {
            VB_VBSD xoaVBSD = db.VB_VBSD.SingleOrDefault(n => n.ID == idXoa);
            if (xoaVBSD != null)
            {
                db.VB_VBSD.Remove(xoaVBSD);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == idXoa);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Xóa văn bản sửa đổi [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_VBSD> lstVBSuaDoi = db.VB_VBSD.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBSuaDoi)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBSD btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanSD }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBSD').text('(" + lstVBSuaDoi.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult LuuVBLienQuan(int MaVBChinh, int MaVBLienQuan)  // luu van ban sua doi
        {
            int iKT = db.VB_VBLQ.Where(n => n.MaVanBan == MaVBChinh && n.MaVanBanLQ == MaVBLienQuan).Count();
            if (iKT == 0)
            {
                VB_VBLQ luuVBLQ = new VB_VBLQ();
                luuVBLQ.MaVanBan = MaVBChinh;
                luuVBLQ.MaVanBanLQ = MaVBLienQuan;
                db.VB_VBLQ.Add(luuVBLQ);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBLienQuan);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Thêm văn bản liên quan [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_VBLQ> lstVBLienQuan = db.VB_VBLQ.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBLienQuan)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBLQ btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanLQ }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBLQ').text('(" + lstVBLienQuan.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult XoaVBLienQuan(int MaVBChinh, int idXoa)  // xoa van ban sua doi
        {
            VB_VBLQ xoaVBLQ = db.VB_VBLQ.SingleOrDefault(n => n.ID == idXoa);
            if (xoaVBLQ != null)
            {
                db.VB_VBLQ.Remove(xoaVBLQ);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == idXoa);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Xóa văn bản liên quan [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_VBLQ> lstVBLienQuan = db.VB_VBLQ.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBLienQuan)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBLQ btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanLQ }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBLQ').text('(" + lstVBLienQuan.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult LuuVBHuongDan(int MaVBChinh, int MaVBHuongDan)  // luu van ban sua doi
        {
            int iKT = db.VB_VBHD.Where(n => n.MaVanBan == MaVBChinh && n.MaVanBanHD == MaVBHuongDan).Count();
            if (iKT == 0)
            {
                VB_VBHD luuVBHD = new VB_VBHD();
                luuVBHD.MaVanBan = MaVBChinh;
                luuVBHD.MaVanBanHD = MaVBHuongDan;
                db.VB_VBHD.Add(luuVBHD);
                db.SaveChanges();

                string sHtml = "";
                IEnumerable<VB_VBHD> lstVBHuongDan = db.VB_VBHD.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBHuongDan)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBHD btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanHD }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBHD').text('(" + lstVBHuongDan.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult XoaVBHuongDan(int MaVBChinh, int idXoa)  // xoa van ban sua doi
        {
            VB_VBHD xoaVBHD = db.VB_VBHD.SingleOrDefault(n => n.ID == idXoa);
            if (xoaVBHD != null)
            {
                db.VB_VBHD.Remove(xoaVBHD);
                db.SaveChanges();

                string sHtml = "";
                IEnumerable<VB_VBHD> lstVBHuongDan = db.VB_VBHD.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBHuongDan)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaVBHD btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaVanBanHD }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numVBHD').text('(" + lstVBHuongDan.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        // BEGIN
        [HttpPost]
        public ActionResult LuuBMLienQuan(int MaVBChinh, int MaVBLienQuan)  // luu bieu mau lien quan
        {
            int iKT = db.VB_BMLQ.Where(n => n.MaVanBan == MaVBChinh && n.MaBieuMau == MaVBLienQuan).Count();
            if (iKT == 0)
            {
                VB_BMLQ luuVBLQ = new VB_BMLQ();
                luuVBLQ.MaVanBan = MaVBChinh;
                luuVBLQ.MaBieuMau = MaVBLienQuan;
                db.VB_BMLQ.Add(luuVBLQ);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBLienQuan);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Thêm biểu mẫu liên quan [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_BMLQ> lstVBLienQuan = db.VB_BMLQ.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBLienQuan)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaBMLQ btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaBieuMau }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numBMLQ').text('(" + lstVBLienQuan.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult XoaBMLienQuan(int MaVBChinh, int idXoa)  // xoa bieu mau lien quan
        {
            VB_BMLQ xoaVBLQ = db.VB_BMLQ.SingleOrDefault(n => n.ID == idXoa);
            if (xoaVBLQ != null)
            {
                db.VB_BMLQ.Remove(xoaVBLQ);
                db.SaveChanges();

                // BEGIN
                try
                {
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == MaVBChinh);
                    TaiLieu tlcc = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == idXoa);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Xóa biểu mẫu liên quan [Id: {4}; Số hiệu: {5}; Mã hiệu: {6}; Trích yếu: {7}] của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu, tlcc.MaTaiLieu, tlcc.SoHieu, tlcc.MaHieu, tlcc.TenTaiLieu));
                }
                catch (Exception) { }
                // END

                string sHtml = "";
                IEnumerable<VB_BMLQ> lstVBLienQuan = db.VB_BMLQ.Include("TaiLieu1").Where(n => n.MaVanBan == MaVBChinh);
                foreach (var item in lstVBLienQuan)
                {
                    sHtml += "<a id='" + item.ID + "' class='xoaBMLQ btn btn-danger fa fa-times'></a> <a class=' text-success' href=" + Url.Action("ChinhSua", "QL_TaiLieu", new { @id = item.MaBieuMau }) + "><i class='fa fa-caret-right '></i> " + item.TaiLieu1.TenTaiLieu + " </a> <hr />";
                }
                sHtml += "<script>$('.numBMLQ').text('(" + lstVBLienQuan.Count() + ")')</script>";
                return Content(sHtml);
            }
            return Content("0");
        }
        // END

        [HttpPost]
        public ActionResult LuuFileGoc()  // luu file ajax
        {

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

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Upload/TaiLieuGoc/"), fname);
                        file.SaveAs(fname);
                    }

                    // luu ten vao csdl
                    int iMaTaiLieu = Convert.ToInt32(Request.Form["MaTaiLieu"]);
                    TaiLieu luuTL = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                    luuTL.fileGOC = files[0].FileName;
                    db.SaveChanges();

                    // BEGIN
                    TaiLieu tl = luuTL;
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Lưu file gốc của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                    // END

                    // Returns message that successfully uploaded  
                    return Json("<iframe src='/Upload/TaiLieuGoc/" + files[0].FileName + "' style='width:100%;' height='500'></iframe>");

                    /// TMP COMMENT
                    //if (tl.fileGOC.ToLower().EndsWith(".pdf"))
                    //{
                    //    var lstRemove = db.PdfPages.Where(x => x.DocumentId == iMaTaiLieu).ToList();
                    //    if (lstRemove != null)
                    //    {
                    //        foreach (var item in lstRemove)
                    //        {
                    //            string strPath = Path.Combine(item.ImageUrl);
                    //            if (System.IO.File.Exists(strPath))
                    //            {
                    //                System.IO.File.Delete(strPath);
                    //            }
                    //        }
                    //        db.PdfPages.RemoveRange(lstRemove);
                    //        db.SaveChanges();
                    //    }

                    //    return Json("<iframe src='/Pdf/Preview/" + iMaTaiLieu + "' style='width:100%;' height='500'></iframe>");
                    //}
                    //else
                    //{
                    //    return Json("<p>PHẦN MỀM CHỈ HỖ TRỢ XEM ONLINE VĂN BẢN GỐC ĐỊNH DẠNG PDF</p><p><a href='/Upload/TaiLieuGoc/" + tl.fileGOC + "'>Tải về</a></p>");
                    //}
                }
                catch (Exception ex)
                {
                    return Json("0");
                }
            }
            else
            {
                return Json("0");
            }
        }

        [HttpPost]
        public ActionResult LuuFilePDF()  // luu file ajax
        {

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

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Upload/filePDF/"), fname);
                        file.SaveAs(fname);


                    }

                    // luu ten vao csdl
                    int iMaTaiLieu = Convert.ToInt32(Request.Form["MaTaiLieu"]);
                    string sNamef = files[0].FileName;

                    TaiLieu luuTL = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                    luuTL.filePDF = sNamef;
                    db.SaveChanges();

                    // BEGIN
                    TaiLieu tl = luuTL;
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Lưu file pdf của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                    // END

                    // Returns message that successfully uploaded  
                    return Json(" <a href='/Upload/filePDF/" + sNamef + "'>" + sNamef + "</a>");
                }
                catch (Exception ex)
                {
                    return Json("0");
                }
            }
            else
            {
                return Json("0");
            }
        }

        [HttpPost]
        public ActionResult LuuFileDOCV()  // luu file ajax
        {

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

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Upload/fileDOCTV/"), fname);
                        file.SaveAs(fname);


                    }

                    // luu ten vao csdl
                    int iMaTaiLieu = Convert.ToInt32(Request.Form["MaTaiLieu"]);
                    string sNamef = files[0].FileName;

                    TaiLieu luuTL = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                    luuTL.fileDOCV = sNamef;
                    db.SaveChanges();

                    // BEGIN
                    TaiLieu tl = luuTL;
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Lưu file .doc tiếng Việt của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                    // END

                    // Returns message that successfully uploaded  
                    return Json(" <a href='/Upload/fileDOCTV/" + sNamef + "'>" + sNamef + "</a>");
                }
                catch (Exception ex)
                {
                    return Json("0");
                }
            }
            else
            {
                return Json("0");
            }
        }

        [HttpPost]
        public ActionResult LuuFileDOCA()  // luu file ajax
        {

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

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Upload/fileDOCTA/"), fname);
                        file.SaveAs(fname);


                    }

                    // luu ten vao csdl
                    int iMaTaiLieu = Convert.ToInt32(Request.Form["MaTaiLieu"]);
                    string sNamef = files[0].FileName;

                    TaiLieu luuTL = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                    luuTL.fileDOCA = sNamef;
                    db.SaveChanges();

                    // BEGIN
                    TaiLieu tl = luuTL;
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Lưu file .doc tiếng Anh của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                    // END

                    // Returns message that successfully uploaded  
                    return Json(" <a href='/Upload/fileDOCTA/" + sNamef + "'>" + sNamef + "</a>");
                }
                catch (Exception ex)
                {
                    return Json("0");
                }
            }
            else
            {
                return Json("0");
            }
        }

        [HttpPost]
        public ActionResult LuuRevisionTV()  // luu file revision ajax
        {

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

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Upload/revision/"), fname);
                        file.SaveAs(fname);


                    }

                    // luu ten vao csdl
                    int iMaRevision = Convert.ToInt32(Request.Form["MaRevision"]);
                    string sTenRevision = Request.Form["TenRevision"];
                    int iMaTaiLieu = Convert.ToInt32(Request.Form["MaTaiLieu"]);
                    string sNamef = files[0].FileName;
                    if (iMaRevision == 0)
                    {
                        Revision themRevision = new Revision();
                        themRevision.MaVanBan = iMaTaiLieu;
                        themRevision.TenRevision = sTenRevision;
                        themRevision.fileTV = sNamef;
                        db.Revisions.Add(themRevision);
                        db.SaveChanges();
                        iMaRevision = themRevision.MaRevision;
                    }
                    else
                    {
                        Revision suaRevision = db.Revisions.SingleOrDefault(n => n.MaRevision == iMaRevision);
                        suaRevision.TenRevision = sTenRevision;
                        suaRevision.fileTV = sNamef;
                        db.SaveChanges();
                    }

                    // BEGIN
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Lưu revision tiếng Việt của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                    // END

                    // Returns message that successfully uploaded  
                    return Json("<input hidden class='MaRevision' value=" + iMaRevision + " /><a href='/Upload/revision/" + sNamef + "'>" + sNamef + "</a> <button type='button' class='btn-xoa-revisionTV btn btn-danger fa fa-times'></button>");
                }
                catch (Exception ex)
                {
                    return Json("0");
                }
            }
            else
            {
                return Json("0");
            }
        }

        [HttpPost]
        public ActionResult XoaRevisionTV(int MaRevision)  // xoa file revision ajax
        {

            Revision xoaRevision = db.Revisions.SingleOrDefault(n => n.MaRevision == MaRevision);
            if (xoaRevision != null)
            {
                int? iMaTaiLieu = xoaRevision.MaVanBan;

                var deletePath = Server.MapPath("~/Upload/revision/" + xoaRevision.fileTV);
                if (System.IO.File.Exists(deletePath))
                {
                    System.IO.File.Delete(deletePath);
                }

                xoaRevision.fileTV = null;
                db.SaveChanges();

                // BEGIN
                TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Xóa revision tiếng Việt của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                // END

                // Returns message that successfully uploaded  
                return Content("<input hidden class='MaRevision' value=" + MaRevision + " /><input type='file'  class='refileV form-control' accept='application/.doc,.docx' />");
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult LuuRevisionTA()  // luu file revision ajax
        {

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

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Upload/revision/"), fname);
                        file.SaveAs(fname);


                    }

                    // luu ten vao csdl
                    int iMaRevision = Convert.ToInt32(Request.Form["MaRevision"]);
                    string sTenRevision = Request.Form["TenRevision"];
                    int iMaTaiLieu = Convert.ToInt32(Request.Form["MaTaiLieu"]);
                    string sNamef = files[0].FileName;
                    if (iMaRevision == 0)
                    {
                        Revision themRevision = new Revision();
                        themRevision.MaVanBan = iMaTaiLieu;
                        themRevision.TenRevision = sTenRevision;
                        themRevision.fileTA = sNamef;
                        db.Revisions.Add(themRevision);
                        db.SaveChanges();
                        iMaRevision = themRevision.MaRevision;
                    }
                    else
                    {
                        Revision suaRevision = db.Revisions.SingleOrDefault(n => n.MaRevision == iMaRevision);
                        suaRevision.TenRevision = sTenRevision;
                        suaRevision.fileTA = sNamef;
                        db.SaveChanges();
                    }

                    // BEGIN
                    TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                    Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Lưu revision tiếng Anh của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                    // END

                    // Returns message that successfully uploaded  
                    return Json("<input hidden class='MaRevision' value=" + iMaRevision + " /><a href='/Upload/revision/" + sNamef + "'>" + sNamef + "</a> <button type='button' class='btn-xoa-revisionTA btn btn-danger fa fa-times'></button>");
                }
                catch (Exception ex)
                {
                    return Json("0");
                }
            }
            else
            {
                return Json("0");
            }
        }

        [HttpPost]
        public ActionResult XoaRevisionTA(int MaRevision)  // xoa file revision ajax
        {
            Revision xoaRevision = db.Revisions.SingleOrDefault(n => n.MaRevision == MaRevision);
            if (xoaRevision != null)
            {
                int? iMaTaiLieu = xoaRevision.MaVanBan;

                var deletePath = Server.MapPath("~/Upload/revision/" + xoaRevision.fileTA);
                if (System.IO.File.Exists(deletePath))
                {
                    System.IO.File.Delete(deletePath);
                }

                xoaRevision.fileTA = null;
                db.SaveChanges();

                // BEGIN
                TaiLieu tl = db.TaiLieux.SingleOrDefault(n => n.MaTaiLieu == iMaTaiLieu);
                Tools.WriteLog("Quản lý tài liệu", "Chỉnh sửa", string.Format("Xóa revision tiếng Anh của tài liệu [Id: {0}; Số hiệu: {1}; Mã hiệu: {2}; Trích yếu: {3}]", tl.MaTaiLieu, tl.SoHieu, tl.MaHieu, tl.TenTaiLieu));
                // END

                // Returns message that successfully uploaded  
                return Content("<input hidden class='MaRevision' value=" + MaRevision + " /><input type='file'  class='refileA form-control' accept='application/.doc,.docx' />");
            }
            return Content("0");
        }

        [HttpPost]
        public ActionResult XoaRevisionRow(int MaRevision)  // xoa file revision ajax
        {
            Revision xoaRevision = db.Revisions.SingleOrDefault(n => n.MaRevision == MaRevision);
            if (xoaRevision != null)
            {
                var deletePath = Server.MapPath("~/Upload/revision/" + xoaRevision.fileTA);
                if (System.IO.File.Exists(deletePath))
                {
                    System.IO.File.Delete(deletePath);
                }
                deletePath = Server.MapPath("~/Upload/revision/" + xoaRevision.fileTV);
                if (System.IO.File.Exists(deletePath))
                {
                    System.IO.File.Delete(deletePath);
                }
                db.Revisions.Remove(xoaRevision);
                db.SaveChanges();
            }
            return Content("");
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