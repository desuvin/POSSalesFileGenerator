using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShopifySharp;
using ShopifySharp.Filters;

namespace POSSalesFileGenerator
{
    class Program
    {
        private static DateTime customdate = new DateTime(2024, 02, 01);
        private static DateTime customdate2 = customdate.AddHours(23).AddMinutes(59).AddSeconds(59);
        private static DateTime currentDateTime = DateTime.Now.Date;
        private static DateTime previousDateTime2 = DateTime.Now.Date.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
        private static DateTime previousDateTime = DateTime.Now.Date.AddDays(-1);
        private static string machineId = "17000013";
        private static string batchId;

        static async Task Main(string[] args)
        {
            batchId = await GetAndUpdateBatchId();
            
            Console.WriteLine("Loading Settings ...");
            Console.WriteLine("Current Date : " + currentDateTime.ToString("yyyy-MM-dd")); 
            Console.WriteLine("Previous Date : " + previousDateTime.ToString("yyyy-MM-dd"));
            string formattedDate = previousDateTime.ToString("ddMMMyyyy").ToUpper();
            Console.WriteLine(formattedDate);

            Console.WriteLine("Connecting to Shopify Store");
            var securityKey = Environment.GetEnvironmentVariable("SECURITY_KEY_POS");
            Console.WriteLine("This is the security key :  " + securityKey);

            var shopService = new ShopService("clearlab-pos.myshopify.com", securityKey);
            Console.WriteLine("Succesfully Connected to : " + shopService);

            Console.WriteLine("Getting Orders from the store ...");
            var orderService = new OrderService("clearlab-pos.myshopify.com", securityKey);

            // Get refunds for the specified date range
            var orderRefunds = await GetRefunds(orderService);

            var filter = new OrderListFilter
            {
                Limit = 250,
                Status = "any",
                CreatedAtMin = previousDateTime,
                CreatedAtMax = previousDateTime2
            };


            int LineNumber = 1000;
            var receiptCount = 0;

            string[] hours = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11",
                               "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23"};

            var GSTRegistered = "Y";
            
            foreach (var hour in hours)
            {
                decimal GTO = 0;
                decimal GST = 0;
                decimal Discount = 0;
                decimal ServiceCharge = 0;
                int NoOfPax = 0;
                decimal Cash = 0;
                decimal NETS = 0;
                decimal Visa = 0;
                decimal MasterCard = 0;
                decimal Amex = 0;
                decimal Voucher = 0;
                decimal Others = 0;
                decimal Refunded = 0;

                var orders = await orderService.ListAsync(filter);

                var ordercountpercall = 0;

                //remove double refund
                var refundDeducted = orderRefunds
                       .Where(r => !orders.Items.Any(o => o.OrderNumber == r.OrderNumber))
                       .ToList();
                int RefundedCount = orderRefunds.Where(r => r.UpdatedAt.Value.ToString("HH") == hour).Count();
<<<<<<< HEAD
                Refunded += orderRefunds.Where(r => r.UpdatedAt.Value.ToString("HH") == hour).Sum(r => r.Refunds.Sum(refund => refund.Transactions.Sum(transaction => transaction.Amount.Value)));
=======
                Refunded += refundDeducted.Where(r => r.UpdatedAt.Value.ToString("HH") == hour).Sum(r => r.Refunds.Sum(refund => refund.Transactions.Sum(transaction => transaction.Amount.Value)));
>>>>>>> 9baca645f2e570c94121197ad9e3b8e1aebcbbef
                //Console.WriteLine("Refunded Amount : ", Refunded);
               
                foreach (var orderitem in orders.Items)
                {
                    int count_eyeexam = 0;
                    //Console.WriteLine("EYE EXAM START : " + count_eyeexam);
                    foreach (var item in orderitem.LineItems)
                    {
                        if (item.SKU == "8122")
                        {
                            count_eyeexam++;
                        }
                    }
                    if (hour == orderitem.CreatedAt.Value.ToString("HH"))
                    {
                        receiptCount++;
                        decimal CurrentSubTotalPrice = orderitem.CurrentSubtotalPrice.Value;
                        decimal CurrentTotalTax = orderitem.CurrentTotalTax.Value;
<<<<<<< HEAD
                        double eyeexam_cost = 27.52;
                        double eyeexam_tax = 2.48;
                        if (count_eyeexam > 0)
                        {
                            eyeexam_cost = eyeexam_cost * count_eyeexam;
                            eyeexam_tax = eyeexam_tax * count_eyeexam;

                            CurrentSubTotalPrice -= (decimal)eyeexam_cost;
                            CurrentTotalTax -= (decimal)eyeexam_tax;
=======
                        double eyeexam_cost = 30.00;
                        double eyeexam_tax = 2.48;
                        if (count_eyeexam > 0)
                        {
                            Console.WriteLine("CurrentSubTotalPrice" + CurrentSubTotalPrice);
                            double c_eyeexam_cost = eyeexam_cost * count_eyeexam;
                            double c_eyeexam_tax = eyeexam_tax * count_eyeexam;

                            CurrentSubTotalPrice -= (decimal)c_eyeexam_cost;
                            CurrentTotalTax -= (decimal)c_eyeexam_tax;

                            Console.WriteLine("After Deduction CurrentSubTotalPrice" + CurrentSubTotalPrice);
>>>>>>> 9baca645f2e570c94121197ad9e3b8e1aebcbbef
                        }
                        if (orderitem.TotalShippingPriceSet.ShopMoney.Amount > 0)
                        {
                            CurrentSubTotalPrice -= (decimal)5;
                        }
                        decimal subGTO = CurrentSubTotalPrice - CurrentTotalTax;
                        Console.WriteLine("Order NUmber : " + orderitem.Name + " Order GTO : " + subGTO);
                        //decimal subGTO = orderitem.CurrentSubtotalPrice.Value - orderitem.CurrentTotalTax.Value;
                        GTO += subGTO;
                        GST += orderitem.CurrentTotalTax.Value;
                        Discount += orderitem.TotalDiscounts.Value;

                        // Determine the payment gateway used for the order
                        foreach (var pgateway in orderitem.PaymentGatewayNames)
                        {
                            if (pgateway == "Visa/MasterCard")
                            {
                                Visa += subGTO;
                            }
                            else if (pgateway == "Cash")
                            {
                                Cash += subGTO;
                            }
                            else
                            {
                                Others += subGTO;
                            }
                        }

                        
                    }

                    ordercountpercall++;
                    LineNumber += 1000;
                    //Console.WriteLine("EYE EXAM END : " + count_eyeexam);
                }

                if (Refunded > 0)
                {
                    Console.WriteLine(Refunded);
                    GTO -= Refunded;
                }

                if (RefundedCount > 0)
                {
                    Console.WriteLine("Refunded Count : "+RefundedCount);
                    receiptCount += RefundedCount;
                }

                var gtostring = GTO.ToString("0.00"); // need to deduct the eye checkup 27.52
                var gststring = GST.ToString("0.00"); // need to remove the eye checkup tax 2.48
                var discountstring = Discount.ToString("0.00");
                var servicechargestring = ServiceCharge.ToString("0.00");
                var cashstring = Cash.ToString("0.00");
                var netsstring = NETS.ToString("0.00");
                var visastring = Visa.ToString("0.00");
                var mastercardstring = MasterCard.ToString("0.00");
                var amexstring = Amex.ToString("0.00");
                var voucherstring = Voucher.ToString("0.00");
                var othersstring = Others.ToString("0.00");
                //17000013 | 225 | 15082024 | 15 | 2 | 222.94 | 20.06 | 18.00 | 0.00 | 0 | 0.00 | 0.00 | 0.00 | 0.00 | 0.00 | 0.00 | 222.94 | Y
                var rowstring = string.Join("|", machineId, batchId, previousDateTime.ToString("ddMMyyyy").ToUpper(), hour, receiptCount, gtostring, gststring, discountstring,
                    servicechargestring, NoOfPax, cashstring, netsstring, visastring, mastercardstring, amexstring, voucherstring, othersstring, GSTRegistered);

                Console.WriteLine(rowstring + "\n\n");
                receiptCount = 0;
                Refunded = 0;

                //string directoryPath = @"C:\Users\robby\Desktop\POSSALESINTEGRATION\Test";
                string directoryPath = @"C:\Users\robby\Desktop\POSSALESINTEGRATION\SalesFiles";
                string fileName = $"H{machineId}_{previousDateTime.ToString("yyyyMMdd")}.txt";
                string filePath = Path.Combine(directoryPath, fileName);
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    await writer.WriteLineAsync(rowstring);
                }
            }
        }

        static async Task<List<Order>> GetRefunds(OrderService orderService)
        {
            var refundfilter = new OrderListFilter
            {
                Status = "any",
                UpdatedAtMin = previousDateTime,
                UpdatedAtMax = previousDateTime2,
                FinancialStatus = "refunded"
            };

            var allRefunds = new List<Order>();
            
            var orderRefunds = await orderService.ListAsync(refundfilter);
            foreach (var orderitem in orderRefunds.Items)
            {
                //Console.WriteLine(orderitem.Name);
                allRefunds.Add(orderitem);
            }

            return allRefunds;
        }

        static async Task<string> GetAndUpdateBatchId()
        {
            //string batchIdFilePath = @"C:\Users\robby\Desktop\POSSALESINTEGRATION\BatchIDTest.txt";
            string batchIdFilePath = @"C:\Users\robby\Desktop\POSSALESINTEGRATION\BatchID.txt";
            string batchId;
            int newBatchId;

            if (File.Exists(batchIdFilePath))
            {
                batchId = await File.ReadAllTextAsync(batchIdFilePath);
                newBatchId = int.Parse(batchId) + 1;
                await File.WriteAllTextAsync(batchIdFilePath, newBatchId.ToString());
                batchId = newBatchId.ToString();

            }
            else
            {
                batchId = "1";
                await File.WriteAllTextAsync(batchIdFilePath, batchId);
            }

            return batchId;
        }
    }
}
