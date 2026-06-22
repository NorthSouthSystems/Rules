public class T_AxisMap_ParseValidateAndConstruct
{
    [Fact]
    public void Exceptions()
    {
        Action act;

        act = static () => AxisMap<float>.ParseValidateAndConstructFromTable(
            [AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 0, ["A"])],
            1,
            [1.0f]);
        act.Should().ThrowExactly<NotSupportedException>().WithMessage("TCellValue == System.Single");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [
                .. Enumerable.Range(0, AxisMap.AxesTotalCountMax + 1)
                    .Select(axisIndex =>
                        AxisString.ParseValidateAndConstruct(
                            string.Create(InvariantCulture, $"H{axisIndex}"), true, axisIndex, ["A"]))
                    .Cast<Axis>()
            ],
            0,
            ["A"]); // This would fail the Axis property existence validation.
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("axes.Length");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [
                .. Enumerable.Range(0, AxisMap.AxesOrientationCountMax + 1)
                    .Select(axisIndex =>
                        AxisString.ParseValidateAndConstruct(
                            string.Create(InvariantCulture, $"H{axisIndex}"), true, axisIndex, ["A"]))
                    .Cast<Axis>()
            ],
            0,
            ["A"]); // This would fail the Axis property existence validation.
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Axes count for orientation exceeded*");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 0, ["A"])],
            0,
            [.. Enumerable.Repeat("A", AxisMap.CellValuesCountMax + 1)]); // This would fail the Axis calculation and CellValues count validation.
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("cellValuesRaw.Length");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [
                AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 0, ["A"]),
                AxisBool.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheBool), true, 2, ["true", "false"])
            ],
            0,
            [.. Enumerable.Repeat("A", 2)]);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Axis out of order*");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [
                AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 1, ["A"]),
                AxisBool.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheBool), true, 2, ["true", "false"])
            ],
            0,
            [.. Enumerable.Repeat("A", 2)]);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Axis out of order*");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [
                AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), false, 0, ["A"]),
                AxisBool.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheBool), false, 2, ["true", "false"])
            ],
            0,
            [.. Enumerable.Repeat("A", 2)]);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Axis out of order*");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [
                AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 0, ["A"]),
                AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 1, ["B"])
            ],
            0,
            ["A"]);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Axes must have unique PropertyPaths*");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 0, ["A", "B"])],
            0,
            ["A"]);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Axes calculation and CellValues count disagree*");

        act = static () =>
        {
            var map = AxisMap<string>.ParseValidateAndConstructFromTable(
                [AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheInt), true, 0, ["A"])],
                0,
                ["A"]);

            map.ThrowIfAxesInputTypeMismatches(typeof(T_AxisMapInput));
        };
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Axes Types must match their corresponding inputType's Property's Type*");

        act = static () => AxisMap<string>.ParseValidateAndConstructFromTable(
            [AxisString.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheString), true, 0, ["A"])],
            1,
            ["A"]);
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("cellValueScaleForFormatting");
    }
}