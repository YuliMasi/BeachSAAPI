using BeachSAAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BeachSAAPI.Services
{
    public class InvoicePdfService : IInvoicePdfService
    {
        private const string PrimaryColor = "#0B2A72";
        private const string SecondaryColor = "#FBAF17";
        private const string TextColor = "#3D4F73";

        public byte[] Generate(Invoice invoice, Reservation reservation, User user, Package package)
        {
            double downPayment = invoice.totalUsd * package.PercentageTotalAmount / 100;
            double monthlyPayment = package.RemainingMonthlyInstallments > 0
                ? (invoice.totalUsd - downPayment) / package.RemainingMonthlyInstallments
                : 0;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(TextColor));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Beach.SA").FontSize(22).Bold().FontColor(PrimaryColor);
                            col.Item().Text("Hoteles & Resort");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().AlignRight().Text($"Factura N° {invoice.InvoiceNumber:D6}").FontSize(16).Bold().FontColor(PrimaryColor);
                            col.Item().AlignRight().Text($"Fecha: {invoice.InvoiceDate:dd/MM/yyyy}");
                        });
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Text("Cliente").Bold().FontColor(PrimaryColor);
                        col.Item().Column(c =>
                        {
                            c.Item().Text(user.FullName);
                            c.Item().Text($"{user.IdType} · {user.Email}");
                            if (user.PhoneNumber > 0)
                            {
                                c.Item().Text($"Tel. {user.PhoneNumber}");
                            }
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text("Reservación").Bold().FontColor(PrimaryColor);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            AddRow(table, "Paquete", package.PackageType);
                            AddRow(table, "Entrada", reservation.CheckInDate.ToString("dd/MM/yyyy"));
                            AddRow(table, "Salida", reservation.CheckOutDate.ToString("dd/MM/yyyy"));
                            AddRow(table, "Noches", reservation.AccommodationDays.ToString());
                            AddRow(table, "Huéspedes", reservation.GuestQuantity.ToString());
                            AddRow(table, "Forma de pago", reservation.PaymentMethod);

                            if (!string.IsNullOrEmpty(reservation.CheckNumber))
                            {
                                AddRow(table, "Cheque", $"N° {reservation.CheckNumber} · {reservation.Bank}");
                            }
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text("Detalle del pago").Bold().FontColor(PrimaryColor);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            AddRow(table, "Subtotal", $"${invoice.Subtotal:N2}");

                            if (invoice.DiscountAmount > 0)
                            {
                                AddRow(table, $"Descuento ({invoice.Discount * 100:N0} %)", $"-${invoice.DiscountAmount:N2}");
                            }

                            AddRow(table, $"IVA ({invoice.IVA * 100:N0} %)", $"${invoice.IvaAmount:N2}");
                            AddRow(table, "Tipo de cambio", $"₡{invoice.ExchangeRate:N2}");
                            AddRow(table, $"Prima ({package.PercentageTotalAmount:N0} %)", $"${downPayment:N2}");
                            AddRow(table, $"{package.RemainingMonthlyInstallments} mensualidades de", $"${monthlyPayment:N2}");
                        });

                        col.Item().Background(PrimaryColor).Padding(12).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Total USD").FontColor(Colors.White);
                                c.Item().Text($"${invoice.totalUsd:N2}").FontSize(16).Bold().FontColor(SecondaryColor);
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text("Total CRC").FontColor(Colors.White);
                                c.Item().AlignRight().Text($"₡{invoice.totalColones:N2}").FontSize(16).Bold().FontColor(SecondaryColor);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text("Gracias por elegir Beach.SA · info@beachsa.com").FontSize(9);
                });
            });

            return document.GeneratePdf();
        }

        private static void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell().PaddingVertical(3).Text(label);
            table.Cell().PaddingVertical(3).AlignRight().Text(value ?? "");
        }
    }
}
