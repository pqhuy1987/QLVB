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
     
    public class QL_FAQController : Controller
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

        [Authorize(Roles = "QL-FAQ")]
        public ActionResult DanhSach()
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            //doi ten mac dinh cho tieu de trong
            IEnumerable<FAQ> edFAQ = db.FAQs.Where(n => n.CauHoi.Trim() == "");           
            foreach(var item in edFAQ)
            {
                item.CauHoi = "(Chưa có tiêu đề)";
            }
            db.SaveChanges();

            IEnumerable<FAQ> lstFAQ = db.FAQs.OrderBy(n=>n.STT);
            DuyetFAQ(0, lstFAQ); 

            return View();
        }

        string sHtml = "";
        public void DuyetFAQ(int iMaFAQ, IEnumerable<FAQ> lstFAQ)
        {
            if(iMaFAQ==0)
                sHtml += "<ol class='nested_with_switch vertical'>";
            else
                sHtml += "<ol class='mn-child'>";
          foreach(var item in lstFAQ.Where(n=>n.Parent_Id==iMaFAQ))
          {
              sHtml += "<li data-id='" + item.Id + "' >";
              sHtml += " <a class='mn-parent fa fa-unsorted btn btn-default btn-xs' ></a> ";
              sHtml += " <span class='span-tieude' data-toggle='tooltip' title='double click để sửa'>" + item.CauHoi + "</span> ";

              sHtml += " <span class='text-success span-soluong'></span> ";
              sHtml += " <span style='float:right'> ";
             // sHtml += " <a data-toggle='tooltip' title='Thêm nội dung' class='btn-noidung fa fa-edit btn btn-primary btn-xs'></a> ";
              sHtml += " <a data-toggle='tooltip' title='Thêm hình' class='btn-image fa fa-picture-o btn btn-primary btn-xs'></a> ";
              sHtml += " <a data-toggle='tooltip' title='Xóa' class='btn-xoa fa fa-times btn btn-danger btn-xs'></a> ";
              sHtml += " </span>";
             
              DuyetFAQ(item.Id,lstFAQ);
          }

            sHtml += "</ol>";
            ViewBag.DuyetFAQ = sHtml;
        }

        [HttpPost]
        public ActionResult insertNut_ajax()
        {
            //cap nhat so stt
            int iSTT = 2;
            IEnumerable<FAQ> lsttFAQ = db.FAQs.Where(n => n.Parent_Id == 0).OrderBy(n => n.STT);
            foreach(var item in lsttFAQ)
            {
                item.STT =iSTT;
                iSTT++;
            }
            db.SaveChanges();

            FAQ newFAQ = new FAQ();
            newFAQ.CauHoi = "";
            newFAQ.TraLoi = "";
            newFAQ.Parent_Id = 0;
            newFAQ.STT = 1;
            db.FAQs.Add(newFAQ);
            db.SaveChanges();
            
            string sContent = "";

            sContent += " <li data-id='" + newFAQ.Id + "'>";
            sContent += " <a class='mn-parent fa fa-unsorted btn btn-default btn-xs' style='display:none'></a> ";
            sContent += " <input class='txt-tieude' /> <a class='txt-check btn btn-success btn-xs fa fa-check' type='button' ></a>";
            sContent += " <span class='span-tieude' data-toggle='tooltip' title='double click để sửa' style='display:none'></span> ";
            sContent += " <span class='text-success span-soluong'></span> ";
            sContent += " <span style='float:right'> ";
          //  sContent += " <a data-toggle='tooltip' title='Thêm nội dung' class='btn-noidung fa fa-edit btn btn-primary btn-xs'></a> ";
            sContent += " <a data-toggle='tooltip' title='Thêm hình' class='btn-image fa fa-picture-o btn btn-primary btn-xs'></a> ";
            sContent += " <a data-toggle='tooltip' title='Xóa' class='btn-xoa fa fa-times btn btn-danger btn-xs'></a> ";
            sContent += " </span><script>$('[data-toggle=\"tooltip\"]').tooltip();</script> <ol class='mn-child'></ol>";

            return Content(sContent);
        }

        [HttpPost]
        public ActionResult editCha_ajax(int idcon, int idcha)
        {
            FAQ editFAQ = db.FAQs.SingleOrDefault(n => n.Id == idcon);
            editFAQ.Parent_Id = idcha;
            db.SaveChanges();

            return Content("");
        }

        [HttpPost]
        public ActionResult editTieuDe_ajax(int idcon, string noidung)
        {
            FAQ editFAQ = db.FAQs.SingleOrDefault(n => n.Id == idcon);
            editFAQ.CauHoi = noidung;
            db.SaveChanges();

            return Content("");
        }

        [HttpPost]
        public ActionResult deleteNut_ajax(int idcon)
        {
            IEnumerable<FAQ> lstFAQ = db.FAQs;
            deleteNut_con(idcon, lstFAQ);

            FAQ reFAQ = db.FAQs.SingleOrDefault(n => n.Id == idcon);
            db.FAQs.Remove(reFAQ);
            db.SaveChanges();

            return Content("");
        }
        public void deleteNut_con(int idcha, IEnumerable<FAQ> lstFAQ)
        {
            foreach (var item in lstFAQ.Where(n => n.Parent_Id == idcha))
            {
                db.FAQs.Remove(item);
                deleteNut_con(item.Id,lstFAQ);
            }
            db.SaveChanges();
        }


        [HttpPost]
        public ActionResult selectFAQ_ajax(int id)
        {
            FAQ itFAQ = db.FAQs.SingleOrDefault(n=>n.Id==id);

            return Content(itFAQ.TraLoi);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult saveNoiDungFAQ_ajax(int id , string cauhoi, string traloi)
        {
            try
            {
                FAQ itFAQ = db.FAQs.SingleOrDefault(n => n.Id == id);
                itFAQ.CauHoi = cauhoi;
                itFAQ.TraLoi = traloi;
                db.SaveChanges();

            }
            catch { return Content("0"); }

            return Content("1");
        }

        [HttpPost]
        public ActionResult saveSTT_ajax(int id, int stt)
        {
            FAQ itFAQ = db.FAQs.SingleOrDefault(n => n.Id == id);
            itFAQ.STT = stt;
            db.SaveChanges();

            return Content("");
        }

        [HttpPost]
        public ActionResult selectImageFAQ_ajax(int id)
        {
            FAQ itFAQ = db.FAQs.SingleOrDefault(n => n.Id == id);

            if(string.IsNullOrEmpty(itFAQ.HinhAnh))
                return Content("");
            return Content("<img src='/Upload/HinhDMFAQ/" + itFAQ.HinhAnh + "' style='max-width:200px' />");
        }

        [HttpPost]
        public ActionResult saveimgFAQ_ajax() // save hinh danh muc
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
                        fname = Path.Combine(Server.MapPath("~/Upload/HinhDMFAQ/"), fname);
                        file.SaveAs(fname);


                    }

                    // luu vao csdl
                    int idFAQ = Convert.ToInt32(Request.Form["idFAQ"]);
                    string sNamef = files[0].FileName;

                    FAQ editFAQ = db.FAQs.SingleOrDefault(n => n.Id == idFAQ);
                    editFAQ.HinhAnh = sNamef;
                    db.SaveChanges();

                    // Returns message that successfully uploaded  
                    return Json("<img src='/Upload/HinhDMFAQ/" + sNamef + "' style='max-width:200px' />");
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
       

         [Authorize(Roles = "XEM-Y-KIEN")]
        public ActionResult dsYKien(int? page)
        {
            if (KiemTraSession() == true)
                return RedirectToAction("DangNhap", "QuanTri");

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(db.FeedBacks.OrderByDescending(n=>n.NgayGui).ToList().ToPagedList(pageNumber, pageSize));
        }

         [Authorize(Roles = "QL-FAQ")]
         public ActionResult dsFAQ(int? page)
         {
             if (KiemTraSession() == true)
                 return RedirectToAction("DangNhap", "QuanTri");

             ViewBag.MaDanhMuc = new SelectList( db.FAQs.OrderBy(n => n.STT), "Id", "CauHoi");

             int pageSize = 20;
             int pageNumber = (page ?? 1);
             return View(db.FAQ_NoiDung.ToList().ToPagedList(pageNumber, pageSize));
         }

         public ActionResult Xoa(int id)
         {
             FAQ_NoiDung xoaFAQ = db.FAQ_NoiDung.SingleOrDefault(n => n.Id == id);
             db.FAQ_NoiDung.Remove(xoaFAQ);
             db.SaveChanges();
             TempData["thongbao"] = "<script> $('#div-pthongbao').text('Xóa thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
             return RedirectToAction("dsFAQ");
         }

         [HttpPost]
        [ValidateInput(false)]
         public ActionResult ThemMoiFAQ(FAQ_NoiDung faq)
         {
             if (KiemTraSession() == true)
                 return RedirectToAction("DangNhap", "QuanTri");

             if (ModelState.IsValid)
             {
                 db.FAQ_NoiDung.Add(faq);
                 db.SaveChanges();
                 TempData["thongbao"] = "<script>$('#div-pthongbao').text('Tạo mới thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
             }
             return RedirectToAction("dsFAQ");
         }

         [HttpPost]
         [ValidateInput(false)]
         public ActionResult ChinhSuaFAQ(FAQ_NoiDung faq, FormCollection f)
         {
             if (KiemTraSession() == true)
                 return RedirectToAction("DangNhap", "QuanTri");


             if (ModelState.IsValid)
             {
                 faq.TraLoi = f["edTraLoi"];
                 db.Entry(faq).State = System.Data.Entity.EntityState.Modified;
                 db.SaveChanges();
                 TempData["thongbao"] = "<script>$('#div-pthongbao').text('Cập nhật thành công !'); $('#div-thongbao').show(); $('#div-thongbao').fadeOut(5000);</script>";
             }
             return RedirectToAction("dsFAQ");
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