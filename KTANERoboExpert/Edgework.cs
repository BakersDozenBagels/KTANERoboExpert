using KTANERoboExpert.Uncertain;
using static KTANERoboExpert.Edgework;

namespace KTANERoboExpert;

/// <summary>
/// Represents a bomb's edgework. Any null values have not yet been entered by the user.
/// </summary>
/// <param name="SerialNumber">The bomb's serial number as a string.</param>
/// <param name="Batteries">The bomb's battery count.</param>
/// <param name="BatteryHolders">The bomb's battery holder count.</param>
/// <param name="Indicators">The bomb's indicators.</param>
/// <param name="Ports">The bomb's port plates.</param>
/// <param name="Strikes">The number of strikes on the bomb.</param>
public record Edgework(Uncertain<string> SerialNumber, UncertainInt Batteries, UncertainInt BatteryHolders, Uncertain<IReadOnlyCollection<Indicator>> Indicators, Uncertain<IReadOnlyCollection<PortPlate>> Ports, int Strikes)
{
    /// <summary>
    /// Represents an indicator.
    /// </summary>
    /// <param name="Label">The indicator's label.</param>
    /// <param name="Lit"><see langword="true"/> if the indicator is lit, <see langword="false"/> otherwise.</param>
    public record struct Indicator(string Label, bool Lit);

    /// <summary>
    /// Represents a port plate.
    /// </summary>
    /// <param name="DVID"><see langword="true"/> if the port plate has a DVI-D port, <see langword="false"/> otherwise.</param>
    /// <param name="Parallel"><see langword="true"/> if the port plate has a parallel port, <see langword="false"/> otherwise.</param>
    /// <param name="PS2"><see langword="true"/> if the port plate has a PS/2 port, <see langword="false"/> otherwise.</param>
    /// <param name="RJ45"><see langword="true"/> if the port plate has a RJ-45 port, <see langword="false"/> otherwise.</param>
    /// <param name="Serial"><see langword="true"/> if the port plate has a serial port, <see langword="false"/> otherwise.</param>
    /// <param name="StereoRCA"><see langword="true"/> if the port plate has a stereo RCA port, <see langword="false"/> otherwise.</param>
    public record struct PortPlate(bool DVID, bool Parallel, bool PS2, bool RJ45, bool Serial, bool StereoRCA);
}