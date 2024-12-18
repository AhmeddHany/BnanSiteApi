using System;
using System.Collections.Generic;

namespace BnanApi.Models
{
    public partial class CrCasRenterContractChoice
    {
        public string CrCasRenterContractChoiceNo { get; set; } = null!;
        public string CrCasRenterContractChoiceCode { get; set; } = null!;
        public decimal? CrCasContractChoiceValue { get; set; }

        public virtual CrMasSupContractOption CrCasRenterContractChoiceCodeNavigation { get; set; } = null!;
    }
}
