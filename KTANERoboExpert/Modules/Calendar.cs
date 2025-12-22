using System.Diagnostics;
using System.Speech.Recognition;
using KTANERoboExpert.Uncertain;

namespace KTANERoboExpert.Modules;

public class Calendar : RoboExpertModule
{
    public override string Name => "Calendar";
    public override string Help => "Green March 17th (first day if range)";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new GrammarBuilder(new Choices("Green", "Yellow", "Red", "Blue")) + new Choices("April 1st", "January 26th", "July 14th", "December 24th", "May 5th", "October 3rd", "October 31st", "April 22nd", "January 6th", "April 29th", "February 2nd", "November 5th", "December 26th", "June 2nd", "March 17th", "February 14th", "November 11th", "January 4th"));

    public override void ProcessCommand(string command)
    {
        var parts = command.Split(' ');


        var month = (DateTime.Now.Month, DateTime.Now.Day) switch
        {
            (3, >= 22) or (4 or 5, _) or (6, <= 21) => (parts[0], DateTime.Now.Day) switch
            {
                ("Green", <= 10) => "January",
                ("Yellow", <= 10) => "December",
                ("Red", <= 10) => "February",
                ("Blue", <= 10) => "May",
                ("Green", >= 11 and <= 20) => "November",
                ("Yellow", >= 11 and <= 20) => "June",
                ("Red", >= 11 and <= 20) => "July",
                ("Blue", >= 11 and <= 20) => "March",
                ("Green", >= 21) => "August",
                ("Yellow", >= 21) => "April",
                ("Red", >= 21) => "October",
                ("Blue", >= 21) => "September",
                _ => throw new UnreachableException(),
            },
            (6, >= 22) or (7 or 8, _) or (9, <= 21) => (parts[0], DateTime.Now.Day) switch
            {
                ("Green", <= 10) => "June",
                ("Yellow", <= 10) => "October",
                ("Red", <= 10) => "January",
                ("Blue", <= 10) => "April",
                ("Green", >= 11 and <= 20) => "March",
                ("Yellow", >= 11 and <= 20) => "May",
                ("Red", >= 11 and <= 20) => "September",
                ("Blue", >= 11 and <= 20) => "July",
                ("Green", >= 21) => "December",
                ("Yellow", >= 21) => "February",
                ("Red", >= 21) => "August",
                ("Blue", >= 21) => "November",
                _ => throw new UnreachableException(),
            },
            (9, >= 22) or (10 or 11, _) or (12, <= 21) => (parts[0], DateTime.Now.Day) switch
            {
                ("Green", <= 10) => "February",
                ("Yellow", <= 10) => "August",
                ("Red", <= 10) => "December",
                ("Blue", <= 10) => "June",
                ("Green", >= 11 and <= 20) => "July",
                ("Yellow", >= 11 and <= 20) => "November",
                ("Red", >= 11 and <= 20) => "April",
                ("Blue", >= 11 and <= 20) => "October",
                ("Green", >= 21) => "September",
                ("Yellow", >= 21) => "March",
                ("Red", >= 21) => "July",
                ("Blue", >= 21) => "January",
                _ => throw new UnreachableException(),
            },
            (12, >= 22) or (1 or 2, _) or (3, <= 21) => (parts[0], DateTime.Now.Day) switch
            {
                ("Green", <= 10) => "May",
                ("Yellow", <= 10) => "July",
                ("Red", <= 10) => "March",
                ("Blue", <= 10) => "December",
                ("Green", >= 11 and <= 20) => "October",
                ("Yellow", >= 11 and <= 20) => "January",
                ("Red", >= 11 and <= 20) => "November",
                ("Blue", >= 11 and <= 20) => "August",
                ("Green", >= 21) => "April",
                ("Yellow", >= 21) => "September",
                ("Red", >= 21) => "June",
                ("Blue", >= 21) => "February",
                _ => throw new UnreachableException(),
            },
            _ => throw new UnreachableException(),
        };

        var day = (parts[1] + " " + parts[2]) switch
        {
            "April 1st" => Edgework.SerialNumberDigits()[0].Map(d => ((string[])["1", "2", "3", "4", "5", "6", "7", "8", "9", "10"])[d]),
            "January 26th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["19", "5", "24", "3", "29 or 1", "28", "18", "30 or 4", "13", "12"])[d]),
            "July 14th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["22", "14", "6", "11", "8", "19", "31 or 7", "23", "28", "26"])[d]),
            "December 24th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["12", "2", "11", "7", "18", "24", "4", "14", "10", "20"])[d]),
            "May 5th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["29 or 3", "19", "27", "15", "9", "16", "19", "14", "9", "3"])[d]),
            "October 3rd" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["4", "27", "8", "22", "10", "14", "13", "28", "13", "21"])[d]),
            "October 31st" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["4", "16", "21", "15", "27", "6", "25", "13", "2", "9"])[d]),
            "April 22nd" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["23", "13", "25", "30 or 3", "4", "11", "27", "15", "21", "31 or 5"])[d]),
            "January 6th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["15", "1", "31 or 7", "17", "26", "30 or 8", "24", "9", "3", "25"])[d]),
            "April 29th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["8", "20", "17", "16", "23", "16", "1", "22", "24", "5"])[d]),
            "February 2nd" => Uncertain<string>.Of("any day 3 times"),
            "November 5th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["26", "16", "3", "26", "29 or 7", "18", "22", "25", "17", "11"])[d]),
            "December 26th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["21", "9", "30 or 6", "24", "28", "6", "21", "26", "31 or 2", "8"])[d]),
            "June 2nd" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["10", "29 or 2", "12", "24", "15", "20", "5", "27", "25", "7"])[d]),
            "March 17th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["2", "28", "18", "13", "21", "12", "3", "10", "20", "1"])[d]),
            "February 14th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["11", "6", "22", "14", "19", "27", "20", "7", "16", "23"])[d]),
            "November 11th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["14", "7", "23", "17", "5", "31 or 1", "2", "25", "17", "11"])[d]),
            "January 4th" => Edgework.SerialNumberDigits()[^1].Map(d => ((string[])["17", "24", "15", "20", "1", "30 or 9", "28", "6", "7", "14"])[d]),
            _ => throw new UnreachableException(),
        };

        if (!day.IsCertain)
        {
            day.Fill(() => ProcessCommand(command), ExitSubmenu);
            return;
        }

        Speak(month + " " + day.Value);
        ExitSubmenu();
        Solve();
    }
}
