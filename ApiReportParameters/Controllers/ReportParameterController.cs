using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using ApiReportParameters.Report;
using Microsoft.Reporting.WebForms;
using System.Net.Http;
using System.Text;
using System.IO;
using ApiReportParameters.Classe;
using System.Web.Http.Cors;


namespace ApiReportParameters.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportParameterController : ApiController
    {

        public HttpResponseMessage Get(long startDate,long endDate)
        {
            try
            {
                if (startDate > endDate)
                    endDate = DateTime.Now.Ticks;

                string ordem = "";
                string codTira = "";

                var localReport = new LocalReport();
                ReportDataSource reportDataSource = null;

                localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Report1.rdlc");

                DataSet1 dbrelatorio = new DataSet1();
                // Pegando a DataTable que será populada
                RestAPI restOtherApi = new RestAPI();

                var dataTable = dbrelatorio.DataTable1;

                var thingList = restOtherApi.ThingGet();

                foreach (var thing in thingList.Where(x => x.position > 0).OrderBy(x => x.position))
                {

                    var bigTable = restOtherApi.BigTableGet(thing.thingId, startDate, endDate);

                    if (bigTable == null || bigTable.tags.Count() <= 1)
                        continue;

                    var bigTableGroup = bigTable.tags.GroupBy(x => x.group);

                    var linhaTags = bigTableGroup.Where(x => x.Key.ToLower() == "linha").FirstOrDefault();
                    var roloTimeStamp = linhaTags.FirstOrDefault().timestamp;
                    var roloValores = linhaTags.Where(x => x.name.ToLower() == "rolo").FirstOrDefault().value;


                    foreach (var group in bigTableGroup)
                    {

                        if (group.Key.ToLower() == "linha")
                        {
                            ordem = group.Where(x => x.name.Contains("ordem")).FirstOrDefault().value.FirstOrDefault();
                            codTira = group.Where(x => x.name.Contains("codTira")).FirstOrDefault().value.FirstOrDefault();
                        }
                        var timeStamp = group.Where(x => x.name.Contains("Valor"));

                        if (timeStamp == null || timeStamp.Count() == 0)
                            continue;
                        //try
                        //{
                        //    if (group.Where(x => x.name.Contains("LSE")).FirstOrDefault().timestamp.Count() < timeStamp.FirstOrDefault().timestamp.Count())
                        //        timeStamp = group.Where(x => x.name.Contains("LSE"));
                        //    if (group.Where(x => x.name.Contains("LSC")).FirstOrDefault().timestamp.Count() < timeStamp.FirstOrDefault().timestamp.Count())
                        //        timeStamp = group.Where(x => x.name.Contains("LSC"));
                        //    if (group.Where(x => x.name.Contains("LIC")).FirstOrDefault().timestamp.Count() < timeStamp.FirstOrDefault().timestamp.Count())
                        //        timeStamp = group.Where(x => x.name.Contains("LIC"));
                        //    if (group.Where(x => x.name.Contains("LIE")).FirstOrDefault().timestamp.Count() < timeStamp.FirstOrDefault().timestamp.Count())
                        //        timeStamp = group.Where(x => x.name.Contains("LIE"));
                        //}
                        //catch
                        //{
                        //    continue;
                        //}
                        for (int i = 0; i < timeStamp.FirstOrDefault().timestamp.Count(); i++)
                        {
                            string sValor = "";
                            string sLSE = "";
                            string sLSC = "";
                            string sLIC = "";
                            string sLIE = "";

                            var valor = group.Where(x => x.name.Contains("Valor"));
                            var LSE = group.Where(x => x.name.Contains("LSE"));
                            var LSC = group.Where(x => x.name.Contains("LSC"));
                            var LIC = group.Where(x => x.name.Contains("LIC"));
                            var LIE = group.Where(x => x.name.Contains("LIE"));

                            if (valor.Count() > 0 && (valor.FirstOrDefault().value.Count - 1) >= i)
                                sValor = valor.FirstOrDefault().value[i];
                            if (LSE.Count() > 0 && (LSE.FirstOrDefault().value.Count - 1) >= i)
                                sLSE = LSE.FirstOrDefault().value[i];
                            if (LSC.Count() > 0 && (LSC.FirstOrDefault().value.Count - 1) >= i)
                                sLSC = LSC.FirstOrDefault().value[i];
                            if (LIC.Count() > 0 && (LIC.FirstOrDefault().value.Count - 1) >= i)
                                sLIC = LIC.FirstOrDefault().value[i];
                            if (LIE.Count() > 0 && (LIE.FirstOrDefault().value.Count - 1) >= i)
                                sLIE = LIE.FirstOrDefault().value[i];


                            var roloIndex = roloTimeStamp.FindIndex(x => x == valor.FirstOrDefault().timestamp[i]);


                            dataTable.AddDataTable1Row("", "", "", roloValores[roloIndex], thing.thingName, group.FirstOrDefault().group, sValor, sLSC
                                , sLSE, sLIC, sLIE, new DateTime(valor.FirstOrDefault().timestamp[i]).ToString("dd/MM/yyyy HH:mm"), thing.position);
                        }
                    }
                }

                if (string.IsNullOrEmpty(ordem))
                    ordem = "";
                if (string.IsNullOrEmpty(codTira))
                    codTira = "";

                localReport.SetParameters(new Microsoft.Reporting.WebForms.ReportParameter("ReportParameter1", ordem));
                localReport.SetParameters(new Microsoft.Reporting.WebForms.ReportParameter("ReportParameter4", codTira));
                localReport.SetParameters(new Microsoft.Reporting.WebForms.ReportParameter("ReportParameter2", new DateTime(startDate).ToString("dd/MM/yyyy HH:mm")));
                localReport.SetParameters(new Microsoft.Reporting.WebForms.ReportParameter("ReportParameter3", new DateTime(endDate).ToString("dd/MM/yyyy HH:mm")));

                reportDataSource = new ReportDataSource("DataSet1", dataTable.ToList());
                localReport.DataSources.Add(reportDataSource);

                //Realizando o Refresh do LocalReport
                localReport.Refresh();

                string reportType = "excel";
                string mimeType;
                string encoding;
                string fileNameExtension;

                //The DeviceInfo settings should be changed based on the reportType            
                //http://msdn2.microsoft.com/en-us/library/ms155397.aspx            
                string deviceInfo = null;

                Warning[] warnings;
                string[] streams;
                byte[] renderedBytes = null;

                var p = localReport.ListRenderingExtensions();

                //Render the report            
                renderedBytes = localReport.Render(
                    reportType,
                    deviceInfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(renderedBytes);
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                return result;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.ToString());
            }
           
        }
    }
}
