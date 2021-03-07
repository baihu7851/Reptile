using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reptile
{
    public class GovProcurement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Key { get; set; }
        public string AgencNname { get; set; }
        public string CaseName { get; set; }
        public int Number { get; set; }
        public string TenderMethod { get; set; }
        public string purchasingProperty { get; set; }
        public DateTime AnnouncementDate { get; set; }
        public DateTime Deadine { get; set; }
        public int Budget { get; set; }
    }
}