using BnanApi.DTOS;
using BnanApi.DTOS.RenterInfoWithFiles;
using BnanApi.Helper;
using BnanApi.Models;
using BnanApi.Services.Whatsup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BnanApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentersController : Controller
    {
        private readonly BnanSCContext _context;
        private readonly IWhatsupService _whatsupService;
        private readonly IMemoryCache _cache;

        public RentersController(BnanSCContext context, IWhatsupService whatsupService, IMemoryCache cache)
        {
            _context = context;
            _whatsupService = whatsupService;
            _cache = cache;
        }


        // إرسال كود التحقق
        [HttpGet("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode(string RenterId)
        {
            try
            {
                if (string.IsNullOrEmpty(RenterId))
                {
                    return BadRequest(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "The 'RenterId' parameter is required and cannot be null or empty.",
                        Data = null,
                        Errors = null
                    });
                }

                var renterInfo = await _context.CrMasRenterInformations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CrMasRenterInformationId == RenterId);

                if (renterInfo == null)
                {
                    return NotFound(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = $"No renter found with ID '{RenterId}'.",
                        Data = null,
                        Errors = null
                    });
                }
                if (string.IsNullOrEmpty(renterInfo.CrMasRenterInformationCountreyKey) || string.IsNullOrEmpty(renterInfo.CrMasRenterInformationMobile))
                {
                    return BadRequest(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "The key or mobile is not Exist.",
                        Data = null,
                        Errors = null
                    });
                }

                // التحقق من أن رقم الهاتف لا يبدأ بـ "0" ويتكون من 10 أرقام فقط
                if (renterInfo.CrMasRenterInformationMobile.StartsWith("0"))
                {
                    return BadRequest(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "The mobile number should not start with '0'.",
                        Data = null,
                        Errors = null
                    });
                }

                if (renterInfo.CrMasRenterInformationMobile.Length != 9 && renterInfo.CrMasRenterInformationMobile.Length != 10 || !renterInfo.CrMasRenterInformationMobile.All(char.IsDigit))
                {
                    return BadRequest(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "The mobile number should be 10 or 9 digits long and contain only numbers.",
                        Data = null,
                        Errors = null
                    });
                }

                var mobile = renterInfo.CrMasRenterInformationCountreyKey + renterInfo.CrMasRenterInformationMobile; // رقم الهاتف
                var otp = new Random().Next(100000, 999999).ToString(); // كود مكون من 6 أرقام
                var message = $"كود تأكيد هوية المستخدم من نظام بنان : {otp}";

                var result = await _whatsupService.SendMessageAsync(new WhatsupDTO
                {
                    Phone = mobile,
                    Message = message
                });

                if (result != ApiResponseStatus.Success)
                {
                    return StatusCode(500, new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 500,
                        Message = "Failed to send the verification code. Please try again later.",
                        Data = null,
                        Errors = null
                    });
                }

                // إزالة الكاش القديم إذا كان موجودًا
                _cache.Remove($"OTP_{RenterId}");

                // تخزين الكود في الكاش ككائن يحتوي على OTP و RenterId
                _cache.Set($"OTP_{RenterId}", new { OTP = otp, RenterId }, TimeSpan.FromMinutes(5));

                return Ok(new ApiRenterResponse<string>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "Verification code sent successfully.",
                    Data = null,
                    Errors = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRenterResponse<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "An unexpected error occurred while processing the request.",
                    Data = null,
                    Errors = new[] { ex.Message }
                });
            }
        }

        //// التحقق من كود التحقق
        //[HttpPost("VerifyOtp")]
        //public IActionResult VerifyOtp(string RenterId, string OTP)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(RenterId) || string.IsNullOrEmpty(OTP))
        //        {
        //            return BadRequest(new ApiRenterResponse<string>
        //            {
        //                Status = "Error",
        //                Code = 400,
        //                Message = "RenterId and OTP are required.",
        //                Data = null,
        //                Errors = null
        //            });
        //        }

        //        // استرجاع الكود وRenterId من الكاش
        //        if (!_cache.TryGetValue($"OTP_{RenterId}", out var cachedData))
        //        {
        //            return Unauthorized(new ApiRenterResponse<string>
        //            {
        //                Status = "Error",
        //                Code = 401,
        //                Message = "Invalid or expired verification code.",
        //                Data = null,
        //                Errors = null
        //            });
        //        }

        //        // تحويل البيانات المسترجعة إلى كائن
        //        var cacheEntry = cachedData as dynamic;
        //        string correctOtp = cacheEntry.OTP;
        //        string correctRenterId = cacheEntry.RenterId;

        //        // التحقق من الكود ورقم المستأجر
        //        if (correctOtp != OTP || correctRenterId != RenterId)
        //        {
        //            return Unauthorized(new ApiRenterResponse<string>
        //            {
        //                Status = "Error",
        //                Code = 401,
        //                Message = "Invalid or expired verification code or mismatched renter ID.",
        //                Data = null,
        //                Errors = null
        //            });
        //        }

        //        // إزالة الكود بعد التحقق الناجح فقط
        //        _cache.Remove($"OTP_{RenterId}");

        //        // تخزين حالة التحقق في الكاش
        //        _cache.Set($"Verified_{RenterId}", true, TimeSpan.FromMinutes(30));  // يمكنك تحديد مدة صلاحية مختلفة

        //        return Ok(new ApiRenterResponse<string>
        //        {
        //            Status = "Success",
        //            Code = 200,
        //            Message = "Verification successful.",
        //            Data = null,
        //            Errors = null
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ApiRenterResponse<string>
        //        {
        //            Status = "Error",
        //            Code = 500,
        //            Message = "An unexpected error occurred while processing the request.",
        //            Data = null,
        //            Errors = new[] { ex.Message }
        //        });
        //    }
        //}

        // استرداد بيانات المستأجر

        // دالة للتحقق من OTP
        private bool VerifyOtpAndRenterId(string RenterId, string OTP)
        {
            // التحقق من صحة RenterId و OTP
            if (string.IsNullOrEmpty(RenterId) || string.IsNullOrEmpty(OTP))
            {
                return false;
            }

            // التحقق من حالة التحقق في الكاش (هل تم التحقق من OTP)
            if (!_cache.TryGetValue($"Verified_{RenterId}", out bool isVerified) || !isVerified)
            {
                return false;
            }

            // استرجاع OTP و RenterId من الكاش للتحقق
            if (!_cache.TryGetValue($"OTP_{RenterId}", out var cachedData))
            {
                return false;
            }

            // تحويل البيانات المسترجعة إلى كائن
            var cacheEntry = cachedData as dynamic;
            string correctOtp = cacheEntry.OTP;
            string correctRenterId = cacheEntry.RenterId;

            // التحقق من أن OTP و RenterId متطابقين
            return correctOtp == OTP && correctRenterId == RenterId;
        }
        // التحقق من كود التحقق
        [HttpPost("VerifyOtp")]
        public IActionResult VerifyOtp(string RenterId, string OTP)
        {
            try
            {
                if (string.IsNullOrEmpty(RenterId) || string.IsNullOrEmpty(OTP))
                {
                    return BadRequest(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "RenterId and OTP are required.",
                        Data = null,
                        Errors = null
                    });
                }

                // استرجاع الكود وRenterId من الكاش
                if (!_cache.TryGetValue($"OTP_{RenterId}", out var cachedData))
                {
                    return Unauthorized(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 401,
                        Message = "Invalid or expired verification code.",
                        Data = null,
                        Errors = null
                    });
                }

                // تحويل البيانات المسترجعة إلى كائن
                var cacheEntry = cachedData as dynamic;
                string correctOtp = cacheEntry.OTP;
                string correctRenterId = cacheEntry.RenterId;

                // التحقق من الكود ورقم المستأجر
                if (correctOtp != OTP || correctRenterId != RenterId)
                {
                    return Unauthorized(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 401,
                        Message = "Invalid or expired verification code or mismatched renter ID.",
                        Data = null,
                        Errors = null
                    });
                }

                // إزالة الكود بعد التحقق الناجح فقط
                _cache.Set($"OTP_{RenterId}", new { OTP = OTP, RenterId }, TimeSpan.FromMinutes(10));

                // تخزين حالة التحقق في الكاش
                _cache.Set($"Verified_{RenterId}", true, TimeSpan.FromMinutes(10));  // يمكنك تحديد مدة صلاحية مختلفة

                return Ok(new ApiRenterResponse<string>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "Verification successful.",
                    Data = null,
                    Errors = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRenterResponse<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "An unexpected error occurred while processing the request.",
                    Data = null,
                    Errors = new[] { ex.Message }
                });
            }
        }

        // استرداد بيانات المستأجر
        [HttpGet("GetFiles")]
        public async Task<IActionResult> GetAllFilesForRenter(string RenterId, string OTP)
        {
            try
            {
                // التحقق من صحة RenterId و OTP
                if (!VerifyOtpAndRenterId(RenterId, OTP))
                {
                    return Unauthorized(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 401,
                        Message = "Invalid or expired verification code or mismatched renter ID.",
                        Data = null,
                        Errors = null
                    });
                }

                // باقي الكود الخاص بجلب البيانات كما هو في الكود السابق...
                var renterInfo = await _context.CrMasRenterInformations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CrMasRenterInformationId == RenterId);

                if (renterInfo == null)
                {
                    return NotFound(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = $"No renter found with ID '{RenterId}'. Please check the ID and try again.",
                        Data = null,
                        Errors = null
                    });
                }

                var renterInfoDTO = new RenterInfoDTO
                {
                    ArName = renterInfo.CrMasRenterInformationArName,
                    EnName = renterInfo.CrMasRenterInformationEnName,
                };

                var receipts = await _context.CrCasAccountReceipts
                    .AsNoTracking()
                    .Include(x => x.CrCasAccountReceiptNavigation.CrCasBranchInformationLessorNavigation)
                    .Include(x => x.CrCasAccountReceiptReferenceTypeNavigation)
                    .Where(x => x.CrCasAccountReceiptRenterId == renterInfo.CrMasRenterInformationId)
                    .ToListAsync();

                var contracts = await _context.CrCasRenterContractBasics
                    .AsNoTracking()
                    .Include(x => x.CrCasRenterContractBasic1.CrCasBranchInformationLessorNavigation)
                    .Where(x => x.CrCasRenterContractBasicRenterId == renterInfo.CrMasRenterInformationId)
                    .ToListAsync();

                var invoices = await (from invoice in _context.CrCasAccountInvoices
                                      join contract in _context.CrCasRenterContractBasics
                                      on invoice.CrCasAccountInvoiceReferenceContract equals contract.CrCasRenterContractBasicNo
                                      where contract.CrCasRenterContractBasicRenterId == renterInfo.CrMasRenterInformationId
                                      select invoice)
                      .AsNoTracking()
                      .Include(x => x.CrCasAccountInvoiceNavigation.CrCasBranchInformationLessorNavigation)
                      .ToListAsync();

                if (!receipts.Any() && !contracts.Any() && !invoices.Any())
                {
                    return NotFound(new ApiRenterResponse<string>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = $"No records found for renter ID '{RenterId}'.",
                        RenterInfo = renterInfoDTO,
                        Data = null,
                        Errors = null
                    });
                }

                var catchReceipts = receipts.Where(x => x.CrCasAccountReceiptType == "301").ToList();
                var receiptReceipts = receipts.Where(x => x.CrCasAccountReceiptType == "302").ToList();
                var proformaInvoices = invoices.Where(x => x.CrCasAccountInvoiceType == "308").ToList();
                var actualInvoices = invoices.Where(x => x.CrCasAccountInvoiceType == "309").ToList();

                var categoriesDTOs = new List<CategoriesFilesDTO>
        {
            CreateCategoryFilesDto("401", "عقود", "Contracts", contracts),
            CreateCategoryFilesDto("301", "سندات قبض", "Catch Receipt", catchReceipts),
            CreateCategoryFilesDto("302", "سندات صرف", "Payment Receipt", receiptReceipts),
            CreateCategoryFilesDto("308", "فواتير اولية", "Proforma Invoices", proformaInvoices),
            CreateCategoryFilesDto("309", "فواتير فعلية", "Actual Invoice", actualInvoices)
        };
                _cache.Remove($"OTP_{RenterId}");
                _cache.Remove($"Verified_{RenterId}");

                return Ok(new ApiRenterResponse<List<CategoriesFilesDTO>>
                {
                    Status = "Success",
                    Code = 200,
                    Message = $"Files retrieved successfully. Total categories: {categoriesDTOs.Count}.",
                    RenterInfo = renterInfoDTO,
                    Data = categoriesDTOs,
                    Errors = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiRenterResponse<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "An unexpected error occurred while processing the request.",
                    Data = null,
                    Errors = new[] { ex.Message }
                });
            }
        }



        //[HttpGet("RenterInfo")]
        //public async Task<IActionResult> GetAllFilesForRenter(string RenterId)
        //{
        //    try
        //    {
        //        // التحقق من أن RenterId ليس null أو فارغ
        //        if (string.IsNullOrEmpty(RenterId))
        //        {
        //            return BadRequest(new ApiRenterResponse<string>
        //            {
        //                Status = "Error",
        //                Code = 400,
        //                Message = "The 'RenterId' parameter is required and cannot be null or empty.",
        //                Data = null,
        //                Errors = null
        //            });
        //        }

        //        // جلب معلومات المستأجر
        //        var renterInfo = await _context.CrMasRenterInformations
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(x => x.CrMasRenterInformationId == RenterId);

        //        if (renterInfo == null)
        //        {
        //            return NotFound(new ApiRenterResponse<string>
        //            {
        //                Status = "Error",
        //                Code = 404,
        //                Message = $"No renter found with ID '{RenterId}'. Please check the ID and try again.",
        //                RenterInfo = null,
        //                Data = null,
        //                Errors = null
        //            });
        //        }
        //        var renterInfoDTO = new RenterInfoDTO
        //        {
        //            ArName = renterInfo.CrMasRenterInformationArName,
        //            EnName = renterInfo.CrMasRenterInformationEnName,
        //        };

        //        // جلب السجلات الخاصة بالمستأجر
        //        var receipts = await _context.CrCasAccountReceipts
        //            .AsNoTracking()
        //            .Include(x => x.CrCasAccountReceiptNavigation.CrCasBranchInformationLessorNavigation)
        //            .Include(x => x.CrCasAccountReceiptReferenceTypeNavigation)
        //            .Where(x => x.CrCasAccountReceiptRenterId == renterInfo.CrMasRenterInformationId)
        //            .ToListAsync();

        //        var contracts = await _context.CrCasRenterContractBasics
        //            .AsNoTracking()
        //            .Include(x => x.CrCasRenterContractBasic1.CrCasBranchInformationLessorNavigation)
        //            .Where(x => x.CrCasRenterContractBasicRenterId == renterInfo.CrMasRenterInformationId)
        //            .ToListAsync();

        //        var invoices = await (from invoice in _context.CrCasAccountInvoices
        //                              join contract in _context.CrCasRenterContractBasics
        //                              on invoice.CrCasAccountInvoiceReferenceContract equals contract.CrCasRenterContractBasicNo
        //                              where contract.CrCasRenterContractBasicRenterId == renterInfo.CrMasRenterInformationId
        //                              select invoice)
        //              .AsNoTracking()
        //              .Include(x => x.CrCasAccountInvoiceNavigation.CrCasBranchInformationLessorNavigation)
        //              .ToListAsync();

        //        // التحقق من وجود أي سجلات
        //        if (!receipts.Any() && !contracts.Any() && !invoices.Any())
        //        {
        //            return NotFound(new ApiRenterResponse<string>
        //            {
        //                Status = "Error",
        //                Code = 404,
        //                Message = $"No records found for renter ID '{RenterId}'.",
        //                RenterInfo = renterInfoDTO,
        //                Data = null,
        //                Errors = null
        //            });
        //        }

        //        // تقسيم السجلات حسب النوع باستخدام قيم مرجعية بدلاً من قيم النصوص
        //        var catchReceipts = receipts.Where(x => x.CrCasAccountReceiptType == "301").ToList();
        //        var receiptReceipts = receipts.Where(x => x.CrCasAccountReceiptType == "302").ToList();
        //        var proformaInvoices = invoices.Where(x => x.CrCasAccountInvoiceType == "308").ToList();
        //        var actualInvoices = invoices.Where(x => x.CrCasAccountInvoiceType == "309").ToList();

        //        // تنظيم البيانات في قوائم CategoriesFilesDTO
        //        var categoriesDTOs = new List<CategoriesFilesDTO>
        //{
        //    CreateCategoryFilesDto("401", "عقود", "Contracts", contracts),
        //    CreateCategoryFilesDto("301", "سندات قبض", "Catch Receipt", catchReceipts),
        //    CreateCategoryFilesDto("302", "سندات صرف", "Payment Receipt", receiptReceipts),
        //    CreateCategoryFilesDto("308", "فواتير اولية", "Proforma Invoices", proformaInvoices),
        //    CreateCategoryFilesDto("309", "فواتير فعلية", "Actual Invoice", actualInvoices)
        //};

        //        return Ok(new ApiRenterResponse<List<CategoriesFilesDTO>>
        //        {
        //            Status = "Success",
        //            Code = 200,
        //            Message = $"Files retrieved successfully. Total categories: {categoriesDTOs.Count}.",
        //            RenterInfo = renterInfoDTO,
        //            Data = categoriesDTOs,
        //            Errors = null
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // معالجة الأخطاء غير المتوقعة مثل مشاكل الاتصال بقاعدة البيانات أو غيرها
        //        return StatusCode(500, new ApiRenterResponse<string>
        //        {
        //            Status = "Error",
        //            Code = 500,
        //            Message = "An unexpected error occurred while processing the request.",
        //            RenterInfo = null,
        //            Data = null,
        //            Errors = new[] { ex.Message } // إرسال رسالة الخطأ للتصحيح
        //        });
        //    }
        //}

        // دالة مساعدة لإنشاء CategoriesFilesDTO من العقود
        private CategoriesFilesDTO CreateCategoryFilesDto(string id, string arName, string enName, List<CrCasRenterContractBasic> contracts)
        {
            return new CategoriesFilesDTO
            {
                Id = id,
                ArName = arName,
                EnName = enName,
                Files = contracts.Select(r => new FilesDTO
                {
                    FileId = r.CrCasRenterContractBasicNo,
                    ArLessorName = r.CrCasRenterContractBasic1?.CrCasBranchInformationLessorNavigation?.CrMasLessorInformationArLongName,
                    EnLessorName = r.CrCasRenterContractBasic1?.CrCasBranchInformationLessorNavigation?.CrMasLessorInformationEnLongName,
                    ArBranchName = r.CrCasRenterContractBasic1?.CrCasBranchInformationArName,
                    EnBranchName = r.CrCasRenterContractBasic1?.CrCasBranchInformationEnName,
                    Date = r.CrCasRenterContractBasicIssuedDate,
                    ArPdfPath = r.CrCasRenterContractBasicArPdfFile?.Replace("~", ""),
                    EnPdfPath = r.CrCasRenterContractBasicEnPdfFile?.Replace("~", "")
                }).ToList()
            };
        }

        // دالة مساعدة لإنشاء CategoriesFilesDTO من سندات القبض
        private CategoriesFilesDTO CreateCategoryFilesDto(string id, string arName, string enName, List<CrCasAccountReceipt> receipts)
        {
            return new CategoriesFilesDTO
            {
                Id = id,
                ArName = arName,
                EnName = enName,
                Files = receipts.Select(r => new FilesDTO
                {
                    FileId = r.CrCasAccountReceiptNo,
                    ArLessorName = r.CrCasAccountReceiptNavigation?.CrCasBranchInformationLessorNavigation?.CrMasLessorInformationArLongName,
                    EnLessorName = r.CrCasAccountReceiptNavigation?.CrCasBranchInformationLessorNavigation?.CrMasLessorInformationEnLongName,
                    ArBranchName = r.CrCasAccountReceiptNavigation?.CrCasBranchInformationArName,
                    EnBranchName = r.CrCasAccountReceiptNavigation?.CrCasBranchInformationEnName,
                    ArReferenceType = r.CrCasAccountReceiptReferenceTypeNavigation?.CrMasSupAccountReceiptReferenceArName,
                    EnReferenceType = r.CrCasAccountReceiptReferenceTypeNavigation?.CrMasSupAccountReceiptReferenceEnName,
                    Date = r.CrCasAccountReceiptDate,
                    ArPdfPath = r.CrCasAccountReceiptArPdfFile?.Replace("~", ""),
                    EnPdfPath = r.CrCasAccountReceiptEnPdfFile?.Replace("~", "")
                }).ToList()
            };
        }

        // دالة مساعدة لإنشاء CategoriesFilesDTO من الفواتير
        private CategoriesFilesDTO CreateCategoryFilesDto(string id, string arName, string enName, List<CrCasAccountInvoice> invoices)
        {
            return new CategoriesFilesDTO
            {
                Id = id,
                ArName = arName,
                EnName = enName,
                Files = invoices.Select(r => new FilesDTO
                {
                    FileId = r.CrCasAccountInvoiceNo,
                    ArLessorName = r.CrCasAccountInvoiceNavigation?.CrCasBranchInformationLessorNavigation?.CrMasLessorInformationArLongName,
                    EnLessorName = r.CrCasAccountInvoiceNavigation?.CrCasBranchInformationLessorNavigation?.CrMasLessorInformationEnLongName,
                    ArBranchName = r.CrCasAccountInvoiceNavigation?.CrCasBranchInformationArName,
                    EnBranchName = r.CrCasAccountInvoiceNavigation?.CrCasBranchInformationEnName,
                    Date = r.CrCasAccountInvoiceDate,
                    ArPdfPath = r.CrCasAccountInvoiceArPdfFile?.Replace("~", ""),
                    EnPdfPath = r.CrCasAccountInvoiceEnPdfFile?.Replace("~", "")
                }).ToList()
            };
        }

    }
}
