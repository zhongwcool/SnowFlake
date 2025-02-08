using System.Windows.Media;
using System.Windows.Shapes;

namespace SnowFlake.Models;

public class Snowflake
{
    public Path Shape { get; set; }
    public double Velocity { get; set; }
    public double RtSpeed { get; set; } = 0;
    public RotateTransform Transform { get; init; } = null!;
}