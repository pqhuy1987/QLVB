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
    
    public partial class Quyen
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Quyen()
        {
            this.Nhom_Quyen = new HashSet<Nhom_Quyen>();
        }
    
        public string MaQuyen { get; set; }
        public string TenQuyen { get; set; }
        public string MoTa { get; set; }
        public Nullable<int> ChucNang { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Nhom_Quyen> Nhom_Quyen { get; set; }
    }
}
