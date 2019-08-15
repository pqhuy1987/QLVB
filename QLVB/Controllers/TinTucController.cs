using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLVB.Models;
using PagedList;
using PagedList.Mvc;
using System.Net;
using System.Net.Mail;

namespace QLVB.Controllers
{
     [Authorize]
    public class TinTucController : Controller
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
        
        public ActionResult XemTin(int id)
        {
            TinTuc layTin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == id);
            layTin.LuotXem++;
            db.SaveChanges();

            string sUrl = "/tin-tuc/" + Tools.RemoveDiacritics(layTin.TieuDe) + "-" + id;

            return Redirect(sUrl);
        }

        public ActionResult ChiTietTin(int id)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();
            DateTime dtNow = DateTime.Now;
            TinTuc layTin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == id);
         
            ViewBag.LoaiVanBans = db.LoaiTaiLieux.OrderBy(n => n.ThuTu);
            ViewBag.VanBans = db.TaiLieux;
            ViewBag.DMTinTuc = db.DanhMucs.Where(n => n.DanhMucCha == Tools.MaDanhMucTin).OrderBy(n => n.ThuTu);
            ViewBag.dsTinTuc = db.TinTucs.Where(n => n.TrangThai == true && n.NgayDang <= dtNow).OrderByDescending(n => n.NgayDang);

            ViewBag.lstBanner = db.Banners.OrderBy(n => n.Order);
            ViewBag.lstBinhLuan = db.BinhLuans.Where(n => n.MaTin == id);
            ViewBag.lstDanhGia = db.DanhGias;

            if (layTin.TrangThai == true)
                return View(layTin);
            else
                return RedirectToAction("Index", "TrangChinh");
        }

        public ActionResult dsTinTuc(int? page, string t,string m)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            Tools tool = new Tools();
            DateTime dtNow = DateTime.Now;
            
            ViewBag.LoaiVanBans = db.LoaiTaiLieux.OrderBy(n => n.CapTaiLieu);
            ViewBag.VanBans = db.TaiLieux;
            ViewBag.DMTinTuc = db.DanhMucs.Where(n => n.DanhMucCha == Tools.MaDanhMucTin).OrderBy(n => n.ThuTu);
            ViewBag.dsTinTuc = db.TinTucs.Where(n => n.TrangThai == true && n.NgayDang <= dtNow).OrderByDescending(n => n.NgayDang);

           

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.txtTimKiem = t;
            ViewBag.muctin = m;

            IEnumerable<TinTuc> lstTinTuc= db.TinTucs.Where(n => n.TrangThai == true && n.NgayDang <= dtNow);
            if(!string.IsNullOrEmpty(t))
            {
                t = t.ToLower().Trim();
                lstTinTuc = lstTinTuc.Where(n => (n.NoiDung!=null && n.NoiDung.ToLower().Contains(t)) || (n.NoiDungThuGon!=null && n.NoiDungThuGon.ToLower().Trim().Contains(t)) || (n.TieuDe!=null && n.TieuDe.ToLower().Trim().Contains(t)) );
            }
            if (!string.IsNullOrEmpty(m))
            {
                int imuctin = int.Parse(m);
                lstTinTuc = lstTinTuc.Where(n=>n.MaDanhMuc== imuctin);
            }
            return View(lstTinTuc.OrderByDescending(n => n.NgayDang).ToList().ToPagedList(pageNumber, pageSize));
        }

         [HttpPost]
        public ActionResult dsTinTuc(string txtTimKiem,string muctin)
        {
            return RedirectToAction("dsTinTuc", "TinTuc", new { @t= txtTimKiem, m = muctin });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult luuBinhLuan_ajax(string matin, string noidung)
        {
            if (KiemTraSession() != true)
            { 
                int iMaTin= int.Parse(matin);
                NhanVien nv = (NhanVien)Session["DangNhap"];

                BinhLuan themBL = new BinhLuan();
                themBL.IdNhanVien = nv.Id;
                themBL.MaTin = iMaTin;
                themBL.NoiDung = noidung;
                themBL.NgayBinhLuan = DateTime.Now;
                themBL.MaBinhLuanCha = 0;

                db.BinhLuans.Add(themBL);

                TinTuc tin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == iMaTin);
                tin.LuotBinhLuan += 1;

                db.SaveChanges();

                string sNoiDung = "<div class='hienthi-binhluan-item'>";
                sNoiDung += "<textarea hidden id='txt-noidung'>" + noidung + "'</textarea>";
                sNoiDung += "<div class='hienthi-binhluan-img'>";
                sNoiDung += "<img width = '40' src='/Upload/Avatar/images.png' /></div>";
                sNoiDung += "<div class='hienthi-binhluan-noidung'><b>" + nv.TenDangNhap + " </b> <span class='noidung-chinhsua' data-MaCha='" + themBL.Id + "'>" + noidung + "</span>";
                sNoiDung += "<p style='font-size:12px'><a class='txt-traloi'>Trả lời</a> - <span style='color: silver'> Vừa xong </span></p> </div> ";
                sNoiDung += "<i class='fa fa-pencil btn-suaBL'  data-MaCha='"+themBL.Id+ "'></i> <i class='fa fa-times btn-xoaBL'  data-MaCha='" + themBL.Id + "'></i>";
                sNoiDung += "<div class='clearfix' style='margin-bottom:10px'></div>";
                sNoiDung += " <div class='hienthi-binhluan-con'> <div class='div-traloi-binhluan-con'></div>";
                sNoiDung += "<div class='div-traloi-binhluan' style='margin-bottom:10px;display:none;'>";
                sNoiDung += "<img width='40' src='/Upload/Avatar/images.png' /> ";
                sNoiDung += "<textarea type='text' data-MaCha='" + themBL.Id+ "' class='txt-traloi-binhluan' placeholder='Viết bình luận của bạn...' ></textarea>";
                //sNoiDung += "<p style='font-size:12px; margin-left: 45px; margin-top: 10px; '> <a class='txt-luu-traloi'>Gửi Bình Luận</a></p>";
                sNoiDung += " </div></div></div>";


                return Content(sNoiDung);
            }
            return Content("");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult TraLoiBinhLuan_ajax(string matin, string noidung,string macha)
        {
            if (KiemTraSession() != true)
            {
                int iMaTin = int.Parse(matin);
                int iMaCha = int.Parse(macha);
                NhanVien nv = (NhanVien)Session["DangNhap"];

                BinhLuan themBL = new BinhLuan();
                themBL.IdNhanVien = nv.Id;
                themBL.MaTin = iMaTin;
                themBL.NoiDung = noidung;
                themBL.NgayBinhLuan = DateTime.Now;
                themBL.MaBinhLuanCha = iMaCha;

                db.BinhLuans.Add(themBL);

                TinTuc tin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == iMaTin);
                tin.LuotBinhLuan += 1;

                db.SaveChanges();

                string sNoiDung = "<div class='hienthi-binhluan-item'>";
                sNoiDung += "<textarea hidden id='txt-noidung'>" + noidung + "'</textarea>";
                sNoiDung += "<div class='hienthi-binhluan-img'>";
                sNoiDung += "<img width = '40' src='/Upload/Avatar/images.png' /></div>";
                sNoiDung += "<div class='hienthi-binhluan-noidung'><b>" + nv.TenDangNhap + " </b> <span class='noidung-chinhsua' data-MaCha='" + themBL.Id + "'>" + noidung + "</span>";
                sNoiDung += "<p style='font-size:12px'><a class='txt-traloi'>Trả lời</a> - <span style='color: silver'> Vừa xong </span></p> </div> ";
                sNoiDung += "<i class='fa fa-pencil btn-suaBL' id="+ themBL.Id +"  data-MaCha='" + themBL.Id + "'></i> <i class='fa fa-times btn-xoaBL'  data-MaCha='" + themBL.Id + "'></i>";
                sNoiDung += "<div class='clearfix' style='margin-bottom:10px'></div> </div>";
                sNoiDung += "<p style='font-size:12px; margin-left: 45px; margin-top: 10px; '> <a id="+ themBL.Id +"class='txt-luu-traloi'> Gửi Bình Luận </a> </p>";

                return Content(sNoiDung);
            }
            return Content("");
        }

        [HttpPost]
        public ActionResult XoaBinhLuan_ajax(string matin, string macha)
        {
           try
            {
                int iLuotComment = 1;
                int iMaTin = int.Parse(matin);
                int iMaCha = int.Parse(macha);
                BinhLuan xoaBL = db.BinhLuans.SingleOrDefault(n => n.Id == iMaCha);

                IEnumerable<BinhLuan> lstxoaBL = db.BinhLuans.Where(n => n.MaBinhLuanCha == iMaCha);

                if(lstxoaBL.Count()>0)
                {
                    iLuotComment += lstxoaBL.Count();
                    db.BinhLuans.RemoveRange(lstxoaBL);
                }

                TinTuc tin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == iMaTin);
                tin.LuotBinhLuan -= iLuotComment;

                db.BinhLuans.Remove(xoaBL);
                db.SaveChanges();
            }
            catch
            {
                return Content("0");
            }
            return Content("1");
        }

        [HttpPost]
        public ActionResult SuaBinhLuan_ajax( string macha, string noidung)
        {
            try
            {
                int iMaCha = int.Parse(macha);
                BinhLuan suaBL = db.BinhLuans.SingleOrDefault(n => n.Id == iMaCha);
                suaBL.NoiDung = noidung;
                
                db.SaveChanges();
            }
            catch
            {
                return Content("0");
            }
            return Content(noidung);
        }

        [HttpPost]
        public ActionResult DanhGia_ajax(string matin)
        {
            string sResult = "0";
            if (KiemTraSession() != true)
            {
                int iMaTin = int.Parse(matin);
                NhanVien nv = (NhanVien)Session["DangNhap"];

                DanhGia layDG = db.DanhGias.SingleOrDefault(n => n.IdNhanVien == nv.Id && n.MaTin == iMaTin);
                TinTuc tin = db.TinTucs.SingleOrDefault(n => n.MaTinTuc == iMaTin);
              

                if (layDG == null)
                {
                    DanhGia themDG = new DanhGia();
                    themDG.IdNhanVien = nv.Id;
                    themDG.MaTin = iMaTin;
                    db.DanhGias.Add(themDG);
                    sResult = "1";

                    tin.LuotThich += 1;
                }
                else
                {
                    db.DanhGias.Remove(layDG);

                    tin.LuotThich -= 1;
                }

              

                db.SaveChanges();
            }
            return Content(sResult);
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