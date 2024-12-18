using BnanApi.DTOS;
using BnanApi.DTOS.RenterInfoWithFiles;
using BnanApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BnanApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentersController : Controller
    {
        private readonly BnanSCContext _context;

        public RentersController(BnanSCContext context)
        {
            _context = context;
        }
        [HttpGet("RenterInfo")]
        public async Task<IActionResult> GetAllFilesForRenter(string RenterId)
        {
            try
            {
                // التحقق من أن RenterId ليس null أو فارغ
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

                // جلب معلومات المستأجر
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
                        RenterInfo = null,
                        Data = null,
                        Errors = null
                    });
                }
                var renterInfoDTO = new RenterInfoDTO
                {
                    ArName = renterInfo.CrMasRenterInformationArName,
                    EnName = renterInfo.CrMasRenterInformationEnName,
                };

                // جلب السجلات الخاصة بالمستأجر
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

                // التحقق من وجود أي سجلات
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

                // تقسيم السجلات حسب النوع باستخدام قيم مرجعية بدلاً من قيم النصوص
                var catchReceipts = receipts.Where(x => x.CrCasAccountReceiptType == "301").ToList();
                var receiptReceipts = receipts.Where(x => x.CrCasAccountReceiptType == "302").ToList();
                var proformaInvoices = invoices.Where(x => x.CrCasAccountInvoiceType == "308").ToList();
                var actualInvoices = invoices.Where(x => x.CrCasAccountInvoiceType == "309").ToList();

                // تنظيم البيانات في قوائم CategoriesFilesDTO
                var categoriesDTOs = new List<CategoriesFilesDTO>
        {
            CreateCategoryFilesDto("401", "عقود", "Contracts", contracts),
            CreateCategoryFilesDto("301", "سندات قبض", "Catch Receipt", catchReceipts),
            CreateCategoryFilesDto("302", "سندات صرف", "Payment Receipt", receiptReceipts),
            CreateCategoryFilesDto("308", "فواتير اولية", "Proforma Invoices", proformaInvoices),
            CreateCategoryFilesDto("309", "فواتير فعلية", "Actual Invoice", actualInvoices)
        };

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
                // معالجة الأخطاء غير المتوقعة مثل مشاكل الاتصال بقاعدة البيانات أو غيرها
                return StatusCode(500, new ApiRenterResponse<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "An unexpected error occurred while processing the request.",
                    RenterInfo = null,
                    Data = null,
                    Errors = new[] { ex.Message } // إرسال رسالة الخطأ للتصحيح
                });
            }
        }

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
