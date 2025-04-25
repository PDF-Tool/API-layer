namespace Logic;

public class InputService
{

    private enum ByteUnits
    {
        GB,
        MB
    };

    private enum MetricUnits
    {
        mm,
        cm
    }

    public (int? Pages, int? Size, string? ByteUnit, string? MetricUnit, string? ErrorMessage)
    CleanseInputs(int? Pages, int? Size, string? ByteUnit, string? MetricUnit)
    {
        // Cleanse Pages
        Pages = CheckIfNullOrHigherThanZero(Pages);

        // Check Size
        if (!Size.HasValue || Size.Value == 0)
        {
            return (null, null, null, null, "No size given");
        }

        // Check ByteUnit
        ByteUnit = CheckByteUnit(ByteUnit);
        if (ByteUnit == null)
        {
            return (null, null, null, null, "Byte unit specified is invalid or missing, only MB and GB are allowed");
        }

        // Check MetricUnit
        MetricUnit = CheckMetricUnit(MetricUnit);
        if (MetricUnit == null)
        {
            return (null, null, null, null, "Metric unit specified is invalid, only mm and cm are allowed");
        }

        // Everything is valid
        return (Pages, Size, ByteUnit, MetricUnit, null);
    }


    private int CheckIfNullOrHigherThanZero(int? value)
    {
        if (value.HasValue && value.Value > 0)
        {
            return value.Value;
        }
        else
        {
            return 1;
        }
    }

    private string? CheckByteUnit(string? ByteUnit)
    {
        if (string.IsNullOrWhiteSpace(ByteUnit))
        {
            return ByteUnits.MB.ToString();
        }
        else
        {
            ByteUnit = ByteUnit.Trim().ToUpper();
            if (Enum.IsDefined(typeof(ByteUnits), ByteUnit) && !int.TryParse(ByteUnit, out _))
            {
                return ByteUnit;
            }
            else
            {
                return null;
            }
        }
    }

    private string? CheckMetricUnit(string? MetricUnit)
    {
        if (string.IsNullOrWhiteSpace(MetricUnit))
        {
            return MetricUnits.mm.ToString();
        }
        else
        {
            MetricUnit = MetricUnit.Trim().ToLower();
            if (Enum.IsDefined(typeof(MetricUnits), MetricUnit) && !int.TryParse(MetricUnit, out _))
            {
                return MetricUnit;
            }
            else
            {
                return null;
            }
        }
    }
}