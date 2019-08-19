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
    [Authorize(Roles = "QL-TIN-TUC")]
    public class QL_TinTucController : Controller
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

        public ActionResult DanhSach(int? page, string TieuDe, string NoiDung, string NgayDang, string MaDanhMuc, string LoaiTinTuc, string MaLoaiCT)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            Tools tool = new Tools();
            ViewBag.TieuDe = TieuDe;
            ViewBag.NoiDung = NoiDung;
            ViewBag.NgayDang = NgayDang;
            ViewBag.MucTin = MaDanhMuc;
            ViewBag.LoaiTin = LoaiTinTuc;
            ViewBag.MaLoaiCT = new SelectList(db.LoaiCongTruongs, "MaLoaiCT", "TenLoaiCT", MaLoaiCT);
            ViewBag.LoaiTinTuc = new SelectList(tool.DMTinTuc(0), "Value", "Text",LoaiTinTuc);
            ViewBag.MaDanhMuc = new SelectList(db.DanhMucs.Where(n => n.DanhMucCha == Tools.MaDanhMucTin), "MaDanhMuc", "TenDanhMuc", MaDanhMuc);

            IEnumerable<TinTuc> lstTinTuc = db.TinTucs;

            if (!string.IsNullOrEmpty(TieuDe))
            {
                //List<TinTuc> lstTam = new List<TinTuc>();  // lst tam dung de loc du lieu tu lst chinh
                //List<TinTuc> lstDaLoc = new List<TinTuc>(); // lst chua du lieu da loc ra

                //string[] sTieuDe = TieuDe.Split(' ');

                //foreach (var item in sTieuDe)
                //{
                //    if (!string.IsNullOrEmpty(item))
                //    {
                //        lstTam = lstTinTuc.Where(n => n.TieuDe != null && Tools.RemoveDiacritics(n.TieuDe).Contains(Tools.RemoveDiacritics(item.Trim())) ).ToList();

                //        lstDaLoc.AddRange(lstTam);
                //    }
                //}
                ////gan lst da loc vao lst chinh
                //lstTinTuc = lstDaLoc.GroupBy(t => t.MaTinTuc).Select(g => g.First()).ToList();

                lstTinTuc = lstTinTuc.Where(n => n.TieuDe != null && Tools.RemoveDiacritics(n.TieuDe).Contains(Tools.RemoveDiacritics(TieuDe.Trim()))).ToList();
            }

            if (!string.IsNullOrEmpty(NoiDung))
            {
                //List<TinTuc> lstTam = new List<TinTuc>();  // lst tam dung de loc du lieu tu lst chinh
                //List<TinTuc> lstDaLoc = new List<TinTuc>(); // lst chua du lieu da loc ra

                //string[] sNoiDung = NoiDung.Split(' ');

                //foreach (var item in sNoiDung)
                //{
                //    if (!string.IsNullOrEmpty(item))
                //    {
                //        lstTam = lstTinTuc.Where(n => (n.NoiDung != null && Tools.RemoveDiacritics(n.NoiDung).Contains(Tools.RemoveDiacritics(item.Trim()))) || (n.NoiDungThuGon != null && Tools.RemoveDiacritics(n.NoiDungThuGon).Contains(Tools.RemoveDiacritics(item.Trim()) )) ).ToList();

                //        lstDaLoc.AddRange(lstTam);
                //    }
                //}
                ////gan lst da loc vao lst chinh
                //lstTinTuc = lstDaLoc.GroupBy(t => t.MaTinTuc).Select(g => g.First()).ToList();

                lstTinTuc = lstTinTuc.Where(n => (n.NoiDung != null && Tools.RemoveDiacritics(n.NoiDung).Contains(Tools.RemoveDiacritics(NoiDung.Trim()))) || (n.NoiDungThuGon != null && Tools.RemoveDiacritics(n.NoiDungThuGon).Contains(Tools.RemoveDiacritics(NoiDung.Trim())))).ToList();

            }

            if (!string.IsNullOrEmpty(NgayDang))
                lstTinTuc = lstTinTuc.Where(n => n.NgayDang != null && n.NgayDang.Value.ToString("dd/MM/yyyy").Contains(NgayDang.Trim()));

            if (!string.IsNullOrEmpty(MaDanhMuc))
            {
                int iMaDanhMuc = int.Parse(MaDanhMuc);
                lstTinTuc = lstTinTuc.Where(n => n.MaDanhMuc != null && n.MaDanhMuc.Value == iMaDanhMuc);
            }
            if (!string.IsNullOrEmpty(MaLoaiCT))
            {
                int iLoaiVanBan = int.Parse(MaLoaiCT);
                lstTinTuc = lstTinTuc.Where(n => (n.MaLoaiCT != null && n.MaLoaiCT.Value == iLoaiVanBan));
            }
            if (!string.IsNullOrEmpty(LoaiTinTuc))
            {
                int iLoaiTinTuc = int.Parse(LoaiTinTuc);
                lstTinTuc = lstTinTuc.Where(n => n.LoaiTinTuc != null && n.LoaiTinTuc.Value == iLoaiTinTuc);
            }

            return View(lstTinTuc.OrderByDescending(n=>n.NgayDang).ToList().ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult DanhSach(FormCollection c)
        {
            string sTieuDe = c["TieuDe"];
            string sNoiDung = c["NoiDung"];
            string sNgayDang = c["NgayDang"];
            string sMaDanhMuc = c["MaDanhMuc"];
            string sLoaiTinTuc = c["LoaiTinTuc"];
            string sLoaiCongTruong = c["LoaiCongTruong"];

            return RedirectToAction("DanhSach", "QL_TinTuc", new { @TieuDe = sTieuDe, @NoiDung = sNoiDung, @NgayDang = sNgayDang, @MaDanhMuc = sMaDanhMuc, @LoaiTinTuc = sLoaiTinTuc, @LoaiTCongTruong = sLoaiCongTruong });
        }

        [HttpGet]
        public ActionResult ThemMoi()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();

            ViewBag.LoaiTinTuc = new SelectList(tool.DMTinTuc(0), "Value", "Text");
            ViewBag.MaLoaiCT = new SelectList(db.LoaiCongTruongs, "MaLoaiCT", "TenLoaiCT");
            ViewBag.MaDanhMuc = new SelectList(db.DanhMucs.Where(n => n.DanhMucCha == Tools.MaDanhMucTin), "MaDanhMuc", "TenDanhMuc");
            Session["lbl"] = "Tạo mới";

            return View();
            //return RedirectToAction("ChinhSua", "QL_TinTuc", new {@id=themTin.MaTinTuc });
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ThemMoi(TinTuc tin,FormCollection c ,HttpPostedFileBase HinhDaiDiens)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();

            ViewBag.LoaiTinTuc = new SelectList(tool.DMTinTuc(0), "Value", "Text");
            ViewBag.MaLoaiCT = new SelectList(db.LoaiCongTruongs, "MaLoaiCT", "TenLoaiCT", tin.MaLoaiCT);
            ViewBag.MaDanhMuc = new SelectList(db.DanhMucs.Where(n => n.DanhMucCha == Tools.MaDanhMucTin), "MaDanhMuc", "TenDanhMuc");
            Session["lbl"] = "Tạo mới";

            if (ModelState.IsValid)
            {
                if (tin.ChoPhepBinhLuan == null)
                    tin.ChoPhepBinhLuan = false;
                if (tin.TinHot == null)
                    tin.TinHot = false;
                if (tin.TrangThai == null)
                    tin.TrangThai = false;
                if (tin.BaoMat == null)
                    tin.BaoMat = true;

                tin.LuotBinhLuan = 0;
                tin.LuotThich = 0;
                tin.LuotXem = 0;
                tin.NgayTao = DateTime.Now;

                //luu hinh dai dien
                if (HinhDaiDiens != null)
                {
                    string sFileName = Tools.md5(DateTime.Now.ToString());
                    var fileName = Path.GetFileName(HinhDaiDiens.FileName);
                    sFileName += fileName.Substring(fileName.LastIndexOf('.'));
                    var path = Path.Combine(Server.MapPath("~/Upload/"), sFileName);
                    HinhDaiDiens.SaveAs(path);

                    tin.HinhDaiDien = sFileName;
                }
                if (!string.IsNullOrEmpty(c["EndHotDates"]))
                    tin.EndHotDate = DateTime.ParseExact(c["EndHotDates"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(c["NgayDangs"]))
                    tin.NgayDang = DateTime.ParseExact(c["NgayDangs"].ToString(), "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                db.TinTucs.Add(tin);
                db.SaveChanges();
                
                TempData["thongbao"] = "<script>$('#div-pthongbao').text('Lưu thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return RedirectToAction("ChinhSua", "QL_TinTuc", new {@id= tin.MaTinTuc });
        }

        public ActionResult ChinhSua(int id, string e)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");
            if (!string.IsNullOrEmpty(e))
                Session["lbl"] = "Cập nhật";

            Tools tool = new Tools();
            TinTuc tin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == id);

            ViewBag.LoaiTinTuc = new SelectList(tool.DMTinTuc(0), "Value", "Text",tin.LoaiTinTuc);
            ViewBag.MaLoaiCT = new SelectList(db.LoaiCongTruongs, "MaLoaiCT", "TenLoaiCT", tin.MaLoaiCT);
            ViewBag.MaDanhMuc = new SelectList(db.DanhMucs.Where(n=>n.DanhMucCha==Tools.MaDanhMucTin), "MaDanhMuc", "TenDanhMuc", tin.MaDanhMuc);

            return View(tin);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ChinhSua(TinTuc tin,FormCollection c)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();
            ViewBag.LoaiTinTuc = new SelectList(tool.DMTinTuc(0), "Value", "Text", tin.LoaiTinTuc);
            ViewBag.MaLoaiCT = new SelectList(db.LoaiCongTruongs, "MaLoaiCT", "TenLoaiCT", tin.MaLoaiCT);
            ViewBag.MaDanhMuc = new SelectList(db.DanhMucs.Where(n => n.DanhMucCha == Tools.MaDanhMucTin), "MaDanhMuc", "TenDanhMuc", tin.MaDanhMuc);

            if (tin.ChoPhepBinhLuan==null)
                tin.ChoPhepBinhLuan = false;
            if (tin.TinHot == null)
                tin.TinHot = false;
            if (tin.TrangThai == null)
                tin.TrangThai = false;
            if (tin.BaoMat == null)
                tin.BaoMat = false;

            
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(c["EndHotDates"]))
                    tin.EndHotDate = DateTime.ParseExact(c["EndHotDates"].ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(c["NgayDangs"]))
                    tin.NgayDang = DateTime.ParseExact(c["NgayDangs"].ToString(), "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                db.Entry(tin).State = System.Data.Entity.EntityState.Modified;
            
                db.SaveChanges();
                TempData["thongbao"] = "<script> $('#div-pthongbao').text('Lưu thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            }
            return View(tin);
        }

        public ActionResult Xoa(int id)
        {
            TinTuc xoaTin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == id);
            db.TinTucs.Remove(xoaTin);
            db.SaveChanges();
            TempData["thongbao"] = "<script> $('#div-pthongbao').text('Xóa tin thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
            return RedirectToAction("DanhSach");
        }

        public ActionResult KichHoat(string id,bool id2)
        {
            NhanVien khNhanVien = db.NhanViens.SingleOrDefault(n => n.TenDangNhap == id);
            khNhanVien.KichHoat = id2;
            db.SaveChanges();
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public ActionResult saveimgTinTuc_ajax() // save hinh danh muc
        {
            string sFileName = Tools.md5(DateTime.Now.ToString());
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
                        fname = Path.Combine(Server.MapPath("~/Upload/"), sFileName);
                        file.SaveAs(fname);


                    }

                    // luu vao csdl
                    int iMaTinTuc = Convert.ToInt32(Request.Form["MaTinTuc"]);

                    TinTuc editTin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == iMaTinTuc);
                    editTin.HinhDaiDien = sFileName;
                    db.SaveChanges();

                    // Returns message that successfully uploaded  
                    return Json("<img src='/Upload/" + sFileName + "' width='100%' /> <input hidden name='HinhDaiDien' value='"+sFileName+"' />");
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