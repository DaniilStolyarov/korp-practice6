using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Xml;
namespace App.ViewModel;

public partial class CountOfWorkingWorkers(string department, int totalCount, int count, string listOfWorks) : ObservableObject
{
    [ObservableProperty]
    int totalCount = totalCount;
    [ObservableProperty]
    string department = department;
    [ObservableProperty]
    int count = count;
    [ObservableProperty]
    string listOfWorks = listOfWorks;
    [ObservableProperty]
    double activity;
}

public partial class SecondViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<Worker> workers;

    [ObservableProperty]
    ObservableCollection<CountOfWorkingWorkers> cowList;

    [ObservableProperty]
    string workersThatWorkOnMultipleJobs;
    [ObservableProperty]
    string departmentsThatHaveMultipleWorkers;
    [ObservableProperty]
    string yearSummonMax;
    [ObservableProperty]
    string yearFiredMin;
    [ObservableProperty]
    string pWorkers;
    public SecondViewModel()
    {
        Workers = AppGlobals.Workers;
        cowList = [];
        var workerInfo_List =
            from worker in Workers
            from workerInfo in worker.WorkExperience
            select new
            {
                workerInfo,
                worker
            };
        var workerInfo_byDepartment_List =
            from wil in workerInfo_List
            group wil by wil.workerInfo.Department;

        var res1 =
            workerInfo_byDepartment_List.Select(info =>
            new CountOfWorkingWorkers
            (
                department : info.Key,
                totalCount : info.DistinctBy(i => i.worker.Name).Count(),
                count : info.Where(i => i.workerInfo.Finish == "").DistinctBy(i => i.worker).Count(),
                listOfWorks : String.Join(", ", info.Select(i => i.workerInfo.Name).Distinct().ToArray())
            ));
        foreach (var c in res1)
        {
            c.Activity = (double)c.Count / c.TotalCount;
            CowList.Add(c);
        }

        // #3
        WorkersThatWorkOnMultipleJobs = String.Join(", ", workerInfo_List.GroupBy(wil => wil.worker.Name).Select(w => new
        {
            Name = w.Key,
            DepsCount = w.Where(wentry => wentry.workerInfo.Finish == "").DistinctBy(wentry => wentry.workerInfo.Department).Count()
        }).Where(r => r.DepsCount > 1).Select(r => r.Name).ToArray());

        // #4
        DepartmentsThatHaveMultipleWorkers = String.Join(", ", workerInfo_List.GroupBy(wil => wil.workerInfo.Department).Select(w => new
        {
            Department = w.Key,
            WorkersCount = w.Where(wentry => wentry.workerInfo.Finish == "").DistinctBy(wentry => wentry.worker.Name).Count()
        }).Where(r => r.WorkersCount <= 3).Select(r => r.Department).ToArray());
        // #5
        YearSummonMax = workerInfo_List.GroupBy(wil => wil.workerInfo.Start).Select(w => new
        {
            Year = w.Key,
            WorkersCount = w.DistinctBy(wentry => wentry.worker.Name).Count()
        }).OrderByDescending(r => r.WorkersCount).First().Year.ToString();
        YearFiredMin = workerInfo_List.GroupBy(wil => wil.workerInfo.Finish).Select(w => new
        {
            Year = w.Key,
            WorkersCount = w.Where(wentry => wentry.workerInfo.Finish != "").DistinctBy(wentry => wentry.worker.Name).Count()
        }).OrderByDescending(r => r.WorkersCount).First().Year.ToString();
        //6
        int currentYear = DateTime.Now.Year;
        PWorkers = String.Join(", ", (from worker in Workers
                                      where (currentYear - int.Parse(worker.BirthString)) % 10 == 0
                                      select worker).Select(w => new { Name = w.Name, Birth = w.BirthString }));

        //7
        var res2 = workerInfo_byDepartment_List.Select(info =>
            new
            {
                department = info.Key,
                totalCount = info.DistinctBy(i => i.worker.Name).Count(),
                countYoung = info.DistinctBy(i => i.worker.Name).Where(i => (currentYear - int.Parse(i.worker.BirthString)) < 30).Count()
            });
        
        XmlDocument res = new XmlDocument();
        XmlElement xDepartments = res.CreateElement("Отделы");
        foreach (var item in res2)
        {
            XmlElement xDepartment = res.CreateElement("Отдел");
            xDepartment.SetAttribute("Название", item.department);
            XmlElement xCount = res.CreateElement("Количество_работающих_сотрудников");
                xCount.InnerText = item.totalCount.ToString();
            XmlElement xCountYoung = res.CreateElement("Количество_работающих_сотрудников_молодёжь");
                xCountYoung.InnerText = item.countYoung.ToString();
            xDepartment.AppendChild(xCount);
            xDepartment.AppendChild(xCountYoung);
            xDepartments.AppendChild(xDepartment);
        }
        res.AppendChild(xDepartments);
        SaveToFile("res7.xml", res);
    }

    public async Task SaveToFile(string filename, XmlDocument doc)
    {
        // Create an output filename
        try
        {
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            doc.Save(targetFile);
            Application.Current.MainPage.DisplayAlert("Путь к файлу", targetFile, "Копировать в буфер");
            await Clipboard.Default.SetTextAsync(targetFile);
        }
        catch (Exception ex)
        {
            Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "Ладно");
        }
    }

}
