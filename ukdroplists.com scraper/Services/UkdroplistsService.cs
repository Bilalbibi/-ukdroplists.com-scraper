using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using scrapingTemplateV51.Models;

namespace scrapingTemplateV51.Services
{
    public static class UkdroplistsService
    {
        public static HttpCaller HttpCaller = new HttpCaller();
        public static async Task<List<Domain>> GetDroppedDomainsFromUkdroplists()
        {
            var date = DateTime.Now;
            var ukdroplists = new List<Domain>();
            var doc = await HttpCaller.GetDoc($@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount=1000&field=mozDa&extra=&searchoption=begins&searchterm=&extrasearch=");
            var droppedDomains = doc.DocumentNode.SelectSingleNode("//script").InnerText.Trim();
            var x = droppedDomains.IndexOf("of", StringComparison.Ordinal) + 3;
            var xx = droppedDomains.IndexOf("domains", StringComparison.Ordinal);
            droppedDomains = droppedDomains.Substring(x, xx - x).Replace(",", "");

            var lenDomains = await UkdroplistsService.GetLenDomains(droppedDomains);
            var moazDomains = await UkdroplistsService.GetMoazDomains(droppedDomains);
            var majisticDomains = await UkdroplistsService.GetMajisticDomains(droppedDomains);
            var cpcAndSearchDomains = await UkdroplistsService.GetCpcAndSearchDomains(droppedDomains);

            if (moazDomains != null)
                ukdroplists.AddRange(moazDomains);

            if (majisticDomains != null)
                ukdroplists.AddRange(majisticDomains);

            if (cpcAndSearchDomains != null)
                ukdroplists.AddRange(cpcAndSearchDomains);

            if (lenDomains != null)
                ukdroplists.AddRange(lenDomains);

            return ukdroplists;
        }
        public static async Task<List<Domain>> GetLenDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=asc&start=0&amount={droppedDomains}&field=domainLength&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedLenDomains = new List<Domain>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            var counter = 0;
            foreach (var tr in trs)
            {
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var len = int.Parse(tr.SelectSingleNode("./td[@data-col='len']").InnerText.Trim());
                if (len > 3 && counter == 0)
                    return null;
                if (len > 3 && counter > 0)
                    return basedLenDomains;
                int freq = domainName.Count(x => x == '.');
                if (domainName.Contains(".co.uk") || (domainName.Contains(".uk") && freq.Equals(1)))
                {
                    var regDateText = tr.SelectSingleNode("./td[@data-col='reg']").InnerText.Trim();
                    var regDate = new DateTime();

                    if (regDateText != "0")
                        regDate = new DateTime(int.Parse(regDateText), 1, 1);

                    var isDigitPresent = domainName.Any(c => char.IsDigit(c));

                    var domain = new Domain();
                    domain.Name = domainName;

                    if (len == 3 && !isDigitPresent && !domainName.Contains("-"))
                    {
                        domain.Type = "4";
                        domain.Priority = "2";
                        domain.Source = "UKDropLists.com: Dictionary Domain (Domain No Numbers or Hyphens!)";

                        var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);

                        if (regDateText != "0" && regDate < dateToCompare)
                        {
                            domain.Priority = "1";
                        }
                        basedLenDomains.Add(domain);
                    }
                    if (len == 2)
                    {
                        domain.Type = "3";
                        domain.Priority = "2";
                        domain.Source = "UKDropLists.com: Dictionary Domain (Numbers in Domain)";

                        var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);

                        if (regDateText != "0" && regDate < dateToCompare)
                        {
                            domain.Priority = "1";
                        }
                        basedLenDomains.Add(domain);
                    }
                    if (len == 1)
                    {
                        domain.Type = "3";
                        domain.Priority = "2";
                        domain.Source = "UKDropLists.com: Dictionary Domain  ";

                        var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);

                        if (regDateText != "0" && regDate < dateToCompare)
                        {
                            domain.Priority = "1";
                        }
                        basedLenDomains.Add(domain);
                    }
                }
                counter++;
            }

            return basedLenDomains;
        }

        public static async Task<List<Domain>> GetMajisticDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount={droppedDomains}&field=majTrustFlow&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedMajisticDomains = new List<Domain>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            int count = 0;
            foreach (var tr in trs)
            {
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var majistic = int.Parse(tr.SelectSingleNode("./td[@data-col='mjtf']").InnerText.Trim());
                if (majistic < 30 && count == 0)
                {
                    return null;
                }
                if (majistic < 30 && count > 0)
                {
                    return basedMajisticDomains;
                }
                var da = int.Parse(tr.SelectSingleNode("./td[@data-col='mzda']").InnerText.Trim());
                var pa = int.Parse(tr.SelectSingleNode("./td[@data-col='mzpa']").InnerText.Trim());
                int freq = domainName.Count(x => x == '.');
                if (domainName.Contains(".co.uk") || (domainName.Contains(".uk") && freq.Equals(1)))
                {
                    if (da > pa && majistic >= 30)
                    {
                        var domains = new Domain();
                        domains.Name = domainName;
                        domains.Priority = "3";
                        domains.Type = "1";
                        domains.Source = "UKDropLists.com: MJTF > 30";
                        basedMajisticDomains.Add(domains);
                        count++;
                    }
                }
            }

            return basedMajisticDomains;
        }

        public static async Task<List<Domain>> GetMoazDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount={droppedDomains}&field=mozDa&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedMoazDomains = new List<Domain>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            var count = 0;
            foreach (var tr in trs)
            {
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var da = int.Parse(tr.SelectSingleNode("./td[@data-col='mzda']").InnerText.Trim());
                var pa = int.Parse(tr.SelectSingleNode("./td[@data-col='mzpa']").InnerText.Trim());
                if (da < 35 && count == 0)
                {
                    return null;
                }
                if (da < 35 && count > 0)
                {
                    return basedMoazDomains;
                }
                int freq = domainName.Count(x => x == '.');
                if (domainName.Contains(".co.uk") || (domainName.Contains(".uk") && freq.Equals(1)))
                {
                    if (da >= 50 && da > pa)
                    {
                        var domains = new Domain();
                        domains.Name = domainName;
                        domains.Priority = "1";
                        domains.Type = "1";
                        domains.Source = "UKDropLists.com: SEO Domain (Moz > 50)";
                        basedMoazDomains.Add(domains);
                        count++;
                    }

                    if (da < 50 && da >= 35 && da > pa)
                    {
                        var domains = new Domain();
                        domains.Name = domainName;
                        domains.Priority = "2";
                        domains.Type = "1";
                        domains.Source = "UKDropLists.com: SEO Domain (Moz > 35 , Moz < 50)";
                        basedMoazDomains.Add(domains);
                        count++;
                    }
                }
            }

            return basedMoazDomains;
        }

        public static async Task<List<Domain>> GetCpcAndSearchDomains(string droppedDomains)
        {
            var url = $@"https://ukdroplists.com/getDailyDroppingContents.php?ttype=nstats&searchtype=domain&dropdate=all&order=desc&start=0&amount={droppedDomains}&field=cpc&extra=&searchoption=begins&searchterm=&extrasearch=";
            var doc = await HttpCaller.GetDoc(url);
            var basedcpcAndSearchDomain = new List<Domain>();
            var trs = doc.DocumentNode.SelectNodes("//tr");
            var count = 0;
            foreach (var tr in trs)
            {
                var domainName = tr.SelectSingleNode("./td[@class='sticky-col first-col']").InnerText.Trim();
                var cpc = double.Parse(tr.SelectSingleNode("./td[@data-col='cpc']").InnerText.Trim());
                if (cpc < 20 && count == 0)
                {
                    return null;
                }
                if (cpc < 20 && count > 0)
                {
                    return basedcpcAndSearchDomain;
                }
                var search = int.Parse(tr.SelectSingleNode("./td[@data-col='srch']").InnerText.Trim());
                var regDateText = tr.SelectSingleNode("./td[@data-col='reg']").InnerText.Trim();
                var regDate = new DateTime();
                var dateToCompare = new DateTime(int.Parse("2005"), 1, 1);
                if (regDateText != "0")
                {
                    regDate = new DateTime(int.Parse(regDateText), 1, 1);
                }
                int freq = domainName.Count(x => x == '.');
                if (domainName.Contains(".co.uk") || (domainName.Contains(".uk") && freq.Equals(1)))
                {
                    var domains = new Domain();
                    if (cpc >= 20 && search >= 500)
                    {
                        domains.Name = domainName;
                        domains.Priority = "2";
                        domains.Type = "1";
                        domains.Source = "UKDropLists.com: cpc > 20 and search > 500";
                    }

                    if (regDateText != "0" && regDate < dateToCompare)
                    {
                        domains.Priority = "1";
                    }

                    basedcpcAndSearchDomain.Add(domains);
                    count++;

                }
            }

            return basedcpcAndSearchDomain;
        }
    }
}