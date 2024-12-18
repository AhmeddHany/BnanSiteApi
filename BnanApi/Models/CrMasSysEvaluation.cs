﻿using System;
using System.Collections.Generic;

namespace BnanApi.Models
{
    public partial class CrMasSysEvaluation
    {
        public CrMasSysEvaluation()
        {
            CrCasRenterContractEvaluations = new HashSet<CrCasRenterContractEvaluation>();
        }

        public string CrMasSysEvaluationsCode { get; set; } = null!;
        public string? CrMasSysEvaluationsClassification { get; set; }
        public string? CrMasSysEvaluationsArDescription { get; set; }
        public string? CrMasSysEvaluationsEnDescription { get; set; }
        public int? CrMasSysServiceEvaluationsValue { get; set; }
        public string? CrMasSysEvaluationsImage { get; set; }
        public string? CrMasSysEvaluationsStatus { get; set; }
        public string? CrMasSysEvaluationsReasons { get; set; }

        public virtual ICollection<CrCasRenterContractEvaluation> CrCasRenterContractEvaluations { get; set; }
    }
}
