using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Online_Shopping_System.Data;
using Online_Shopping_System.Models.Shipping;


namespace Online_Shopping_System.Services
{
    public class ReportService
    {
        private readonly OnlineShoppingContext _context;

        public ReportService(OnlineShoppingContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateOrdersExcelAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Cart)
                .Include(o => o.ShippingRecords)
                .Include(o => o.ShippingRecords.ShippingType)
                .Include(o => o.User)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Orders");

            // Header
            worksheet.Cell(1, 1).Value = "Order ID";
            worksheet.Cell(1, 2).Value = "Cart ID";
            worksheet.Cell(1, 3).Value = "User Name";
            worksheet.Cell(1, 4).Value = "Total Price";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Order Date";
            worksheet.Cell(1, 7).Value = "Payment Type";
            worksheet.Cell(1, 8).Value = "Cart Status";
            worksheet.Cell(1, 9).Value = "Cart Date";
            worksheet.Cell(1, 10).Value = "Shipping Type";

            int row = 2;

            foreach (var order in orders)
            {
                worksheet.Cell(row, 1).Value = order.OrderId;
                worksheet.Cell(row, 2).Value = order.CartId;
                worksheet.Cell(row, 3).Value = order.User.UserName;
                worksheet.Cell(row, 4).Value = order.TotalCost;
                worksheet.Cell(row, 5).Value = order.OrderStatus;
                worksheet.Cell(row, 6).Value = order.CreatedAt.DateTime;
                worksheet.Cell(row, 7).Value = order.PaymentTypeName;
                worksheet.Cell(row, 8).Value = order.Cart.CartStatus;
                worksheet.Cell(row, 9).Value = order.Cart.CreatedAt.DateTime;
                worksheet.Cell(row, 10).Value = order.ShippingRecords.ShippingType.ShippingName;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public async Task<byte[]> GenerateOrdersPdfAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Cart)
                .Include(o => o.ShippingRecords)
                .Include(o => o.ShippingRecords.ShippingType)
                .Include(o => o.User)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Landscape is better for 10 columns
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(20);

                    // Header
                    page.Header()
                        .AlignCenter()
                        .Text("Orders Report")
                        .FontSize(18)
                        .Bold();

                    page.Content()
                        .PaddingTop(15)
                        .Table(table =>
                        {
                            // 10 columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(45);  // Order ID
                                columns.ConstantColumn(45);  // Cart ID
                                columns.RelativeColumn(1.5f); // User Name
                                columns.ConstantColumn(60);  // Total Price
                                columns.RelativeColumn(1.2f); // Status
                                columns.ConstantColumn(75);  // Order Date
                                columns.RelativeColumn(1.2f); // Payment Type
                                columns.RelativeColumn(1.2f); // Cart Status
                                columns.ConstantColumn(75);  // Cart Date
                                columns.RelativeColumn(1.3f); // Shipping Type
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Order ID");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Cart ID");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("User Name");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Total Price");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Status");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Order Date");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Payment Type");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Cart Status");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Cart Date");

                                header.Cell()
                                    .Element(HeaderStyle)
                                    .Text("Shipping Type");
                            });

                            // Data rows
                            foreach (var order in orders)
                            {
                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.OrderId.ToString());

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.CartId.ToString());

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.User?.UserName ?? "N/A");

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.TotalCost.ToString("0.00"));

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.OrderStatus?.ToString() ?? "N/A");

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.CreatedAt.DateTime.ToString("yyyy-MM-dd HH:mm"));

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.PaymentTypeName ?? "N/A");

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(order.Cart?.CartStatus?.ToString() ?? "N/A");

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(
                                        order.Cart == null
                                            ? "N/A"
                                            : order.Cart.CreatedAt.DateTime.ToString("yyyy-MM-dd HH:mm")
                                    );

                                table.Cell()
                                    .Element(CellStyle)
                                    .Text(
                                        order.ShippingRecords?.ShippingType?.ShippingName ?? "N/A"
                                    );
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();


            // Header cell style
            static IContainer HeaderStyle(IContainer container)
            {
                return container
                    .Background("#D9EAF7")
                    .Border(1)
                    .BorderColor("#000000")
                    .Padding(4)
                    .AlignCenter()
                    .AlignMiddle();
            }

            // Normal cell style
            static IContainer CellStyle(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor("#000000")
                    .Padding(4)
                    .AlignCenter()
                    .AlignMiddle();
            }
        }


    }
}
