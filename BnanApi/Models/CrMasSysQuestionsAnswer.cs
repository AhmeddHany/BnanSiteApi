﻿using System;
using System.Collections.Generic;

namespace BnanApi.Models
{
    public partial class CrMasSysQuestionsAnswer
    {
        public string CrMasSysQuestionsAnswerNo { get; set; } = null!;
        public string? CrMasSysQuestionsAnswerSystem { get; set; }
        public string? CrMasSysQuestionsAnswerMainTask { get; set; }
        public string? CrMasSysQuestionsAnswerArQuestions { get; set; }
        public string? CrMasSysQuestionsAnswerArAnswer { get; set; }
        public string? CrMasSysQuestionsAnswerEnQuestions { get; set; }
        public string? CrMasSysQuestionsAnswerEnAnswer { get; set; }
        public string? CrMasSysQuestionsAnswerArVideo { get; set; }
        public string? CrMasSysQuestionsAnswerEnVideo { get; set; }
        public string? CrMasSysQuestionsAnswerStatus { get; set; }
        public string? CrMasSysQuestionsAnswerReasons { get; set; }
    }
}
