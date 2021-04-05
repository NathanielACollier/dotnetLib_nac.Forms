using Avalonia.Media;

namespace nac.Forms.model
{
    public class Style
    {
        public model.Optional<Avalonia.Media.Color> foregroundColor { get; set; } = new Optional<Color>();
        public model.Optional<Avalonia.Media.Color> backgroundColor { get; set; } = new Optional<Color>();
        public model.Optional<int> height { get; set; } = new Optional<int>();
    }
}