//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using Online_Shopping_System.Services;
//using System;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace Online_Shopping_System.Tests
//{
//    // Assumes ReportService has been refactored behind an IReportService
//    // interface (see TESTING_GUIDE.md, section 2), and ReportsController's
//    // constructor takes IReportService instead of the concrete ReportService.
//    public class ReportsControllerTests
//    {
//        private readonly Mock<ReportService> _reportServiceMock;
//        private readonly ReportsController _controller;

//        public ReportsControllerTests()
//        {
//            _reportServiceMock = new Mock<ReportService>();
//            _controller = new ReportsController(_reportServiceMock.Object);
//        }

//        [Fact]
//        public async Task OrdersExcel_ReturnsFile_WithExcelContentType()
//        {
//            var fakeBytes = Encoding.UTF8.GetBytes("fake excel content");
//            _reportServiceMock
//                .Setup(s => s.GenerateOrdersExcelAsync())
//                .ReturnsAsync(fakeBytes);

//            var result = await _controller.OrdersExcel();

//            var fileResult = Assert.IsType<FileContentResult>(result);
//            Assert.Equal(
//                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                fileResult.ContentType);
//            Assert.Equal(fakeBytes, fileResult.FileContents);
//            Assert.StartsWith("Orders-", fileResult.FileDownloadName);
//        }

//        [Fact]
//        public async Task OrdersExcel_Returns500_WhenServiceThrows()
//        {
//            _reportServiceMock
//                .Setup(s => s.GenerateOrdersExcelAsync())
//                .ThrowsAsync(new InvalidOperationException("db unreachable"));

//            var result = await _controller.OrdersExcel();

//            var statusResult = Assert.IsType<ObjectResult>(result);
//            Assert.Equal(500, statusResult.StatusCode);
//            Assert.Contains("db unreachable", statusResult.Value!.ToString());
//        }

//        [Fact]
//        public async Task OrdersPdf_ReturnsFile_WithPdfContentType()
//        {
//            var fakeBytes = Encoding.UTF8.GetBytes("%PDF-fake");
//            _reportServiceMock
//                .Setup(s => s.GenerateOrdersPdfAsync())
//                .ReturnsAsync(fakeBytes);

//            var result = await _controller.OrdersPdf();

//            var fileResult = Assert.IsType<FileContentResult>(result);
//            Assert.Equal("application/pdf", fileResult.ContentType);
//            Assert.Equal(fakeBytes, fileResult.FileContents);
//        }

//        [Fact]
//        public async Task OrdersPdf_Returns500_WhenServiceThrows()
//        {
//            _reportServiceMock
//                .Setup(s => s.GenerateOrdersPdfAsync())
//                .ThrowsAsync(new Exception("render failure"));

//            var result = await _controller.OrdersPdf();

//            var statusResult = Assert.IsType<ObjectResult>(result);
//            Assert.Equal(500, statusResult.StatusCode);
//        }
//    }
//}
