using System;
using System.Collections.Generic;

namespace BnanApi.Models
{
    public partial class CrCasPriceCarOption
    {
        public string CrCasPriceCarOptionsNo { get; set; } = null!;
        public string CrCasPriceCarOptionsCode { get; set; } = null!;
        public decimal? CrCasPriceCarOptionsValue { get; set; }

        public virtual CrMasSupContractOption CrCasPriceCarOptionsCodeNavigation { get; set; } = null!;
        public virtual CrCasPriceCarBasic CrCasPriceCarOptionsNoNavigation { get; set; } = null!;
    }
}
