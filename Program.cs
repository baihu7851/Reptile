using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Reptile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            GetPurchase();
            Console.ReadKey();
        }

        private static void GetPurchase()
        {
            Console.WriteLine("資料處理中...");
            int page = 1;
            List<string> oldKey = new List<string>();
            Model purchaseSave = new Model();
            while (true)
            {
                string path = $"https://web.pcc.gov.tw/tps/pss/tender.do?searchMode=common&searchType=basic&method=search&isSpdt=&pageIndex={page}";
                string req = Tools.GetWebContent(path);
                HtmlDocument purchasedDocument = new HtmlDocument();
                purchasedDocument.LoadHtml(req);
                HtmlNodeCollection loadPurchased = purchasedDocument.DocumentNode.SelectNodes(
                    "/html/body/table/tr[2]/td[2]/table/tr[5]/td/table/tr[3]/td/table/tbody/tr/td/div/table/tr");

                if (loadPurchased[1].SelectNodes("td")[0].InnerText == "無符合條件資料")
                {
                    break;
                }
                for (int j = 2; j < loadPurchased.Count - 1; j++) //單頁數量迴圈
                {
                    HtmlNodeCollection dataNode = loadPurchased[j].SelectNodes("td");
                    GovProcurement govProcurement = new GovProcurement();
                    if (CheckKey(GetKey(ResetString(dataNode[2].InnerHtml))))
                    {
                        continue;
                    }

                    if (!CheckKeyword(dataNode[2].InnerText))
                    {
                        continue;
                    }
                    Console.WriteLine(ResetString(dataNode[2].SelectNodes("a")[0].InnerText));
                    govProcurement.Key = Convert.ToInt32(GetKey(ResetString(dataNode[2].InnerHtml)));
                    govProcurement.AgencNname = ResetString(dataNode[1].InnerText);
                    govProcurement.CaseName = ResetString(dataNode[2].SelectNodes("a")[0].InnerText);
                    govProcurement.Number = Convert.ToInt32(ResetString(dataNode[3].InnerText) ?? "0");
                    govProcurement.TenderMethod = ResetString(dataNode[4].InnerText);
                    govProcurement.purchasingProperty = ResetString(dataNode[5].InnerText);
                    govProcurement.AnnouncementDate = Tools.TaiwanTimeChange(ResetString(dataNode[6].InnerText));
                    govProcurement.Deadine = Tools.TaiwanTimeChange(ResetString(dataNode[7].InnerText));
                    govProcurement.Budget =
                        Convert.ToInt32(ResetString(dataNode[8].InnerText) ?? "0".Replace(",", ""));
                    purchaseSave.GovProcurements.Add(govProcurement);
                }

                page++;
            }
            purchaseSave.SaveChanges();
            Console.WriteLine("資料儲存完成。");

            string ResetString(string oldString)
            {
                string newString = oldString.Replace("&nbsp;", "").Replace("\r\n", "").Replace("\t", "").Trim();
                return newString == "" ? null : newString;
            }

            string GetKey(string key)
            {
                var keyStart = key.IndexOf("primaryKey", StringComparison.Ordinal) + 11;
                var keyEnd = key.IndexOf("title", StringComparison.Ordinal) - 2;
                return key.Substring(keyStart, keyEnd - keyStart);
            }

            bool CheckKey(string key)
            {
                if (oldKey.Count == 0)
                {
                    foreach (var item in purchaseSave.GovProcurements)
                    {
                        oldKey.Add(item.Key.ToString());
                    }
                }

                foreach (string word in oldKey)
                {
                    if (key == word)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CheckKeyword(string caseName)
            {
                string[] keyWord = { "資料", "資訊", "網頁", "網站" };
                foreach (string word in keyWord)
                {
                    if (caseName.Contains(word))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static void SendMail()
        {
            Console.WriteLine("開始寄送信件");
            Model EProcurementRead = new Model();

            MailMessage mail = new MailMessage();
            //寄件者(沒伺服器只能用gmail)
            mail.From = new MailAddress("baihu7851@gmail.com");
            //收件者
            mail.To.Add("s15302136@icloud.com");
            //標題
            mail.Subject = "每日爬取資料";
            //內容
            StringBuilder data = new StringBuilder(@"<html><table><tr>
                    <td>項次</td>
                    <td>機關名稱</td>
                    <td>標案名稱</td>
                    <td>招標方式</td>
                    <td>採購性質</td>
                    <td>公告日期</td>
                    <td>截止投標</td>
                    <td>預算金額</td></tr>");

            foreach (var item in EProcurementRead.GovProcurements)
            {
                if (item.AnnouncementDate <= DateTime.Now)
                {
                    data.Append($@"<tr>
                    <td>{item.Key}</rd>
                    <td>{item.AgencNname}</rd>
                    <td>{item.CaseName}</rd>
                    <td>{item.TenderMethod}</rd>
                    <td>{item.purchasingProperty}</rd>
                    <td>{item.AnnouncementDate}</rd>
                    <td>{item.Deadine}</rd>
                    <td>{item.Budget}</rd></tr>");
                }
            }

            data.Append("</table></html>");
            mail.Body = data.ToString();
            mail.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //設定為gamil
            client.EnableSsl = true; //加密連線
            client.Credentials = new NetworkCredential("baihu7851@gmail.com", "password"); //gamil帳密
            //gmail需設定**低**安全性才可以透過這方式寄送
            try
            {
                client.Send(mail);
                Console.WriteLine("信件已寄送，按任意鍵關閉。");
            }
            catch
            {
                Console.WriteLine("信件寄送失敗，按任意鍵關閉。");
            }
        }
    }
}