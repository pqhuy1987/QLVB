using System.Web.Mvc;
using System.Web.Routing;

namespace QLVB
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // TinTuc/XemTin
            routes.MapRoute(
                name: "XemTin",
                url: "xem-tin/{metatitle}-{id}",
                defaults: new { controller = "TinTuc", action = "XemTin", id = UrlParameter.Optional }
            );

            // TinTuc/ChiTietTin
            routes.MapRoute(
                name: "ChiTietTin",
                url: "tin-tuc/{metatitle}-{id}",
                defaults: new { controller = "TinTuc", action = "ChiTietTin", id = UrlParameter.Optional }
            );

            // TinTuc/dsTinTuc
            routes.MapRoute(
                name: "dsTin",
                url: "tin-tuc",
                defaults: new { controller = "TinTuc", action = "dsTinTuc", id = UrlParameter.Optional }
            );

            // TrangChinh/Index
            routes.MapRoute(
                name: "TrangChinh",
                url: "trang-chu",
                defaults: new { controller = "TrangChinh", action = "Index", id = UrlParameter.Optional }
            );

            // LichHop/DangKy
            routes.MapRoute(
                name: "LichHop_DangKy",
                url: "lich-hop",
                defaults: new { controller = "LichHop", action = "DangKy", id = UrlParameter.Optional }
            );

            // TrangChinh/dsFAQ
            routes.MapRoute(
                name: "TrangChinh_dsFAQ",
                url: "faq",
                defaults: new { controller = "TrangChinh", action = "dsFAQ", id = UrlParameter.Optional }
            );

            // TrangChinh/ChiTietFAQ
            routes.MapRoute(
                name: "TrangChinh_ChiTietFAQ",
                url: "faq/{metatitle}-{id}",
                defaults: new { controller = "TrangChinh", action = "ChiTietFAQ", id = UrlParameter.Optional }
            );

            // TrangChinh/IndexVanBan
            routes.MapRoute(
                name: "TrangChinh_IndexVanBan",
                url: "van-ban",
                defaults: new { controller = "TrangChinh", action = "IndexVanBan", id = UrlParameter.Optional }
            );

            // TrangChinh/XemVanBan
            routes.MapRoute(
                name: "TrangChinh_XemVanBan",
                url: "xem-van-ban/{metatitle}-{id}",
                defaults: new { controller = "TrangChinh", action = "XemVanBan", id = UrlParameter.Optional }
            );

            // TrangChinh/ChiTietVanBan
            routes.MapRoute(
                name: "TrangChinh_ChiTietVanBan",
                url: "van-ban/{metatitle}-{id}",
                defaults: new { controller = "TrangChinh", action = "ChiTietVanBan", id = UrlParameter.Optional }
            );

            // LichHop/LichHopTuan
            routes.MapRoute(
                name: "LichHop_LichHopTuan",
                url: "lich-hop-tuan",
                defaults: new { controller = "LichHop", action = "LichHopTuan", id = UrlParameter.Optional }
            );

            // QuanTri/DangNhap
            routes.MapRoute(
                name: "QuanTri_DangNhap",
                url: "dang-nhap",
                defaults: new { controller = "QuanTri", action = "DangNhap", id = UrlParameter.Optional }
            );

            // LichHop_BLD/DangKy
            routes.MapRoute(
                name: "LichHop_BLD_DangKy",
                url: "lich-hop-BLD",
                defaults: new { controller = "LichHop_BLD", action = "DangKy", id = UrlParameter.Optional }
            );

            // BEGIN
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}/{id2}/{id3}",
            //    defaults: new { controller = "ComPass", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional, id3 = UrlParameter.Optional }
            //);
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}/{id2}/{id3}",
                defaults: new { controller = "TrangChinh", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional, id3 = UrlParameter.Optional }
            );
            // END
        }
    }
}
