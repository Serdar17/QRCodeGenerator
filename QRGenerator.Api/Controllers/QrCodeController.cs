using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using IronBarCode;
using QRGenerator.Api.Dtos;

namespace QRGenerator.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class QrCodeController : ControllerBase
{
    public QrCodeController(IConfiguration configuration)
    {
        // TODO: Удалить из конфига
        License.LicenseKey = configuration.GetValue<string>("IronBarCodeKey");
    }

    [HttpGet("qr-generator")]
    [ProducesResponseType(typeof(File), 200)]
    public async Task<IActionResult> GenerateQrcodeAsync([FromQuery] string data, [FromQuery] int size = 500)
    {
        var fileName = $"images/{Path.GetRandomFileName()}.png";
        QRCodeWriter.CreateQrCode(data, size).SaveAsPng(fileName);
        var bytes = await System.IO.File.ReadAllBytesAsync(fileName); 
        return File(bytes, "image/png");
    }

    [HttpPost("qr-decoder")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    public async Task<IActionResult> DecodeQrcodeAsync([FromForm] DecodeFileRequest request)
    {
        var filter = new SharpenFilter();
        var images = new ImageFilterCollection { filter };
        var options = new BarcodeReaderOptions
        {
            Speed = ReadingSpeed.ExtremeDetail,
            ExpectMultipleBarcodes = false,
            ExpectBarcodeTypes = BarcodeEncoding.All,
            Multithreaded = false,
            MaxParallelThreads = 1,
            CropArea = null,
            UseCode39ExtendedMode = false,
            RemoveFalsePositive = false,
            ImageFilters = images
        };
        
        var result = await BarcodeReader.ReadAsync(
            request.File.OpenReadStream(), options);
        
        if (result is not null && result.Values().Length > 0)
        {
            return Ok(result.First().Value);
        }

        return BadRequest();
    }

    [HttpGet("barcode-generator")]
    [ProducesResponseType(typeof(File), 200)]
    public async Task<IActionResult> GenerateBarcodeAsync([FromQuery] string data, BarcodeWriterEncoding encoding = BarcodeWriterEncoding.Code128)
    {
        var fileName = $"images/{Path.GetRandomFileName()}.png";
        var myBarcode = BarcodeWriter.CreateBarcode(
            data,
            encoding);
        myBarcode.SaveAsPng(fileName);
        var bytes = await System.IO.File.ReadAllBytesAsync(fileName);         
        return File(bytes, "image/png");
    }
    
    [HttpPost("barcode-decoder")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> DecodeBarcodeAsync([FromForm] DecodeFileRequest request)
    {
        var options = new BarcodeReaderOptions
        {
            Speed = ReadingSpeed.ExtremeDetail,
            ExpectMultipleBarcodes = false,
            ExpectBarcodeTypes = BarcodeEncoding.All,
            Multithreaded = false,
            MaxParallelThreads = 1,
            CropArea = null,
            UseCode39ExtendedMode = false,
            RemoveFalsePositive = false,
            ImageFilters = null
        };
        
        var result = await BarcodeReader.ReadAsync(
            request.File.OpenReadStream(), options);
        
        if (result is not null && result.Values().Length > 0)
        {
            return Ok(result.First().Value);
        }
    
        return BadRequest();
    }
}