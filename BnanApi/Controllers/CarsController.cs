using BnanApi.DTOS;
using BnanApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BnanApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : Controller
    {
        private readonly BnanSCContext _context;

        public CarsController(BnanSCContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCar()
        {
            List<CarInfomationDTO> carInfomationDTOList = new List<CarInfomationDTO>();
            var cars = await _context.CrCasCarInformations.Where(x => x.CrCasCarInformationForSaleStatus != "A").Include(x => x.CrCasCarInformationDistributionNavigation).ToListAsync();
            foreach (var car in cars)
            {
                var lessorInfo = await _context.CrMasLessorInformations.FirstOrDefaultAsync(x => x.CrMasLessorInformationCode == car.CrCasCarInformationLessor);

                CarInfomationDTO carInfomationDTO = new CarInfomationDTO();
                carInfomationDTO.CarArName = car.CrCasCarInformationConcatenateArName;
                carInfomationDTO.CarEnName = car.CrCasCarInformationConcatenateEnName;
                carInfomationDTO.CompanyArName = lessorInfo.CrMasLessorInformationArLongName;
                carInfomationDTO.CompanyEnName = lessorInfo.CrMasLessorInformationEnLongName;
                carInfomationDTO.CurrentMeter = car.CrCasCarInformationCurrentMeter?.ToString("N0", CultureInfo.InvariantCulture);
                carInfomationDTO.TelephoneContact = lessorInfo.CrMasLessorInformationCommunicationMobileKey + lessorInfo.CrMasLessorInformationCallFree;
                carInfomationDTO.Email = lessorInfo.CrMasLessorInformationEmail;
                carInfomationDTO.ImagePath = car.CrCasCarInformationDistributionNavigation.CrMasSupCarDistributionImage.Replace("~", "");
                carInfomationDTO.Price = car.CrCasCarInformationOfferValueSale?.ToString("N2", CultureInfo.InvariantCulture);
                carInfomationDTOList.Add(carInfomationDTO);
            }
            return Ok(carInfomationDTOList);
        }
        [HttpGet("Categories")]
        public async Task<IActionResult> GetCategories()
        {

            List<CategoriesDTO> CategoriesDTOList = new List<CategoriesDTO>();
            var cars = await _context.CrCasCarInformations.Where(x => x.CrCasCarInformationForSaleStatus != "A").Include(x => x.CrCasCarInformationDistributionNavigation).Include(x => x.CrCasCarInformationCategoryNavigation).ToListAsync();
            var categoryCars = cars.Select(item => item.CrCasCarInformationCategoryNavigation).Distinct().OrderBy(item => item.CrMasSupCarCategoryCode).ToList();
            foreach (var categoryCar in categoryCars)
            {
                CategoriesDTO CategoriesDTO = new CategoriesDTO();
                CategoriesDTO.Code = categoryCar.CrMasSupCarCategoryCode;
                CategoriesDTO.ArName = categoryCar.CrMasSupCarCategoryArName;
                CategoriesDTO.EnName = categoryCar.CrMasSupCarCategoryEnName;
                CategoriesDTOList.Add(CategoriesDTO);

            }

            return Ok(CategoriesDTOList);
        }
        [HttpGet("GetCarsByCategory")]
        public async Task<IActionResult> GetCarBysCategory(string Code)
        {

            List<CarInfomationDTO> carInfomationDTOList = new List<CarInfomationDTO>();
            var cars = new List<CrCasCarInformation>();
            if (Code == "3400000000") cars = await _context.CrCasCarInformations.Where(x => x.CrCasCarInformationForSaleStatus != "A").Include(x => x.CrCasCarInformationDistributionNavigation).ToListAsync();
            else cars = await _context.CrCasCarInformations.Where(x => x.CrCasCarInformationForSaleStatus != "A" && x.CrCasCarInformationCategory == Code).Include(x => x.CrCasCarInformationDistributionNavigation).ToListAsync();
            foreach (var car in cars)
            {
                var lessorInfo = await _context.CrMasLessorInformations.FirstOrDefaultAsync(x => x.CrMasLessorInformationCode == car.CrCasCarInformationLessor);

                CarInfomationDTO carInfomationDTO = new CarInfomationDTO();
                carInfomationDTO.CarArName = car.CrCasCarInformationConcatenateArName;
                carInfomationDTO.CarEnName = car.CrCasCarInformationConcatenateEnName;
                carInfomationDTO.CompanyArName = lessorInfo.CrMasLessorInformationArLongName;
                carInfomationDTO.CompanyEnName = lessorInfo.CrMasLessorInformationEnLongName;
                carInfomationDTO.CurrentMeter = car.CrCasCarInformationCurrentMeter?.ToString("N0", CultureInfo.InvariantCulture);
                carInfomationDTO.TelephoneContact = lessorInfo.CrMasLessorInformationCommunicationMobileKey + lessorInfo.CrMasLessorInformationCallFree;
                carInfomationDTO.Email = lessorInfo.CrMasLessorInformationEmail;
                carInfomationDTO.ImagePath = car.CrCasCarInformationDistributionNavigation.CrMasSupCarDistributionImage;
                carInfomationDTO.Price = car.CrCasCarInformationOfferValueSale?.ToString("N2", CultureInfo.InvariantCulture);
                carInfomationDTOList.Add(carInfomationDTO);
            }
            return Ok(carInfomationDTOList);
        }
    }
}
