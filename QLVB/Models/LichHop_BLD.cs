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
    
    public partial class LichHop_BLD
    {
        public int ID { get; set; }
        public string Subject { get; set; }
        public Nullable<int> OwnerId { get; set; }
        public Nullable<System.DateTime> Start { get; set; }
        public Nullable<System.DateTime> End { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedUserId { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<int> UpdatedUserId { get; set; }
        public string Description { get; set; }
        public string RecurrenceRule { get; set; }
        public string RecurrenceParentId { get; set; }
    }
}
