using App.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;

namespace App;
class Record(string date, double nominal, double value, double vunitRate)
{
    public string date = date;
    public double nominal = nominal;
    public double value = value;
    public double vunitRate = vunitRate;
}

public partial class StatPage : ContentPage
{
    List<Record> records;
    ChartEntry[] entries;
    Dictionary<string, string> mapNameCode;
    public void DrawChart(string startDate, string finishDate, string? moneyCode /*доллар*/)
    {
        if (moneyCode == null || moneyCode == "") moneyCode = "R01235";
        XmlDocument Prices = new XmlDocument();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // добавляет кодировку windows-1251
        Prices.Load($"https://www.cbr.ru/scripts/XML_dynamic.asp?date_req1={startDate}&date_req2={finishDate}&VAL_NM_RQ={moneyCode}");
        records = [];
        XmlElement xValCurs = Prices.DocumentElement;
        foreach (XmlElement xRecord in xValCurs)
        {
            try
            {
                Record record = new Record(xRecord.GetAttribute("Date"),
                double.Parse(xRecord.SelectSingleNode("Nominal").InnerText),
                double.Parse(xRecord.SelectSingleNode("Value").InnerText),
                double.Parse(xRecord.SelectSingleNode("VunitRate").InnerText)
                );
                records.Add(record);
            }
            catch
            {

            }
        }
        if (records.Count == 0) 
        {
            Application.Current.MainPage.DisplayAlert("Ошибка", "ЦБ не привёл актуальных данных по курсу валюты с кодом "
                + moneyCode, "Ладно");
            return;
        }
        entries = [];

        for (int i = 0; i < records.Count(); i++)
        {
            ChartEntry entry = new ChartEntry((float)records[i].value);
            if (i % (records.Count / 10 + 10) == 0)
            {
                entry.ValueLabel = double.Round(records[i].value,2).ToString();
                entry.Label = records[i].date;
            }
            entry.Color = SKColor.Parse("#77d055");
            entries = entries.Append(entry).ToArray();
        }

        chartView.Chart = new LineChart()
        {
            Entries = entries,
            LineSize = 1,
            LabelColor = SKColor.Parse("#000000"),
            PointSize = 4f,
            LineMode = LineMode.Straight,
            IsAnimated = false
        };
    }
    public StatPage(StatViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        XmlDocument Codes = new XmlDocument();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // добавляет кодировку windows-1251
        Codes.Load($"https://www.cbr.ru/scripts/XML_val.asp?d=0");

        mapNameCode = new Dictionary<string, string>();
        XmlElement xRoot = Codes.DocumentElement;
        foreach (XmlElement xItem in xRoot)
        {
            mapNameCode.TryAdd(xItem.SelectSingleNode("Name").InnerText,
                xItem.SelectSingleNode("ParentCode").InnerText.Replace(" ", ""));
        }
        MoneyType.ItemsSource = new string[] { "заглушка" };
        MoneyType.ItemsSource = mapNameCode.Keys.ToArray();

    }

    private void Selected(object sender, DateChangedEventArgs e)
    {

        DrawChart(Start.Date.Date.ToShortDateString().Replace('.', '/'),
            Finish.Date.Date.ToShortDateString().Replace('.', '/'),
            MoneyType.SelectedItem == null ? null :
            mapNameCode.GetValueOrDefault((string)MoneyType.SelectedItem, "")
            );
    }

    private void MoneyType_SelectedIndexChanged(object sender, EventArgs e)
    {

        DrawChart(Start.Date.Date.ToShortDateString().Replace('.', '/'),
            Finish.Date.Date.ToShortDateString().Replace('.', '/'),
            MoneyType.SelectedItem == null ? null : 
            mapNameCode.GetValueOrDefault((string)MoneyType.SelectedItem,""));
    }
}