//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QLVB.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class VB_VBSD
    {
        public int ID { get; set; }
        public int MaVanBan { get; set; }
        public int MaVanBanSD { get; set; }
    
        public virtual TaiLieu TaiLieu { get; set; }
        public virtual TaiLieu TaiLieu1 { get; set; }
    }
}
