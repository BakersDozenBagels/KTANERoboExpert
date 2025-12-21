using KTANERoboExpert.Uncertain;
using static KTANERoboExpert.Edgework;

namespace KTANERoboExpert;

/// <summary>Represents a bomb's edgework. Any uncertain values have not yet been entered by the user.</summary>
/// <param name="SerialNumber">The bomb's serial number as a string.</param>
/// <param name="Batteries">The bomb's battery count.</param>
/// <param name="BatteryHolders">The bomb's battery holder count.</param>
/// <param name="Indicators">The bomb's indicators.</param>
/// <param name="PortPlates">The bomb's port plates.</param>
/// <param name="Strikes">The number of strikes on the bomb.</param>
public record Edgework(
    Uncertain<string> SerialNumber,
    UncertainInt Batteries,
    UncertainInt BatteryHolders,
    UncertainEnumerable<Indicator> Indicators,
    UncertainEnumerable<PortPlate> PortPlates,
    int Strikes,
    UncertainInt Solves,
    UncertainInt SolvableModuleCount,
    UncertainInt NeedyModuleCount,
    UncertainInt WidgetCount)
{
    /// <summary>The bomb's battery count.</summary>
    public UncertainInt Batteries
    {
        get
        {
            if (field.IsCertain)
                return field;
            return BatteryHolders * UncertainInt.InRange(1, 2, field.Fill);
        }
        init;
    } = Batteries;

    internal UncertainInt _batteryHolders { get => field.ButWithinRange(0, WidgetCount); init => field = value; } = BatteryHolders;
    /// <summary>The bomb's battery holder count.</summary>
    public UncertainInt BatteryHolders => _batteryHolders.Coalesce(WidgetCount - (_indicatorCount + _portPlateCount).ButAtMost(WidgetCount));

    internal UncertainEnumerable<Indicator> _indicators { get; init; } = Indicators;
    /// <summary>The bomb's indicators.</summary>
    public UncertainEnumerable<Indicator> Indicators { get => _indicators.IsCertain ? _indicators : UncertainEnumerable<Indicator>.Of(_indicators.Fill, 0, IndicatorCount.Max); }
    private UncertainInt _indicatorCount => _indicators.Count.Coalesce(UncertainInt.AtLeast(0, Indicators.Fill).ButAtMost(WidgetCount));
    private UncertainInt IndicatorCount => _indicators.Count.Coalesce(WidgetCount - (_batteryHolders + _portPlateCount).ButAtMost(WidgetCount));

    internal UncertainEnumerable<PortPlate> _ports { get; init; } = PortPlates;
    /// <summary>The bomb's port plates.</summary>
    public UncertainEnumerable<PortPlate> PortPlates { get => _ports.IsCertain ? _ports : UncertainEnumerable<PortPlate>.Of(_ports.Fill, 0, PortPlateCount.Max); }

    private UncertainInt _portPlateCount => _ports.Count.Coalesce(UncertainInt.AtLeast(0, _ports.Fill).ButAtMost(WidgetCount));
    private UncertainInt PortPlateCount => _ports.Count.Coalesce(WidgetCount - (_batteryHolders + _indicatorCount).ButAtMost(WidgetCount));

    public UncertainInt PortCount => PortPlates.Map(pl => pl.Sum(CountPlate)).Into().Coalesce(PortPlateCount * UncertainInt.InRange(0, 4, PortPlates.Fill));
    /// <summary>The bomb's distinct ports.</summary>
    public UncertainEnumerable<PortType> PortTypes => UncertainEnumerable<PortType>.Of(Enum.GetValues<PortType>()).Where(p => PortPlates.Count(l => l.HasPort(p)) > 0);
    /// <summary>The bomb's ports.</summary>
    public UncertainEnumerable<PortType> Ports =>
        PortPlates.IsCertain
            ? UncertainEnumerable<PortType>.Of(PortPlates.Value.SelectMany(l => Enum.GetValues<PortType>().Where(p => l.HasPort(p))))
            : UncertainEnumerable<PortType>.Of(PortPlates.Fill, 0, Enum.GetValues<PortType>().Length * PortPlates.Count.Max);

    /// <summary>The bomb's AA battery count.</summary>
    public UncertainInt AABatteries => (2 * (Batteries - BatteryHolders)).ButWithinRange(0, Batteries);
    /// <summary>The bomb's D battery count.</summary>
    public UncertainInt DBatteries => (2 * BatteryHolders - Batteries).ButWithinRange(0, Batteries);

    public UncertainInt TotalModuleCount => SolvableModuleCount + NeedyModuleCount;

    private static int CountPlate(PortPlate pl)
    {
        int c = 0;
        if (pl.DVID) c++;
        if (pl.Parallel) c++;
        if (pl.PS2) c++;
        if (pl.RJ45) c++;
        if (pl.Serial) c++;
        if (pl.StereoRCA) c++;
        return c;
    }

    /// <summary>Represents an indicator.</summary>
    /// <param name="Label">The indicator's label.</param>
    /// <param name="Lit"><see langword="true"/> if the indicator is lit, <see langword="false"/> otherwise.</param>
    public record struct Indicator(string Label, bool Lit);

    /// <summary>Represents a port plate.</summary>
    /// <param name="DVID"><see langword="true"/> if the port plate has a DVI-D port, <see langword="false"/> otherwise.</param>
    /// <param name="Parallel"><see langword="true"/> if the port plate has a parallel port, <see langword="false"/> otherwise.</param>
    /// <param name="PS2"><see langword="true"/> if the port plate has a PS/2 port, <see langword="false"/> otherwise.</param>
    /// <param name="RJ45"><see langword="true"/> if the port plate has a RJ-45 port, <see langword="false"/> otherwise.</param>
    /// <param name="Serial"><see langword="true"/> if the port plate has a serial port, <see langword="false"/> otherwise.</param>
    /// <param name="StereoRCA"><see langword="true"/> if the port plate has a stereo RCA port, <see langword="false"/> otherwise.</param>
    public record struct PortPlate(bool DVID, bool Parallel, bool PS2, bool RJ45, bool Serial, bool StereoRCA)
    {
        public readonly bool HasPort(PortType p)
        {
            return p switch
            {
                PortType.DVID => DVID,
                PortType.Parallel => Parallel,
                PortType.PS2 => PS2,
                PortType.RJ45 => RJ45,
                PortType.Serial => Serial,
                PortType.StereoRCA => StereoRCA,
                _ => throw new ArgumentException("Illegal port type", nameof(p))
            };
        }
    }

    /// <summary>Represents a type of port.</summary>
    public enum PortType
    {
        DVID, Parallel, PS2, RJ45, Serial, StereoRCA
    }
}