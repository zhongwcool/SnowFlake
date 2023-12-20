using System.Windows.Shapes;

namespace SnowFlake.Models;

public class Snowflake
{
    public Path Shape { get; set; }
    public double Velocity { get; set; }
    public double RtSpeed { get; set; } = 0;
}