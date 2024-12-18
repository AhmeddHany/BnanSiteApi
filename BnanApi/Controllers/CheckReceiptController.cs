using BnanApi.DTOS;
using BnanApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BnanApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckReceiptController : Controller
    {
        private readonly BnanSCContext _context;

        public CheckReceiptController(BnanSCContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetReceipt(string Code)
        {
            var pdfModel = new ReceiptsVM();
            if (Code.Contains("-1401-"))
            {
                // Contract
                var contract = await _context.CrCasRenterContractBasics
                    .Where(x => x.CrCasRenterContractBasicNo == Code)
                    .OrderByDescending(x => x.CrCasRenterContractBasicCopy)
                    .FirstOrDefaultAsync();

                if (contract != null)
                {
                    pdfModel.Type = "Contract";
                    pdfModel.ArPdf = contract.CrCasRenterContractBasicArPdfFile?.Replace("~", "");
                    pdfModel.EnPdf = contract.CrCasRenterContractBasicEnPdfFile?.Replace("~", "");
                    return Ok(pdfModel);
                }
                else
                {
                    return NotFound("The contract number is incorrect.");
                }
            }
            else if (Code.Contains("-1301-"))
            {
                // Receipt
                var receipt = await _context.CrCasAccountReceipts.FirstOrDefaultAsync(x => x.CrCasAccountReceiptNo == Code);
                if (receipt != null)
                {
                    pdfModel.Type = "Receipt";
                    pdfModel.ArPdf = receipt.CrCasAccountReceiptArPdfFile?.Replace("~", "");
                    pdfModel.EnPdf = receipt.CrCasAccountReceiptEnPdfFile?.Replace("~", "");
                    return Ok(pdfModel);
                }
                else
                {
                    return NotFound("The receipt number is incorrect.");
                }
            }
            else if (Code.Contains("-1308-"))
            {
                // Invoice
                var invoice = await _context.CrCasAccountInvoices.FirstOrDefaultAsync(x => x.CrCasAccountInvoiceNo == Code);
                if (invoice != null)
                {
                    pdfModel.Type = "Invoice";
                    pdfModel.ArPdf = invoice.CrCasAccountInvoiceArPdfFile?.Replace("~", "");
                    pdfModel.EnPdf = invoice.CrCasAccountInvoiceEnPdfFile?.Replace("~", "");
                    return Ok(pdfModel);
                }
                else
                {
                    return NotFound("The invoice number is incorrect.");
                }
            }

            // If the provided code doesn't match any of the predefined patterns
            return NotFound("The code provided is not recognized.");
        }
    }
}
